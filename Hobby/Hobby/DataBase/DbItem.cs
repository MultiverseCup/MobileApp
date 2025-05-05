using SQLite;

namespace Hobby.DataBase
{
    public class DbItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public int WorkDuration { get; set; }
        public int RestDuration { get; set; }
        public int TimeRemaining { get; set; }
        public string WorkTimePerDay { get; set; }
        public int TotalWorkTime { get; set; }
        public string Schedule { get; set; }
        public string BoxColor { get; set; }
    }
}