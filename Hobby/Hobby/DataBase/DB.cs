using SQLite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Hobby.DataBase
{
    public class DB
    {
        private readonly SQLiteConnection _database;

        public DB(string dbPath)
        {
            _database = new SQLiteConnection(dbPath);
            _database.CreateTable<DbItem>(); // Создаём таблицу, если её нет
        }

        // Получение всех элементов
        public List<DbItem> GetItems()
        {
            return _database.Table<DbItem>().ToList();
        }

        // Сохранение (добавление или обновление)
        public int SaveItem(DbItem item)
        {
            if (item.ID != 0)
                return _database.Update(item);
            else
                return _database.Insert(item);
        }

        // Удаление элемента по ID
        public int DeleteItem(int id)
        {
            return _database.Delete<DbItem>(id); // Важно: передаём ID, а не объект
        }

        // Проверка, пуста ли БД
        public bool IsEmpty()
        {
            return _database.Table<DbItem>().Count() == 0;
        }
    }
}