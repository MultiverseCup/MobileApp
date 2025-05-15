using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using PomodoroProject.Data.Models;
using PomodoroProject.Data;
using Microsoft.Maui.Controls;

namespace PomodoroProject.ViewModels
{
    public partial class TimerViewModel : INotifyPropertyChanged
    {
        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Список задач
        public ObservableCollection<PomodoroTask> Tasks { get; } = new();

        private PomodoroTask _currentTask;
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

        // Текстовые представления времени
        private string _pomodoroTime = "00:00";
        public string PomodoroTime
        {
            get => _pomodoroTime;
            set { _pomodoroTime = value; OnPropertyChanged(); }
        }

        private string _freeTime = "00:00";
        public string FreeTime
        {
            get => _freeTime;
            set { _freeTime = value; OnPropertyChanged(); }
        }

        private string _totalWorkTime = "00:00:00";
        public string TotalWorkTime
        {
            get => _totalWorkTime;
            set { _totalWorkTime = value; OnPropertyChanged(); }
        }

        // Команды
        public ICommand LoadTasksCommand { get; }
        public ICommand SelectTaskCommand { get; }
        public ICommand StartPomodoroCommand { get; }
        public ICommand ResetPomodoroCommand { get; }
        public ICommand StartFreeCommand { get; }
        public ICommand ResetFreeCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }

        public TimerViewModel()
        {
            LoadTasksCommand = new Command(async () => await LoadTasksAsync());
            SelectTaskCommand = new Command<PomodoroTask>(t => CurrentTask = t);
            StartPomodoroCommand = new Command(async () => await OnStartPomodoro());
            ResetPomodoroCommand = new Command(async () => await OnResetPomodoro());
            StartFreeCommand = new Command(async () => await OnStartFree());
            ResetFreeCommand = new Command(() => OnResetFree());
            // AddTaskCommand, DeleteTaskCommand аналогично

            LoadTasksCommand.Execute(null);
        }

        private async Task LoadTasksAsync()
        {
            Tasks.Clear();
            var all = await App.Database.GetAllTasksAsync();
            foreach (var t in all) Tasks.Add(t);
            if (Tasks.Count > 0)
                CurrentTask = Tasks[0];
        }

        private void RefreshTimers()
        {
            if (CurrentTask == null) return;
            PomodoroTime = TimeSpan.FromMilliseconds(CurrentTask.TimeRemaining).ToString(@"mm\:ss");
            TotalWorkTime = TimeSpan.FromMilliseconds(CurrentTask.TotalWorkTime).ToString(@"hh\:mm\:ss");
            FreeTime = "00:00"; // сброшено при выборе
        }

        private bool _pomodoroRunning;
        private async Task OnStartPomodoro()
        {
            if (CurrentTask == null) return;

            _pomodoroRunning = !_pomodoroRunning;
            if (!_pomodoroRunning)
            {
                await App.Database.SaveTaskAsync(CurrentTask);
                return;
            }
            while (_pomodoroRunning && CurrentTask.TimeRemaining > 0)
            {
                await Task.Delay(100);
                CurrentTask.TimeRemaining -= 100;
                CurrentTask.TotalWorkTime += 100;
                RefreshTimers();
            }
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

        private bool _freeRunning;
        private long _freeElapsed;
        private async Task OnStartFree()
        {
            if (CurrentTask == null) return;
            _freeRunning = !_freeRunning;
            if (!_freeRunning) return;

            while (_freeRunning)
            {
                await Task.Delay(100);
                _freeElapsed += 100;
                CurrentTask.TotalWorkTime += 100;
                RefreshTimers();
            }
            await App.Database.SaveTaskAsync(CurrentTask);
        }

        private void OnResetFree()
        {
            _freeRunning = false;
            _freeElapsed = 0;
            RefreshTimers();
        }
    }
}
