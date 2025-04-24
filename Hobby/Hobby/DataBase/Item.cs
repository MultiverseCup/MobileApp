using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using SQLite;

namespace Hobby.DataBase
{
    enum Weekdays
    {
        MON, TUE, WED, THU, FRI, SAT, SUN
    };
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }
        public int WorkDuration { get; set; }  // значение в мс
        public int RestDuration { get; set; }
        public int TimeRemaining { get; set; }



        // Нужно будет где-то делать словарь(timespan с датой-int время) и хэш функцию
        // И сюда записывать хэши, либо как-то подругому записывать знаения
        public string WorkTimePerDay { get; set; }
        public int TotalWorkTime { get; set; }

        // Расписание 
        public string Schedule { get; set; } //Формат записи: "MON[10:00] FRI[18:30]"

        public string BoxColor { get; set; } //Дефолтные цвета из Color. только в строчке

    }
}