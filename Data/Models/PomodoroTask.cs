using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace PomodoroProject.Data.Models
{
    public class PomodoroTask : INotifyPropertyChanged
    {
        // ===== Событие для INotifyPropertyChanged =====
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ===== Поля и свойства =====

        private int _id;
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        private long _workDuration;
        public long WorkDuration
        {
            get => _workDuration;
            set
            {
                if (_workDuration == value) return;
                _workDuration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayWorkDuration));
            }
        }

        private long _restDuration;
        public long RestDuration
        {
            get => _restDuration;
            set
            {
                if (_restDuration == value) return;
                _restDuration = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayRestDuration));
            }
        }

        private long _timeRemaining;
        public long TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                if (_timeRemaining == value) return;
                _timeRemaining = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayTimeRemaining));
            }
        }

        private long _totalWorkTime;
        public long TotalWorkTime
        {
            get => _totalWorkTime;
            set
            {
                if (_totalWorkTime == value) return;
                _totalWorkTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayTotalTime));
            }
        }

        // ===== Вычисляемые свойства для UI =====

        public string DisplayWorkDuration =>
            TimeSpan.FromMilliseconds(WorkDuration).ToString(@"mm\:ss");

        public string DisplayRestDuration =>
            TimeSpan.FromMilliseconds(RestDuration).ToString(@"mm\:ss");

        public string DisplayTimeRemaining =>
            TimeSpan.FromMilliseconds(TimeRemaining).ToString(@"mm\:ss");

        public string DisplayTotalTime =>
            TimeSpan.FromMilliseconds(TotalWorkTime).ToString(@"hh\:mm\:ss");
    }
}
