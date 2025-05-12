using SQLite;
using System;

namespace Hobby.DataBase
{
    public class DbPurposesItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        // Внешний ключ — ID задачи из DbItem
        public int TaskID { get; set; }

        // Сколько времени (в мс) планируется потратить
        public int PlannedTime { get; set; }

        // Крайний срок (DateTime в виде строки)
        public string Deadline { get; set; }

        // Снимок TotalWorkTime в момент добавления в план
        public int InitialTotalTime { get; set; }
    }
}