using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimerPomodoro.DataBase
{
    public class DbItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }

        // Теперь long
        public long WorkDuration { get; set; }
        public long RestDuration { get; set; }
        public long TimeRemaining { get; set; }
        public long TotalWorkTime { get; set; }

        public string WorkTimePerDay { get; set; }
        public string Schedule { get; set; }
        public string BoxColor { get; set; }
    }
}
