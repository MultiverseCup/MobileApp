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

        
        public bool IsWork { get; set; } = true;

        public bool IsRunning { get; set; }

        public string DisplayWorkDuration => TimerPage.DisplayTime(WorkDuration);
        public string DisplayRestDuration => TimerPage.DisplayTime(RestDuration);
        
    }



    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimerPage : ContentPage
    {

        public TaskItem CurrentTaskItem { get; set; }
        public ObservableCollection<TaskItem> Tasks { get; set; }

        public TimerPage()
        {
            InitializeComponent();

            BindingContext = Tasks;

            Tasks = new ObservableCollection<TaskItem>();
            
           // App.Db.ClearAll(); //удаляет все, использовал для отладки

            if (App.Db.IsEmpty())
            {
                App.Db.SaveItem(
                        new Item
                        {
                            Name = "Учёба",
                            WorkDuration = 3  * 1000, // 3 секи в мс
                            RestDuration = 10  * 1000,
                        }
                        );
                App.Db.SaveItem(
                        new Item
                        {
                            Name = "Хобби",
                            WorkDuration = 2 * 1000,
                            RestDuration = 5 * 1000,
                        }
                        );
            }

            UpdateTasksFromDB();

            if (CurrentTaskItem != null)
            {
                ResetTimer();
            }


           
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
            
        }

        private void ResetTimer()
        {
            StartButton.BackgroundColor = Color.Orange;
            CurrentTaskItem.IsWork = true;
            
            CurrentTaskItem.TimeRemaining = CurrentTaskItem.WorkDuration;
            UpdateTimerDisplay();
        }

        public void UpdateTimerDisplay()
        {
            TimerLabel.Text = DisplayTime(CurrentTaskItem.TimeRemaining);
        }

        public static string DisplayTime(int seconds) => TimeSpan.FromSeconds(seconds / 1000).ToString(@"mm\:ss");


        private async void StartButton_Clicked(object sender, EventArgs e)
        {
            if (CurrentTaskItem == null) 
            {
                await DisplayAlert("Ошибка", "Выберите задачу", "OK");
                return;
            }
            if (CurrentTaskItem.IsRunning) 
            {
                CurrentTaskItem.IsRunning = false;
                StartButton.Text = "Старт";

                return;
            }
            CurrentTaskItem.IsRunning = true;
            StartButton.Text = "Пауза";
            StartButton.IsEnabled = false;

            while (CurrentTaskItem.IsRunning && CurrentTaskItem.TimeRemaining > 0)
            {
                await System.Threading.Tasks.Task.Delay(100); // значение в миллисекундах

                if (CurrentTaskItem.IsRunning)
                    CurrentTaskItem.TimeRemaining -= 100;

                UpdateTimerDisplay();
                StartButton.IsEnabled = true;
            }

            if (CurrentTaskItem.TimeRemaining == 0 && CurrentTaskItem.IsRunning)
            {
                CurrentTaskItem.IsRunning = false;
                StartButton.Text = "Старт";

                if (CurrentTaskItem.IsWork)
                    StartButton.BackgroundColor = Color.Aqua;
                else
                {
                    StartButton.BackgroundColor = Color.Orange;
                }
                CurrentTaskItem.IsWork = !CurrentTaskItem.IsWork;
                

            CurrentTaskItem.TimeRemaining = CurrentTaskItem.IsWork
                    ? CurrentTaskItem.WorkDuration : CurrentTaskItem.RestDuration;
            await DisplayAlert("Помодоро", CurrentTaskItem.IsWork ? "Время работать!" : "Время отдыхать!", "OK");
            UpdateTimerDisplay();
            }
        }

        private async void ResetButton_Clicked(object sender, EventArgs e)
        {
            
            if (CurrentTaskItem == null)
            {
                await DisplayAlert("Ошибка", "Выберите задачу", "OK");
                return;
            }

            CurrentTaskItem.IsRunning = false;
            StartButton.Text = "Старт";
            ResetTimer();
        }

        private async void Add_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HobbyEditor(this));
        }

        private void TaskItemsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentTaskItem = e.CurrentSelection[0] as TaskItem;
            ResetTimer();
            UpdateTimerDisplay();

        }
    }
}