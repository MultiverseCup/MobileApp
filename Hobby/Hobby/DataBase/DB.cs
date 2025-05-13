using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hobby.DataBase
{
    public class DB
    {
        private readonly SQLiteConnection _syncDb;
        private readonly SQLiteAsyncConnection _asyncDb;

        public DB(string dbPath)
        {
            _syncDb = new SQLiteConnection(dbPath);
            _asyncDb = new SQLiteAsyncConnection(dbPath);

            // создаём таблицы
            _syncDb.CreateTable<DbItem>();
            _syncDb.CreateTable<DbPurposesItem>();
            _asyncDb.CreateTableAsync<DbItem>().Wait();
            _asyncDb.CreateTableAsync<DbPurposesItem>().Wait();

            // миграция InitialTotalTime
            var cols = _syncDb.GetTableInfo(nameof(DbPurposesItem)).Select(c => c.Name).ToList();
            if (!cols.Contains(nameof(DbPurposesItem.InitialTotalTime)))
            {
                _syncDb.Execute(
                  $"ALTER TABLE {nameof(DbPurposesItem)} " +
                  $"ADD COLUMN {nameof(DbPurposesItem.InitialTotalTime)} INTEGER DEFAULT 0;"
                );
            }
        }

        public int DeletePurposesForTask(int taskId) => _syncDb.Execute(
        "DELETE FROM DbPurposesItem WHERE TaskID = ?",
        taskId);

        // PomodoroPage
        public List<DbItem> GetItems() => _syncDb.Table<DbItem>().ToList();
        public int SaveItem(DbItem item) =>
            item.ID != 0 ? _syncDb.Update(item) : _syncDb.Insert(item);
        public int DeleteItem(int id) => _syncDb.Delete<DbItem>(id);
        public List<string> GetColumns() =>
            _syncDb.GetTableInfo(nameof(DbItem)).Select(c => c.Name).ToList();

        // PurposesPage
        public List<DbPurposesItem> GetPurposesItems() =>
            _syncDb.Table<DbPurposesItem>().ToList();

        public Task<int> SavePurposesItemAsync(DbPurposesItem item)
        {
            return item.ID == 0
                ? _asyncDb.InsertAsync(item)
                : _asyncDb.UpdateAsync(item);
        }

        public Task<int> DeletePurposesItemAsync(int id) =>
            _asyncDb.DeleteAsync<DbPurposesItem>(id);

        public List<DbPurposesItem> GetPurposesForTask(int taskId) =>
            _syncDb.Table<DbPurposesItem>().Where(s => s.TaskID == taskId).ToList();

        public List<string> GetPurposesColumns() =>
            _syncDb.GetTableInfo(nameof(DbPurposesItem)).Select(c => c.Name).ToList();
    }
}
