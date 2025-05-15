using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PomodoroProject.Data.Models
{
    public class PomodoroTask
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public long WorkDuration { get; set; }
        public long RestDuration { get; set; }
        public long TimeRemaining { get; set; }
        public long TotalWorkTime { get; set; }

        public string WorkTimePerDay { get; set; }
        public string Schedule { get; set; }
        public string BoxColor { get; set; }
    }
}
