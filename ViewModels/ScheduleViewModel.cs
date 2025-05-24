
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.LocalNotification;
using PomodoroProject.Data;
using PomodoroProject.Data.Models;



namespace PomodoroProject.ViewModels
{
    public partial class ScheduleViewModel : INotifyPropertyChanged
    {
        // === События ===
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // === Поля ===

        private Color _clr;

        //Свойства

        public Color CLR
        {
            get => _clr;
            private set { _clr = value; OnPropertyChanged(); }
        }

        //Команды

        public ICommand TestNotificationCommand { get; }

        //Конструктор
        public ScheduleViewModel()
        {
            TestNotificationCommand = new Command(async () => await TestNotification());
        }
        //Методы

        private async Task TestNotification()
        {
            var request = new NotificationRequest
            {
                NotificationId = 1,
                Title = "TEST",
                Description = "bb'",

                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(3)
                }
            };

            await LocalNotificationCenter.Current.Show(request);
            CLR = Colors.Aqua;
        }
    }
}
