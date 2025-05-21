using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace PomodoroProject.Data.Models
{
    public class TaskDeadline
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int TaskId { get; set; } // Foreign key

        public long PlannedTime { get; set; }
        public DateTime Deadline { get; set; } 
        public long InitialTotalTime { get; set; }
        public long TotalTime { get; set; }
        public string TaskName { get; set; } // Foreign key

        public string DeadlineName { get; set; }
        public bool IsActual => DateTime.Now < Deadline;
        public bool IsCompleted => !IsActual && TotalTime >= PlannedTime;
        public bool IsOverdue => !IsActual && TotalTime < PlannedTime;

        public string DisplayStatus => IsCompleted ? "Выполнено" : IsOverdue ? "Просрочено" : "В работе";

        public string DisplayDeadline => Deadline.ToString("D", new CultureInfo("ru-RU"));
        public string DisplayPlannedTime => Math.Round((PlannedTime / 3600000.0),1).ToString();
    }
}
