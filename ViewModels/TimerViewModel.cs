using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PomodoroProject.Data.Models;
using PomodoroProject.Data;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Plugin.Maui.Audio;
using System.Diagnostics;
using System.Text.Json;

namespace PomodoroProject.ViewModels;

public partial class TimerViewModel : INotifyPropertyChanged
{
    // === События ===
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // === Поля ===
    private int[] _minutes = Enumerable.Range(0, 60).ToArray();
    private string _filePath;
    private Random rnd;
    private List<MaskotMessage> _messages;
    
    private readonly IAudioManager _audioManager;
    private readonly Func<PomodoroTask, Task<bool>> _confirmDelete;

    private ObservableCollection<PomodoroTask> _tasks = new();
    private ObservableCollection<TaskDeadline> _deadlines = new();

    private PomodoroTask _currentTask;
    private bool _isAddMenuOpen;
    private bool _isTimerRunning;
    private double _addMenuTranslationY = 500;
    private double _addMenuOpacity = 0;
    private string _newTaskName;
    private bool _isWorkPhase = true;

    private int _newWorkHours = 0;
    private int _newWorkMinutes = 25;
    private int _newRestHours = 0;
    private int _newRestMinutes = 5;
    private string _pomodoroTime = "0:00:00";
    private string _freeTime = "0:00:00";
    private string _totalWorkTime = "00:00:00";
    private bool _pomodoroRunning;
    private bool _isPickerOpen;
    private bool _freeRunning;
    private long _freeElapsed;
    private bool _isPomodoroVisible = true;
    private bool _isFreeModeVisible;
    private string _selectedModeText = "Pomodoro";
    private string _currentCatImage = "cat.png";
    private double _karmaValue;

    private bool _isMaskotMessageOn;
    private double _maskotMessageOpacity;
    private string _maskotText;

    // === Свойства ===
    public int[] Minutes
    {
        get => _minutes;
        set { _minutes = value; OnPropertyChanged(); }
    }
    public bool IsMaskotMessageOn
    {
        get => _isMaskotMessageOn;
        set { _isMaskotMessageOn = value; OnPropertyChanged(); }
    }
    public double MaskotMessageOpacity
    {
        get => _maskotMessageOpacity;
        set { _maskotMessageOpacity = value; OnPropertyChanged(); }
    }
    public string MaskotText
    {
        get => _maskotText;
        set { _maskotText = value; OnPropertyChanged(); }
    }
    public double KarmaValue
    {
        get => _karmaValue;
        set
        {
            double clampedValue = Math.Min(1, Math.Max(0, value));
            if (_karmaValue != clampedValue)
            {
                Preferences.Set("karma", clampedValue);
                _karmaValue = clampedValue;
                OnPropertyChanged(nameof(KarmaValue));
                UpdateBackgroundColor();
                UpdateCatImageBasedOnKarma(clampedValue);
            }
        }
    }
    public ObservableCollection<PomodoroTask> Tasks
    {
        get => _tasks;
        set { _tasks = value; OnPropertyChanged(); }
    }
    public ObservableCollection<TaskDeadline> Deadlines
    {
        get => _deadlines;
        set { _deadlines = value; OnPropertyChanged(); }
    }
    public PomodoroTask CurrentTask
    {
        get => _currentTask;
        set
        {
            if (_currentTask == value) return;
            _currentTask = value;
            OnPropertyChanged();
            RefreshTimers();
        }
    }


    public string AlternativeModeText =>
        SelectedModeText == "Pomodoro" ? "Free Timer" : "Pomodoro";

    public string PomodoroButtonIcon =>
        _pomodoroRunning ? "pause.png" : "play.png";

    public string FreeButtonIcon =>
        _freeRunning ? "pause.png" : "play.png";

    public bool IsPickerOpen
    {
        get => _isPickerOpen;
        private set
        {
            if (_isPickerOpen != value)
            {
                _isPickerOpen = value;
                OnPropertyChanged();
            }
        }
    }
    public bool IsWorkPhase
    {
        get => _isWorkPhase;
        private set { _isWorkPhase = value; OnPropertyChanged(); }
    }

