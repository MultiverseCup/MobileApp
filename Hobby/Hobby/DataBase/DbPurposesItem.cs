using SQLite;

namespace Hobby.DataBase
{
    public class DbPurposesItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int TaskID { get; set; }

        // Теперь long
        public long PlannedTime { get; set; }
        public string Deadline { get; set; }
        public long InitialTotalTime { get; set; }
    }
}