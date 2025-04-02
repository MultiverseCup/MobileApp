using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using static System.Net.Mime.MediaTypeNames;
using Xamarin.Forms.PlatformConfiguration;
using Hobby.DataBase;

namespace Hobby
{
    public class TaskItem : Item
    {
        public Command StartCommand { get; set; }
    }



    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimerPage : ContentPage
    {
        private int WorkDuration = 0 * 1000;   // значение в мс
        private int RestDuration = 0 * 1000;  

        private int _timeRemaining;
        private bool _isWorking;
        private bool _isRunning;
        public ObservableCollection<TaskItem> Tasks { get; set; }


        public TimerPage()
        {
            InitializeComponent();
            Tasks = new ObservableCollection<TaskItem>();
            
            //App.Db.ClearAll(); удаляет все, использовал для отладки

            if (App.Db.IsEmpty())
            {
                App.Db.SaveItem(
                        new Item
                        {
                            Name = "Учёба",
                            WorkDuration = 3 * 60 * 1000, // 3 минуты в мс
                            RestDuration = 10 * 60 * 1000,
                        }
                        );
                App.Db.SaveItem(
                        new Item
                        {
                            Name = "Хобби",
                            WorkDuration = 2 * 60 * 1000,
                            RestDuration = 5 * 60 * 1000,
                        }
                        );
            }

            UpdateTasksFromDB();
            ResetTimer();
        }

        public void UpdateTasksFromDB()
        {
            Tasks = new ObservableCollection<TaskItem>();
            var items = App.Db.GetItems();
            foreach(var item in items)
            {
                Tasks.Add(new TaskItem
                {
                    ID = item.ID,
                    Name = item.Name,
                    WorkDuration = item.WorkDuration,
                    RestDuration = item.RestDuration,
                    StartCommand = new Command(() => OnTaskStart(item.ID))
                });
            }
            TaskItemsCollection.ItemsSource = Tasks;
        }
        public void OnTaskStart(int itemID)
        {
            WorkDuration = Tasks
                .Where(task => task.ID == itemID)
                .First()
                .WorkDuration;
            RestDuration = Tasks
                .Where(task => task.ID == itemID)
                .First()
                .RestDuration;
            _timeRemaining = WorkDuration;
            UpdateTimerDisplay();
        }

        private void ResetTimer()
        {
            _isWorking = true;
            _timeRemaining = WorkDuration;
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            TimerLabel.Text = TimeSpan.FromSeconds(_timeRemaining / 1000).ToString(@"mm\:ss");
        }
       
        private async void StartButton_Clicked(object sender, EventArgs e)
        {
            if (WorkDuration == 0 || RestDuration == 0) 
            {
                await DisplayAlert("Ошибка", "Выберете задачу", "OK");
                return;
            }
            if (_isRunning) 
            {
                _isRunning = false;
                StartButton.Text = "Старт";

                return;
            }
            _isRunning = true;
            StartButton.Text = "Пауза";
            StartButton.IsEnabled = false;

            while (_isRunning && _timeRemaining > 0)
            {
                await System.Threading.Tasks.Task.Delay(100); // значение в миллисекундах

                if (_isRunning)                                                  
                    _timeRemaining -= 100;

                UpdateTimerDisplay();
                StartButton.IsEnabled = true;
            }

            if (_timeRemaining == 0 && _isRunning)
            {
                _isRunning = false;
                StartButton.Text = "Старт";

                _isWorking = !_isWorking;

            _timeRemaining = _isWorking ? WorkDuration : RestDuration;
            await DisplayAlert("Помодоро", _isWorking ? "Время работать!" : "Время отдыхать!", "OK");
            UpdateTimerDisplay();
            }
        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            _isRunning = false;
            StartButton.Text = "Старт";
            ResetTimer();
        }

        private async void Add_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HobbyEditor(this));
        }

        private void TaskItemsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}