    public bool IsTimerRunning
    {
        get => _isTimerRunning;
        set
        {
            _isTimerRunning = value;
            OnPropertyChanged();
        }
    }

    public string CurrentCatImage
    {
        get => _currentCatImage;
        set
        {
            _currentCatImage = value;
            Console.WriteLine($"Cat image changed to: {value}");
            OnPropertyChanged(nameof(CurrentCatImage));
        }
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

    public string NewTaskName
    {
        get => _newTaskName;
        set { _newTaskName = value; OnPropertyChanged(); }
    }

    public int NewWorkHours
    {
        get => _newWorkHours;
        set { _newWorkHours = value; OnPropertyChanged(); }
    }

    public int NewWorkMinutes
    {
        get => _newWorkMinutes;
        set { _newWorkMinutes = value; OnPropertyChanged(); }
    }

    public int NewRestHours
    {
        get => _newRestHours;
        set { _newRestHours = value; OnPropertyChanged(); }
    }

    public int NewRestMinutes
    {
        get => _newRestMinutes;
        set { _newRestMinutes = value; OnPropertyChanged(); }
    }

    public string PomodoroTime
    {
        get => _pomodoroTime;
        set { _pomodoroTime = value; OnPropertyChanged(); }
    }

    public string FreeTime
    {
        get => _freeTime;
        set { _freeTime = value; OnPropertyChanged(); }
    }

    public string TotalWorkTime
    {
        get => _totalWorkTime;
        set { _totalWorkTime = value; OnPropertyChanged(); }
    }

    public bool IsPomodoroVisible
    {
        get => _isPomodoroVisible;
        private set { _isPomodoroVisible = value; OnPropertyChanged(); }
    }

    public bool IsFreeModeVisible
    {
        get => _isFreeModeVisible;
        private set { _isFreeModeVisible = value; OnPropertyChanged(); }
    }

    public string SelectedModeText
    {
        get => _selectedModeText;
        private set { _selectedModeText = value; OnPropertyChanged(); }
    }


    // === Команды ===
    public ICommand LoadTasksCommand { get; }

    public ICommand LoadDeadlinesCommand { get; }
    public ICommand SelectTaskCommand { get; }
    public ICommand StartPomodoroCommand { get; }
    public ICommand ResetPomodoroCommand { get; }
    public ICommand StartFreeCommand { get; }
    public ICommand ResetFreeCommand { get; }
    public ICommand ResetTotalCommand { get; }
    public ICommand ShowAddMenuCommand { get; }
    public ICommand HideAddMenuCommand { get; }
    public ICommand ConfirmAddTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }
    public ICommand ToggleModeCommand { get; }
    public ICommand OpenModePickerCommand { get; }
    public ICommand CancelAddTaskCommand { get; }
    public ICommand PushMaskotMessageCommand { get; }


    // === Команды для тестирования кармы ===
    public ICommand IncreaseKarmaCommand { get; }
    public ICommand DecreaseKarmaCommand { get; }


    public ICommand SaveCurrentTaskCommand =>
    new Command(async () =>
    {
        if (CurrentTask != null)
            await App.Database.SaveTaskAsync(CurrentTask);
    });

