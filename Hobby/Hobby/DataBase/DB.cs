using SQLite;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hobby.DataBase
{
    public class DB
    {
        private SQLiteConnection _connection;

        public DB(string dbPath)
        {
            // Создаём подключение к базе данных
            _connection = new SQLiteConnection(dbPath);
            CreateTables(); // Создаём таблицы при инициализации базы данных
        }

        private void CreateTables()
        {
            // Создаём таблицы для Item и TaskItem (если они еще не существуют)
            _connection.CreateTable<Item>();
            _connection.CreateTable<TaskItem>(); // Обратите внимание, что TaskItem теперь тоже создается
        }

        // Метод для получения всех элементов из базы данных
        public List<Item> GetItems()
        {
            return _connection.Table<Item>().ToList();
        }

        // Метод для сохранения элемента в базе данных
        public void SaveItem(Item item)
        {
            if (item.ID != 0)
                _connection.Update(item); // Если ID существует, обновляем запись
            else
                _connection.Insert(item); // Если нет, то вставляем новую запись
        }

        // Проверка на пустоту базы данных
        public bool IsEmpty()
        {
            return _connection.Table<Item>().Count() == 0;
        }
    }
}
