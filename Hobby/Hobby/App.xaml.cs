using Hobby.DataBase;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
namespace Hobby
{
    public partial class App : Application
    {
        private static DB db;
        public static DB Db
        {
            get
            {
                if (db == null)
                    db = new DB(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "database.sqlite3")); // Путь где создастся бд
                return db;
            }
        }
        public App ()
        {
            InitializeComponent();
            
            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
        }
    }
}