    // === Конструктор ===
    public TimerViewModel(Func<PomodoroTask, Task<bool>> confirmDelete, IAudioManager audioManager)
    {
        rnd = new Random();
        KarmaValue = Preferences.Get("karma", 0.2);
        OnPropertyChanged(nameof(KarmaValue));

        MessagingCenter.Subscribe<object, double>(
            this,
            "karmaupdate",
            (sender, newkarmavalue) =>
            {
                KarmaValue = newkarmavalue;
            }
        );
        LoadMessages();

        _audioManager = audioManager;

        _confirmDelete = confirmDelete;


        

        // Инициализация команд
        LoadTasksCommand = new Command(async () => await LoadTasksAsync());
        LoadDeadlinesCommand = new Command(async () => await LoadDeadlinesAsync());
        SelectTaskCommand = new Command<PomodoroTask>(task => CurrentTask = task);
        StartPomodoroCommand = new Command(async () => await OnStartPomodoro());
        ResetPomodoroCommand = new Command(async () => await OnResetPomodoro());
        StartFreeCommand = new Command(async () => await OnStartFree());
        ResetFreeCommand = new Command(() => OnResetFree());
        ResetTotalCommand = new Command(async () => await OnResetTotal());
        ShowAddMenuCommand = new Command(async () => await ShowAddMenuAsync());
        HideAddMenuCommand = new Command(async () => await HideAddMenuAsync());
        ConfirmAddTaskCommand = new Command(async () => await ConfirmAddTaskAsync());
        ToggleModeCommand = new Command(async () => await OnToggleMode());
        OpenModePickerCommand = new Command(async () => await OnOpenModePicker());
        DeleteTaskCommand = new Command<PomodoroTask>(async task => await OnDeleteTaskAsync(task));
        CancelAddTaskCommand = new Command(async () => await HideAddMenuAsync());
        PushMaskotMessageCommand = new Command(async () => await PushMaskotMessageAsync());

        // Добавляем команды для тестирования
        IncreaseKarmaCommand = new Command(() => KarmaValue += 0.1);
        DecreaseKarmaCommand = new Command(() => KarmaValue -= 0.1);

        LoadTasksCommand.Execute(null);
        LoadDeadlinesCommand.Execute(null);

        UpdateBackgroundColor();


    }

    // === Методы ===
    private async void LoadMessages()
    {   
        var json = await FileSystem.OpenAppPackageFileAsync("maskotMessages.json");
        _messages = JsonSerializer.Deserialize<List<MaskotMessage>>(json);
    }


    private async Task PushMaskotMessageAsync()
    {
        MaskotMessage[] mes;
        switch (KarmaValue)
        {
            case (>= 0.8):
                mes = _messages.Where(x => x.Level == 2).ToArray();
                break;
            case (<= 0.4):
                mes = _messages.Where(x => x.Level == 0).ToArray();
                break;
            default:
                mes = _messages.Where(x => x.Level == 1).ToArray();
                break;
        }
        MaskotText = mes[rnd.Next(0, mes.Length)].Text;
        await ShowMaskotMessageAsync();

        await Task.Delay(3000);

        await HideMaskotMessageAsync();
    }
    private async Task ShowMaskotMessageAsync()
    {
        await Task.Delay(1);
        for (double t = 0; t <= 1.0; t += 0.1)
        {
            MaskotMessageOpacity = t;
            await Task.Delay(16);
        }
        IsMaskotMessageOn = true;
    }
    private async Task HideMaskotMessageAsync()
    {
        for (double t = 1; t >= 0; t -= 0.1)
        {
            MaskotMessageOpacity = t;
            await Task.Delay(16);
        }
        IsMaskotMessageOn = false;
    }
    private async Task LoadTasksAsync()
    {
        Tasks.Clear();
        var all = await App.Database.GetAllTasksAsync();
        foreach (var task in all) Tasks.Add(task);
        if (Tasks.Count > 0) CurrentTask = Tasks[0];
    }

    public void UpdateCatImageBasedOnKarma(double karma)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            string newImage = karma switch
            {
                >= 0.8 => "cat.png",
                <= 0.4 => "catsad.png",
                _ => "catnormis.png"
            };

