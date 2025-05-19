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

namespace PomodoroProject.ViewModels
{
    public partial class TimerViewModel : INotifyPropertyChanged
    {
        // === События ===
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // === Поля ===
        private readonly Func<PomodoroTask, Task<bool>> _confirmDelete;

        private ObservableCollection<PomodoroTask> _tasks = new();
        private PomodoroTask _currentTask;
        private bool _isAddMenuOpen;
        private double _addMenuTranslationY = 500;
        private double _addMenuOpacity = 0;
        private string _newTaskName;
        private bool _isWorkPhase = true;
        private string _newWorkMinutes = "25";
        private string _newWorkSeconds = "00";
        private string _newRestMinutes = "5";
        private string _newRestSeconds = "00";
        private string _pomodoroTime = "00:00";
        private string _freeTime = "00:00";
        private string _totalWorkTime = "00:00:00";
        private bool _pomodoroRunning;
        private bool _isPickerOpen;
        private bool _freeRunning;
        private long _freeElapsed;
        private bool _overlayIsVisible;
        private bool _isPomodoroVisible = true;
        private bool _isFreeModeVisible;
        private string _selectedModeText = "Pomodoro";

        // === Свойства ===
        public ObservableCollection<PomodoroTask> Tasks
        {
            get => _tasks;
            set { _tasks = value; OnPropertyChanged(); }
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

        public string NewWorkMinutes
        {
            get => _newWorkMinutes;
            set { _newWorkMinutes = value; OnPropertyChanged(); }
        }

        public string NewWorkSeconds
        {
            get => _newWorkSeconds;
            set { _newWorkSeconds = value; OnPropertyChanged(); }
        }

        public string NewRestMinutes
        {
            get => _newRestMinutes;
            set { _newRestMinutes = value; OnPropertyChanged(); }
        }

        public string NewRestSeconds
        {
            get => _newRestSeconds;
            set { _newRestSeconds = value; OnPropertyChanged(); }
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
        public ICommand SelectTaskCommand { get; }
        public ICommand StartPomodoroCommand { get; }
        public ICommand ResetPomodoroCommand { get; }
        public ICommand StartFreeCommand { get; }
        public ICommand ResetFreeCommand { get; }
        public ICommand ResetTotalCommand { get; }
        public ICommand ShowAddMenuCommand { get; }
        public ICommand ConfirmAddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ToggleModeCommand { get; }
        public ICommand OpenModePickerCommand { get; }

        public ICommand CancelAddTaskCommand { get; }

        public ICommand SaveCurrentTaskCommand =>
        new Command(async () =>
        {
            if (CurrentTask != null)
                await App.Database.SaveTaskAsync(CurrentTask);
        });

        // === Конструктор ===
        public TimerViewModel(Func<PomodoroTask, Task<bool>> confirmDelete)
        {
            _confirmDelete = confirmDelete;

            // Инициализация команд
            LoadTasksCommand = new Command(async () => await LoadTasksAsync());
            SelectTaskCommand = new Command<PomodoroTask>(task => CurrentTask = task);
            StartPomodoroCommand = new Command(async () => await OnStartPomodoro());
            ResetPomodoroCommand = new Command(async () => await OnResetPomodoro());
            StartFreeCommand = new Command(async () => await OnStartFree());
            ResetFreeCommand = new Command(() => OnResetFree());
            ResetTotalCommand = new Command(async () => await OnResetTotal());
            ShowAddMenuCommand = new Command(async () => await ShowAddMenuAsync());
            ConfirmAddTaskCommand = new Command(async () => await ConfirmAddTaskAsync());
            ToggleModeCommand = new Command(OnToggleMode);
            OpenModePickerCommand = new Command(OnOpenModePicker);
            DeleteTaskCommand = new Command<PomodoroTask>(async task => await OnDeleteTaskAsync(task));
            CancelAddTaskCommand = new Command(async () => await HideAddMenuAsync());

            LoadTasksCommand.Execute(null);
        }

        // === Методы ===
        private async Task LoadTasksAsync()
        {
            Tasks.Clear();
            var all = await App.Database.GetAllTasksAsync();
            foreach (var task in all) Tasks.Add(task);
            if (Tasks.Count > 0) CurrentTask = Tasks[0];
        }

        private void RefreshTimers()
        {
            if (CurrentTask == null) return;
            PomodoroTime = TimeSpan.FromMilliseconds(CurrentTask.TimeRemaining).ToString(@"mm\:ss");
            TotalWorkTime = TimeSpan.FromMilliseconds(CurrentTask.TotalWorkTime).ToString(@"hh\:mm\:ss");
            FreeTime = TimeSpan.FromMilliseconds(_freeElapsed).ToString(@"mm\:ss");
        }


        private async Task HideAddMenuAsync()
        {
            for (double t = 1; t >= 0; t -= 0.1)
            {
                AddMenuTranslationY = 500 * (1 - t);
                AddMenuOpacity = t;
                await Task.Delay(16);
            }
            IsAddMenuOpen = false;
        }
        private async Task OnStartPomodoro()
        {
            if (CurrentTask == null) return;
            _pomodoroRunning = !_pomodoroRunning;
            OnPropertyChanged(nameof(PomodoroButtonIcon));

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
                    CurrentTask.TimeRemaining = IsWorkPhase
                        ? CurrentTask.WorkDuration
                        : CurrentTask.RestDuration;

                    // обновляем текст
                    RefreshTimers();

                    // показываем алерт
                    string title = IsWorkPhase ? "Пора работать!" : "Пора отдыхать!";
                    await Application.Current.MainPage.DisplayAlert("Время!", title, "OK");
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
            
        }

        private async Task OnResetPomodoro()
        {
            if (CurrentTask == null) return;
            _pomodoroRunning = false;
            CurrentTask.TimeRemaining = CurrentTask.WorkDuration;
            CurrentTask.TotalWorkTime = 0;
            RefreshTimers();
            await App.Database.SaveTaskAsync(CurrentTask);
        }

        private async Task OnStartFree()
        {
            if (CurrentTask == null) return;
            _freeRunning = !_freeRunning;
            OnPropertyChanged(nameof(FreeButtonIcon));
            if (!_freeRunning)
            {
                // при остановке сохраняем общее время
                await App.Database.SaveTaskAsync(CurrentTask);
                return;
            }

            while (_freeRunning)
            {
                await Task.Delay(100);
                _freeElapsed += 100;
                CurrentTask.TotalWorkTime += 100;

                RefreshTimers();
            }

            // при полной остановке
            await App.Database.SaveTaskAsync(CurrentTask);
        }

        private void OnResetFree()
        {
            _freeRunning = false;
            _freeElapsed = 0;
            RefreshTimers();
        }

        private async Task OnResetTotal()
        {
            await OnResetPomodoro();
        }

        private async Task ShowAddMenuAsync()
        {
            IsAddMenuOpen = true;
            AddMenuTranslationY = 500;
            AddMenuOpacity = 0;
            await Task.Delay(1);
            for (double t = 0; t <= 1.0; t += 0.1)
            {
                AddMenuTranslationY = 500 * (1 - t);
                AddMenuOpacity = t;
                await Task.Delay(16);
            }
        }

        private async Task ConfirmAddTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskName)) return;
            bool minW = int.TryParse(NewWorkMinutes, out int wMin);
            bool secW = int.TryParse(NewWorkSeconds, out int wSec);
            bool minR = int.TryParse(NewRestMinutes, out int rMin);
            bool secR = int.TryParse(NewRestSeconds, out int rSec);
            if (!minW || !secW || !minR || !secR) return;
            var workMs = (wMin * 60 + wSec) * 1000;
            var restMs = (rMin * 60 + rSec) * 1000;
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
            NewWorkMinutes = "25";
            NewWorkSeconds = "00";
            NewRestMinutes = "5";
            NewRestSeconds = "00";
            OnPropertyChanged(nameof(NewWorkMinutes));
            OnPropertyChanged(nameof(NewWorkSeconds));
            OnPropertyChanged(nameof(NewRestMinutes));
            OnPropertyChanged(nameof(NewRestSeconds));
            for (double t = 1; t >= 0; t -= 0.1)
            {
                AddMenuTranslationY = 500 * (1 - t);
                AddMenuOpacity = t;
                await Task.Delay(16);
            }
            IsAddMenuOpen = false;
        }

        private void OnToggleMode(object obj)
        {
            // Переключаем режим
            bool isPomodoro = SelectedModeText == "Pomodoro";
            IsPomodoroVisible = !isPomodoro;
            IsFreeModeVisible = isPomodoro;
            SelectedModeText = isPomodoro ? "Free Timer" : "Pomodoro";
            

            OnPropertyChanged(nameof(SelectedModeText));
            OnPropertyChanged(nameof(AlternativeModeText));
            RefreshTimers();
        }
        private void OnOpenModePicker(object obj)
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
}