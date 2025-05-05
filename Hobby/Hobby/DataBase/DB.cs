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


            // Миграция (если нужно)
            //var cols = _database.GetTableInfo(nameof(DbItem)).Select(c => c.Name).ToList();
            //if (!cols.Contains(nameof(DbItem.TotalWorkTime)))
            //{
            //    _database.Execute(
            //        $"ALTER TABLE {nameof(DbItem)} ADD COLUMN {nameof(DbItem.TotalWorkTime)} INTEGER DEFAULT 0;");
            //}
        }

        public List<DbItem> GetItems() => _database.Table<DbItem>().ToList();
        public int SaveItem(DbItem item) => item.ID != 0 ? _database.Update(item) : _database.Insert(item);
        public int DeleteItem(int id) => _database.Delete<DbItem>(id);
        public bool IsEmpty() => _database.Table<DbItem>().Count() == 0;

        // ← Новый метод
        public List<string> GetColumns()
        {
            return _database
                .GetTableInfo(nameof(DbItem))
                .Select(c => c.Name)
                .ToList();
        }
    }
}