            if (CurrentCatImage != newImage)
                CurrentCatImage = newImage;
        });
    }
    private async Task LoadDeadlinesAsync()
    {
        Deadlines.Clear();
        var all = await App.Database.GetAllDeadlineAsync();
        foreach (var task in all) Deadlines.Add(task);

    }
    private void RefreshTimers()
    {
        if (CurrentTask == null) return;
        PomodoroTime = TimeSpan.FromMilliseconds(CurrentTask.TimeRemaining).ToString(@"h\:mm\:ss");
        TotalWorkTime = TimeSpan.FromMilliseconds(CurrentTask.TotalWorkTime).ToString(@"hh\:mm\:ss");
        FreeTime = TimeSpan.FromMilliseconds(_freeElapsed).ToString(@"h\:mm\:ss");
    }

    private void SendCurrentTaskToDeadlines()
    {
        MessagingCenter.Send<object, PomodoroTask>(
                this,
                "Task",
                CurrentTask
            );
    }
    private async Task PlaySound()
    {
        var player = _audioManager.CreatePlayer(
            await FileSystem.OpenAppPackageFileAsync("budilnik1.wav"));
        player.Play();
    }
    private async Task OnStartPomodoro()
    {
        PushMaskotMessageAsync();
        if (CurrentTask == null) return;
        _pomodoroRunning = !_pomodoroRunning;
        IsTimerRunning = _pomodoroRunning;
        OnPropertyChanged(nameof(PomodoroButtonIcon));
        SendCurrentTaskToDeadlines();
        UpdateBackgroundColor();

        if (!_pomodoroRunning)
        {
            await App.Database.SaveTaskAsync(CurrentTask);
            return;
        }

        while (_pomodoroRunning)
        {
            if (CurrentTask.TimeRemaining <= 0)
            {
                // переключаем фазу
                IsWorkPhase = !IsWorkPhase;
                foreach (var t in Tasks)
                {
                    t.TimeRemaining = IsWorkPhase
                        ? t.WorkDuration
                        : t.RestDuration;
                }

                // обновляем текст
                RefreshTimers();

                SendCurrentTaskToDeadlines();
                // показываем алерт
                await PlaySound();
                if (!IsWorkPhase)
                {
                    KarmaValue += 0.15;
                    Debug.WriteLine($"Karma updated to: {KarmaValue}"); // Для отладки
                }
                string title = IsWorkPhase ? "Пора работать!" : "Пора отдыхать!";
                await Application.Current.MainPage.DisplayAlert("Время!", title, "OK");
                UpdateBackgroundColor();
            }

            await Task.Delay(100);
            CurrentTask.TimeRemaining -= 100;
            if (IsWorkPhase)
                CurrentTask.TotalWorkTime += 100;

            RefreshTimers();
        }

        // по окончании сохраняем
        _pomodoroRunning = false;
        await App.Database.SaveTaskAsync(CurrentTask);
        UpdateBackgroundColor();

    }

    private async Task OnResetPomodoro()
    {
        if (CurrentTask == null) return;
        _pomodoroRunning = false;
        CurrentTask.TimeRemaining = CurrentTask.WorkDuration;
        IsWorkPhase = true;

        RefreshTimers();
        await App.Database.SaveTaskAsync(CurrentTask);
        UpdateBackgroundColor();
        
    }
    

    private async Task OnStartFree()
    {
        PushMaskotMessageAsync();
        if (CurrentTask == null) return;
        _freeRunning = !_freeRunning;
        IsTimerRunning = _freeRunning;
        OnPropertyChanged(nameof(FreeButtonIcon));
        SendCurrentTaskToDeadlines();
        UpdateBackgroundColor();
        if (!_freeRunning)
        {
            // при остановке сохраняем общее время
            await App.Database.SaveTaskAsync(CurrentTask);
            UpdateBackgroundColor();
            return;
        }

        while (_freeRunning)
        {
            await Task.Delay(100);
            _freeElapsed += 100;
            CurrentTask.TotalWorkTime += 100;
            UpdateBackgroundColor();
            RefreshTimers();
        }
        OnPropertyChanged(nameof(Tasks));
        // при полной остановке
        await App.Database.SaveTaskAsync(CurrentTask);
        UpdateBackgroundColor();
        
    }

    private void OnResetFree()
    {
        _freeRunning = false;
        _freeElapsed = 0;
        RefreshTimers();
    }

    private async Task OnResetTotal()
    {
        if (await Application.Current.MainPage.DisplayAlert("Полный сброс", "Вы точно хотите сбросить общее время?", "Да", "Нет"))
        {        
            RefreshTimers();
            CurrentTask.TotalWorkTime = 0;
            SendCurrentTaskToDeadlines();
        }
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

    private async Task ConfirmAddTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTaskName)) return;
        
        var workMs = (NewWorkHours * 60 * 60 + NewWorkMinutes * 60) * 1000;
        var restMs = (NewRestHours * 60 * 60 + NewRestMinutes * 60) * 1000;
        var newTask = new PomodoroTask
        {
            Name = NewTaskName,
            WorkDuration = workMs,
            RestDuration = restMs,
            TimeRemaining = workMs,
            TotalWorkTime = 0
        };
        await App.Database.SaveTaskAsync(newTask);
        Tasks.Add(newTask);

        NewTaskName = string.Empty;
        NewWorkHours = 0;
        NewWorkMinutes = 25;
        NewRestHours = 0;
        NewRestMinutes = 5;
        OnPropertyChanged(nameof(NewWorkHours));
        OnPropertyChanged(nameof(NewWorkMinutes));
        OnPropertyChanged(nameof(NewRestHours));
        OnPropertyChanged(nameof(NewRestMinutes));

        await HideAddMenuAsync();

    }


    private void UpdateBackgroundColor()
    {
        if (!_freeRunning && !_pomodoroRunning)
            switch (KarmaValue)
            {
                case ( >= 0.8):
                    ThemeManager.Instance.GlobalColor = Color.FromArgb("#FF7E7E");
                    break;
                case (<= 0.4):
                    ThemeManager.Instance.GlobalColor = Color.FromArgb("#B49292");
                    break;
                default:
                    ThemeManager.Instance.GlobalColor = Color.FromArgb("#E28C8C");
                    break;
            }
            
        else if (IsWorkPhase || _freeRunning)
            switch (KarmaValue)
            {
                case (>= 0.8):
                    ThemeManager.Instance.GlobalColor = Color.FromArgb("#FFCC46");
                    break;
                case (<= 0.4):
                    ThemeManager.Instance.GlobalColor = Color.FromArgb("#A99254");
                    break;
                default:
                    ThemeManager.Instance.GlobalColor = Color.FromArgb("#DFBA58");
                    break;
            }
        else
            switch (KarmaValue)
            {
                case (>= 0.8):
                    ThemeManager.Instance.GlobalColor = Color.FromArgb("#15BF2E");
                    break;
                case (<= 0.4):
                    ThemeManager.Instance.GlobalColor = Color.FromArgb("#3E8047");
                    break;
                default:
                    ThemeManager.Instance.GlobalColor = Color.FromArgb("#129425");
                    break;
            }
    }

    private async Task OnToggleMode()
    {
        // Переключаем режим
        bool isPomodoro = SelectedModeText == "Pomodoro";
        IsPomodoroVisible = !isPomodoro;
        IsFreeModeVisible = isPomodoro;
        SelectedModeText = isPomodoro ? "Free Timer" : "Pomodoro";


        OnPropertyChanged(nameof(SelectedModeText));
        OnPropertyChanged(nameof(AlternativeModeText));
        RefreshTimers();
        OnOpenModePicker();
    }
    private async Task OnOpenModePicker()
    {
        // Переключаем меню выбора
        IsPickerOpen = !IsPickerOpen;

    }

    private async Task OnDeleteTaskAsync(PomodoroTask task)
    {
        if (task == null) return;

        bool confirmed = await _confirmDelete(task);
        if (!confirmed) return;

        await App.Database.DeleteTaskAsync(task.Id);
        Tasks.Remove(task);

        if (CurrentTask == task && Tasks.Count > 0)
            CurrentTask = Tasks.FirstOrDefault();
    }

}