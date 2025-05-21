
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
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
    private List<PomodoroTask> _tasks;
    private ObservableCollection<TaskDeadline> _deadlines = new();

    private bool _isAddMenuOpen = false;
    private double _addMenuTranslationY;
    private double _addMenuOpacity = 0;
    private int _selectedTaskIndex;
    private int _deadlineDate;
    private string _plannedTime;

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

    public List<PomodoroTask> Tasks { 
        get => _tasks;
        set { _tasks = value; OnPropertyChanged(); } 
    }

    public int DeadlineDate { 
        get => _deadlineDate;
        set { _deadlineDate = value; OnPropertyChanged();}
    }

    public string PlannedTime { 
        get => _plannedTime;
        set { _plannedTime = value; OnPropertyChanged(); } 
    }
    // === Команды ===
    public ICommand LoadTasksCommand { get; }
    
    
    public ICommand ShowAddMenuCommand { get; }
    public ICommand HideAddMenuCommand { get; }
    public ICommand ConfirmAddTaskCommand { get; }



    

    // === Конструктор ===
    public PurposesViewModel()
    {
        

        // Инициализация команд
        LoadTasksCommand = new Command(async () => await LoadTasksAsync());
        
        ShowAddMenuCommand = new Command(async () => await ShowAddMenuAsync());
        HideAddMenuCommand = new Command(async () => await HideAddMenuAsync());
        ConfirmAddTaskCommand = new Command(async () => await ConfirmAddDeadLineAsync());
        

        LoadTasksCommand.Execute(null);
    }



    // === Методы ===
    private async Task LoadTasksAsync()
    {
        Tasks.Clear();
        var all = await App.Database.GetAllTasksAsync();
        foreach (var task in all) Tasks.Add(task);
    }
    private async Task ShowAddMenuAsync()
    {
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
            || !double.TryParse(PlannedTime, out double hours))
        {
            //await DisplayAlert("Ошибка", "Заполните все поля корректно", "OK");
            return;
        }

        const double maxHours = int.MaxValue / 3600000.0; // ≈596.5
        if (hours > maxHours)
        {
            //await DisplayAlert(
            //    "Ошибка",
            //    $"Слишком большое время (максимум {maxHours:F1} ч).",
            //    "OK");
            return;
        }

        var picked = Tasks[SelectedTaskIndex];
        var plannedMs = (int)(hours * 3_600_000);

        var item = new TaskDeadline
        {
            TaskId = picked.Id,
            PlannedTime = plannedMs,
            DeadlineData = DeadlineDate.ToString("o"),
            InitialTotalTime = picked.TotalWorkTime
        };

        
    }
}
