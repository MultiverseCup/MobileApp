
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using PomodoroProject.Data;
using PomodoroProject.Data.Models;


namespace PomodoroProject.ViewModels;

public partial class PurposesViewModel : INotifyPropertyChanged
{
    // === События ===
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // === Поля ===
    

    private List<PomodoroTask> _tasks = new();
    private ObservableCollection<TaskDeadline> _deadlines = new();

    private readonly Func<TaskDeadline, Task<bool>> _confirmDelete;
    private bool _isAddMenuOpen = false;
    private double _addMenuTranslationY;
    private double _addMenuOpacity = 0;
    private string _newDeadlineName;
    private int _selectedTaskIndex;
    private DateTime _deadlineDate;
    private string _newPlannedTime;


    

    // === Свойства ===
    public ObservableCollection<TaskDeadline> Deadlines
    {
        get => _deadlines;
        set { _deadlines = value; OnPropertyChanged(); }
    }

    public bool IsAddMenuOpen
    {
        get => _isAddMenuOpen;
        set { _isAddMenuOpen = value; OnPropertyChanged(); }
    }

    public double AddMenuTranslationY
    {
        get => _addMenuTranslationY;
        set { _addMenuTranslationY = value; OnPropertyChanged(); }
    }

    public double AddMenuOpacity
    {
        get => _addMenuOpacity;
        set { _addMenuOpacity = value; OnPropertyChanged(); }
    }

    public int SelectedTaskIndex
    {
        get => _selectedTaskIndex;
        set
        {
            _selectedTaskIndex = value; OnPropertyChanged();
        }
    }

    public List<PomodoroTask> Tasks
    {
        get => _tasks;
        set { _tasks = value; OnPropertyChanged(); }
    }

    public DateTime DeadlineDate
    {
        get => _deadlineDate;
        set { _deadlineDate = value; OnPropertyChanged(); }
    }

    public string NewPlannedTime
    {
        get => _newPlannedTime;
        set { _newPlannedTime = value; OnPropertyChanged(); }
    }
    public List<string> TasksNames => _tasks.Select(x => x.Name).ToList();

    public string NewDeadlineName
    {
        get => _newDeadlineName;
        set { _newDeadlineName = value; OnPropertyChanged(); }
    }




    // === Команды ===
    public ICommand LoadTasksCommand { get; }
    public ICommand LoadDeadlinesCommand { get; }

    public ICommand ShowAddMenuCommand { get; }
    public ICommand HideAddMenuCommand { get; }
    public ICommand ConfirmAddDeadlineCommand { get; }
    public ICommand DeleteDeadlineCommand { get; }






    // === Конструктор ===
    public PurposesViewModel(Func<TaskDeadline, Task<bool>> confirmDelete)
    {
        MessagingCenter.Subscribe<object, PomodoroTask>(
            this,     // Подписчик (обычно `this`)
            "DataUpdated",
            async (sender, task) =>
            {
                foreach(var d in Deadlines)
                {
                    if (d.TaskId == task.Id)
                    {
                        d.ElapsedTotalTime += task.TotalWorkTime - d.InitialTotalTime;
                        d.InitialTotalTime = task.TotalWorkTime;
                        await App.Database.SaveDeadlineAsync(d);
                    }
                }
            }
        );


        _confirmDelete = confirmDelete;
        // Инициализация команд
        LoadTasksCommand = new Command(async () => await LoadTasksAsync());
        LoadDeadlinesCommand = new Command(async () => await LoadDeadlinesAsync());

        ShowAddMenuCommand = new Command(async () => await ShowAddMenuAsync());
        HideAddMenuCommand = new Command(async () => await HideAddMenuAsync());
        ConfirmAddDeadlineCommand = new Command(async () => await ConfirmAddDeadLineAsync());
        DeleteDeadlineCommand = new Command<TaskDeadline>(async deadline => await OnDeleteDeadlineAsync(deadline));



        LoadTasksCommand.Execute(null);
        LoadDeadlinesCommand.Execute(null);
        
    }



    // === Методы ===
    private async Task LoadTasksAsync()
    {
        Tasks.Clear();
        var all = await App.Database.GetAllTasksAsync();
        foreach (var task in all) Tasks.Add(task);
    }

    private async Task LoadDeadlinesAsync()
    {
        Deadlines.Clear();
        var all = await App.Database.GetAllDeadlineAsync();
        foreach (var task in all) Deadlines.Add(task);
        
    }

    private async Task ShowAddMenuAsync()
    {
        await LoadTasksAsync();
        OnPropertyChanged(nameof(TasksNames));

        IsAddMenuOpen = true;
        AddMenuTranslationY = 600;
        AddMenuOpacity = 0;
        await Task.Delay(1);
        for (double t = 0; t <= 1.0; t += 0.1)
        {
            AddMenuTranslationY = 600 * (1 - t);
            AddMenuOpacity = 0.8 * t;
            await Task.Delay(16);
        }
    }

    private async Task HideAddMenuAsync()
    {
        for (double t = 1; t >= 0; t -= 0.1)
        {
            AddMenuTranslationY = 600 * (1 - t);
            AddMenuOpacity = 0.8 * t;
            await Task.Delay(16);
        }
        IsAddMenuOpen = false;
    }
    private async Task ConfirmAddDeadLineAsync()
    {
        if (SelectedTaskIndex < 0
            || !double.TryParse(NewPlannedTime, out double hours))
        {
            return;
        }

        const double maxHours = int.MaxValue / 3600000.0; // ≈596.5
        if (hours > maxHours)
        {
            return;
        }

        var picked = Tasks[SelectedTaskIndex];
        var plannedMs = (int)(hours * 3_600_000);

        var item = new TaskDeadline
        {
            TaskId = picked.Id,
            PlannedTime = plannedMs,
            Deadline = DeadlineDate,
            InitialTotalTime = picked.TotalWorkTime,
            DeadlineName = NewDeadlineName,
            TaskName = picked.Name
        };

        await App.Database.SaveDeadlineAsync(item);
        Deadlines.Add(item);

        await HideAddMenuAsync();
    }
    private async Task OnDeleteDeadlineAsync(TaskDeadline deadline)
    {
        if (deadline == null) return;

        bool confirmed = await _confirmDelete(deadline);
        if (!confirmed) return;

        await App.Database.DeleteDeadlineAsync(deadline.Id);
        Deadlines.Remove(deadline);
    }
}
