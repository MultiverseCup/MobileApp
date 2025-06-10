using Plugin.LocalNotification;
using PomodoroProject.Data;
using PomodoroProject.ViewModels;


namespace PomodoroProject
{
    public partial class App : Application
    {

        public static AppDatabase Database { get; private set; }

        public App()
        {
            InitializeComponent();

            ThemeManager.Instance.UpdateApplicationResources();
            // Назначаем Shell сразу — UI сможет отрисоваться
            MainPage = new AppShell();


            

            // А БД инициализируем в фоне, не блокируя UI
            _ = InitializeDatabaseAsync();
        }

        private async Task InitializeDatabaseAsync()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "app.db");
            Database = new AppDatabase(path);
            await Database.InitializeAsync();
        }

    }
}