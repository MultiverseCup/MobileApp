using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hobby.DataBase
{
    public class DB
    {
        // синхронная и асинхронная связи
        private readonly SQLiteConnection _syncDb;
        private readonly SQLiteAsyncConnection _asyncDb;

        public DB(string dbPath)
        {
            _syncDb = new SQLiteConnection(dbPath);
            _asyncDb = new SQLiteAsyncConnection(dbPath);

            // создаём таблицы, если их нет
            _syncDb.CreateTable<DbItem>();
            _syncDb.CreateTable<DbPurposesItem>();

            _asyncDb.CreateTableAsync<DbItem>().Wait();
            _asyncDb.CreateTableAsync<DbPurposesItem>().Wait();

            // миграция: добавляем InitialTotalTime, если его нет
            var cols = _syncDb.GetTableInfo(nameof(DbPurposesItem))
                              .Select(c => c.Name)
                              .ToList();
            if (!cols.Contains(nameof(DbPurposesItem.InitialTotalTime)))
            {
                _syncDb.Execute(
                    $"ALTER TABLE {nameof(DbPurposesItem)} " +
                    $"ADD COLUMN {nameof(DbPurposesItem.InitialTotalTime)} INTEGER DEFAULT 0;");
            }
        }

        // -------------------------
        // Методы для PomodoroPage
        // -------------------------

        // Синхронное получение всех задач
        public List<DbItem> GetItems() =>
            _syncDb.Table<DbItem>().ToList();

        // Синхронное сохранение/обновление DbItem
        public int SaveItem(DbItem item) =>
            item.ID != 0
                ? _syncDb.Update(item)
                : _syncDb.Insert(item);

        // Синхронное удаление DbItem
        public int DeleteItem(int id) =>
            _syncDb.Delete<DbItem>(id);

        // Список колонок DbItem (для дебага)
        public List<string> GetColumns() =>
            _syncDb
              .GetTableInfo(nameof(DbItem))
              .Select(c => c.Name)
              .ToList();

        // --------------------------------
        // Методы для PurposesPage (Planner)
        // --------------------------------

        // Синхронно получить все записи планировщика
        public List<DbPurposesItem> GetPurposesItems() =>
            _syncDb.Table<DbPurposesItem>().ToList();

        // Асинхронно сохранить/обновить запись планировщика
        public Task<int> SavePurposesItemAsync(DbPurposesItem item)
        {
            if (item.ID == 0)
                return _asyncDb.InsertAsync(item);
            else
                return _asyncDb.UpdateAsync(item);
        }

        // Асинхронно удалить запись планировщика
        public Task<int> DeletePurposesItemAsync(int id) =>
            _asyncDb.DeleteAsync<DbPurposesItem>(id);

        // Синхронно найти все записи планировщика по ID задачи
        public List<DbPurposesItem> GetPurposesForTask(int taskId) =>
            _syncDb
              .Table<DbPurposesItem>()
              .Where(s => s.TaskID == taskId)
              .ToList();

        // Дебаг: колонки таблицы планировщика
        public List<string> GetPurposesColumns() =>
            _syncDb
              .GetTableInfo(nameof(DbPurposesItem))
              .Select(c => c.Name)
              .ToList();
    }
}
