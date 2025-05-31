
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.LocalNotification;
using PomodoroProject.Data;
using PomodoroProject.Data.Models;
using Microsoft.Maui.Storage;


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
        private bool _isMondayToggled;
        private bool _isTuesdayToggled;
        private bool _isWednesdayToggled;
        private bool _isThursdayToggled;
        private bool _isFridayToggled;
        private bool _isSaturdayToggled;
        private bool _isSundayToggled;

        //Свойства

        public bool IsMondayToggled
        {
            get => _isMondayToggled;
            set { _isMondayToggled = value;
                OnPropertyChanged();
                Preferences.Set("monday", value);
            }
        }

        public bool IsTuesdayToggled
        {
            get => _isTuesdayToggled;
            set { _isTuesdayToggled = value; 
                OnPropertyChanged();
                Preferences.Set("tuesday", value);
            }
        }

        public bool IsWednesdayToggled
        {
            get => _isWednesdayToggled;
            set { _isWednesdayToggled = value; 
                OnPropertyChanged();
                Preferences.Set("wednesday", value);
            }
        }

        public bool IsThursdayToggled
        {
            get => _isThursdayToggled;
            set { _isThursdayToggled = value; 
                OnPropertyChanged();
                Preferences.Set("thursday", value);
            }
        }

        public bool IsFridayToggled
        {
            get => _isFridayToggled;
            set { _isFridayToggled = value; 
                OnPropertyChanged();
                Preferences.Set("friday", value);
            }
        }

        public bool IsSaturdayToggled
        {
            get => _isSaturdayToggled;
            set { _isSaturdayToggled = value; 
                OnPropertyChanged();
                Preferences.Set("saturday", value);
            }
        }

        public bool IsSundayToggled
        {
            get => _isSundayToggled;
            set { _isSundayToggled = value; 
                OnPropertyChanged(); 
                Preferences.Set("sunday", value);
            }
        }


        //Команды
        public ICommand TestNotificationCommand { get; }
        public ICommand StopTestNotificationCommand { get; }

        public ICommand ToggleMondayCommand { get; }
        public ICommand ToggleTuesdayCommand { get; }
        public ICommand ToggleWednesdayCommand { get; }
        public ICommand ToggleThursdayCommand { get; }
        public ICommand ToggleFridayCommand { get; }
        public ICommand ToggleSaturdayCommand { get; }
        public ICommand ToggleSundayCommand { get; }


        //Конструктор
        public ScheduleViewModel()
        {
            IsMondayToggled = Preferences.Get("monday", false);
            IsTuesdayToggled = Preferences.Get("tuesday", false);
            IsWednesdayToggled = Preferences.Get("wednesday", false);
            IsThursdayToggled = Preferences.Get("thursday", false);
            IsFridayToggled = Preferences.Get("friday", false);
            IsSaturdayToggled = Preferences.Get("saturday", false);
            IsSundayToggled = Preferences.Get("sunday", false);

            TestNotificationCommand = new Command(async () => await TestNotification());
            StopTestNotificationCommand = new Command(async () => await StopTestNotification());

            ToggleMondayCommand = new Command(async () => await ToggleMonday());
            ToggleTuesdayCommand = new Command(async () => await ToggleTuesday());
            ToggleWednesdayCommand = new Command(async () => await ToggleWednesday());
            ToggleThursdayCommand = new Command(async () => await ToggleThursday());
            ToggleFridayCommand = new Command(async () => await ToggleFriday());
            ToggleSaturdayCommand = new Command(async () => await ToggleSaturday());
            ToggleSundayCommand = new Command(async () => await ToggleSunday());
        }

        //Методы
        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        private async Task CreateLocalNotification(int id, string title, 
            string description, DayOfWeek weekDay)
        {
            var request = new NotificationRequest
            {
                NotificationId = id,
                Title = title,
                Description = description,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = GetNextWeekday(DateTime.Today, weekDay),
                    RepeatType = NotificationRepeat.TimeInterval,
                    NotifyRepeatInterval = TimeSpan.FromDays(7)
                }
            };

            await LocalNotificationCenter.Current.Show(request);
        }
        private void CancelLocalNotification(int id)
        {
            LocalNotificationCenter.Current.Cancel(id);
        }

        private async Task ToggleMonday()
        {
            if (!IsMondayToggled)
                await CreateLocalNotification(1, "", "", DayOfWeek.Monday);
            else
                CancelLocalNotification(1);
            IsMondayToggled = !IsMondayToggled;
        }
        private async Task ToggleTuesday()
        {
            if (!IsTuesdayToggled)
                await CreateLocalNotification(2, "", "", DayOfWeek.Tuesday);
            else
                CancelLocalNotification(2);
            IsTuesdayToggled = !IsTuesdayToggled;
        }
        private async Task ToggleWednesday()
        {
            if (!IsWednesdayToggled)
                await CreateLocalNotification(3, "", "", DayOfWeek.Wednesday);
            else
                CancelLocalNotification(3);
            IsWednesdayToggled = !IsWednesdayToggled;
        }
        private async Task ToggleThursday()
        {
            if (!IsThursdayToggled)
                await CreateLocalNotification(4, "", "", DayOfWeek.Thursday);
            else
                CancelLocalNotification(4);
            IsThursdayToggled = !IsThursdayToggled;
        }
        private async Task ToggleFriday()
        {
            if (!IsFridayToggled)
                await CreateLocalNotification(5, "", "", DayOfWeek.Friday);
            else
                CancelLocalNotification(5);
            IsFridayToggled = !IsFridayToggled;
        }
        private async Task ToggleSaturday()
        {
            if (!IsSaturdayToggled)
                await CreateLocalNotification(6, "", "", DayOfWeek.Saturday);
            else
                CancelLocalNotification(6);
            IsSaturdayToggled = !IsSaturdayToggled;
        }
        private async Task ToggleSunday()
        {
            if (!IsSundayToggled)
                await CreateLocalNotification(7, "", "", DayOfWeek.Sunday);
            else
                CancelLocalNotification(7);
            IsSundayToggled = !IsSundayToggled;
        }


        private async Task TestNotification()
        {
            var request = new NotificationRequest
            {
                NotificationId = 555,
                Title = "Привет это Test",
                Description = "котьек - 🐈",
                Subtitle = "это подзаголовок!!!",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now,
                    RepeatType = NotificationRepeat.TimeInterval,
                    NotifyRepeatInterval = TimeSpan.FromSeconds(5)
                }
            };

            await LocalNotificationCenter.Current.Show(request);
            
        }

        private async Task StopTestNotification()
        {
            
            LocalNotificationCenter.Current.Cancel(555);
            
        }
    }
}
