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
            _database.CreateTable<Item>(); // Создаём таблицу, если её нет
        }

        // Получение всех элементов
        public List<Item> GetItems()
        {
            return _database.Table<Item>().ToList();
        }

        // Сохранение (добавление или обновление)
        public int SaveItem(Item item)
        {
            if (item.ID != 0)
                return _database.Update(item);
            else
                return _database.Insert(item);
        }

        // Удаление элемента по ID
        public int DeleteItem(int id)
        {
            return _database.Delete<Item>(id); // Важно: передаём ID, а не объект
        }

        // Проверка, пуста ли БД
        public bool IsEmpty()
        {
            return _database.Table<Item>().Count() == 0;
        }
    }
}