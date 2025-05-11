using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hobby.DataBase
{
    public class DB
    {
        private readonly SQLiteConnection _database;

        public DB(string dbPath)
        {
            _database = new SQLiteConnection(dbPath);


            _database.CreateTable<DbItem>(CreateFlags.AllImplicit);
            _database.CreateTable<DbPurposesItem>(CreateFlags.AllImplicit);
        }

        public List<DbItem> GetItems() => _database.Table<DbItem>().ToList();
        public int SaveItem(DbItem item) => item.ID != 0 ? _database.Update(item) : _database.Insert(item);
        public int SaveItem(DbPurposesItem item) => item.ID != 0 ? _database.Update(item) : _database.Insert(item);

        public int DeleteItem(int id) => _database.Delete<DbItem>(id);
        public bool IsEmpty() => _database.Table<DbItem>().Count() == 0;



        public List<DbPurposesItem> GetScheduleItems() =>
    _database.Table<DbPurposesItem>().ToList();

        public int SaveScheduleItem(DbPurposesItem item) =>
            item.ID != 0 ? _database.Update(item) : _database.Insert(item);

        public int DeleteScheduleItem(int id) =>
            _database.Delete<DbPurposesItem>(id);

        // Поиск всех планов по задаче
        public List<DbPurposesItem> GetScheduleForTask(int taskId) =>
            _database.Table<DbPurposesItem>().Where(s => s.TaskID == taskId).ToList();

        public List<string> GetColumns()
        {
            return _database
                .GetTableInfo(nameof(DbItem))
                .Select(c => c.Name)
                .ToList();
        }
    }
}
