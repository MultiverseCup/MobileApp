using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hobby.DataBase
{
    public class DB
    {
        private readonly SQLiteConnection conn;

        public DB(string path) //Путь к файлу бд
        {
            conn = new SQLiteConnection(path);
            conn.CreateTable<Item>();
        }

        public List<Item> GetItems()
        {
            return conn.Table<Item>().ToList();
        }

        public int SaveItem(Item item)
        {
            return conn.Insert(item);
        }

        public bool IsEmpty()
        {
            return GetItems().Count == 0;
        }

        public void ClearAll()
        {
            conn.DeleteAll<Item>();
        }
    }
}
