
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PomodoroProject.Data.Models
{
    public class TaskDeadline
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int TaskId { get; set; } // Foreign key

        public long PlannedTime { get; set; }
        public string DeadlineData { get; set; } 
        public long InitialTotalTime { get; set; }

        public int TaskName { get; set; } // Foreign key

        public int DeadlineName { get; set; }
    }
}
