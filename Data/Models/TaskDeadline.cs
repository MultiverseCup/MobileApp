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

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int TaskId { get; set; } // Foreign key

        public long PlannedTime { get; set; }
        public DateTime Deadline { get; set; } 
        public long InitialTotalTime { get; set; }
        public long ElapsedTotalTime { 
            get => _elapsedTotalTime; 
            set { _elapsedTotalTime = value; OnPropertyChanged(nameof(DisplayElapsedTotalTime)); } }

        public string TaskName { get; set; } // Foreign key
        public string DeadlineName { get; set; }



        public bool IsActual => DateTime.Now < Deadline;
        public bool IsCompleted => !IsActual && ElapsedTotalTime >= PlannedTime;
        public bool IsOverdue => !IsActual && ElapsedTotalTime < PlannedTime;

        public string DisplayElapsedTotalTime => TimeSpan.FromMilliseconds(ElapsedTotalTime).ToString(@"hh\:mm\:ss");
        public string DisplayStatus => IsCompleted ? "Выполнено" : IsOverdue ? "Просрочено" : "В работе";
        public string DisplayDeadline => Deadline.ToString("D", new CultureInfo("ru-RU"));
        public string DisplayPlannedTime => Math.Round((PlannedTime / 3600000.0),1).ToString();






        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
