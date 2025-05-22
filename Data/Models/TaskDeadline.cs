using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PomodoroProject.Data.Models
{
    public class TaskDeadline : INotifyPropertyChanged
    {
        private long _elapsedTotalTime;
        private bool _isCompleted;
        private bool _isOverdue;
        private bool _isActual = true;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int TaskId { get; set; } // Foreign key

        public long PlannedTime { get; set; }
        public DateTime Deadline { get; set; } 
        public long InitialTotalTime { get; set; }
        public long ElapsedTotalTime { 
            get => _elapsedTotalTime; 
            set { _elapsedTotalTime = value; 
                OnPropertyChanged(nameof(DisplayElapsedTotalTime));
                
                
            } }

        public string TaskName { get; set; } // Foreign key
        public string DeadlineName { get; set; }


        public bool IsCompleted { get => _isCompleted; 
            set { _isCompleted = value; OnPropertyChanged(nameof(DisplayStatus)); OnPropertyChanged(); } }
        public bool IsOverdue { get => _isOverdue; 
            set { _isOverdue = value; OnPropertyChanged(nameof(DisplayStatus)); OnPropertyChanged(); } }
        public bool IsActual { 
            get => _isActual;
            set { _isActual = value; OnPropertyChanged(); } }
        public string DisplayElapsedTotalTime => TimeSpan.FromMilliseconds(ElapsedTotalTime).ToString(@"hh\:mm\:ss");
        public string DisplayStatus => IsCompleted ? "Выполнено" : IsOverdue ? "Просрочено" : "В работе";
        public string DisplayDeadline => Deadline.ToString("D", new CultureInfo("ru-RU"));
        public string DisplayPlannedTime => Math.Round((PlannedTime / 3600000.0),2).ToString();

        

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
