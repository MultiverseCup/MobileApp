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

namespace Hobby
{
    public class TaskItem
    {
        public string Name { get; set; }
        public int WorkDuration { get; set; }
        public int RestDuartion { get; set; }
        public Command StartCommand { get; set; }
    }



    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimerPage : ContentPage
    {
        private int WorkDuration = 0 * 1000;   // миллисекунды
        private int RestDuration = 0 * 1000;  

        private int _timeRemaining;
        private bool _isWorking;
        private bool _isRunning;
        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();


        public TimerPage()
        {
            InitializeComponent();

            // Инициализация коллекции с командами
            Tasks = new ObservableCollection<TaskItem>
    {
        new TaskItem
        {
            Name = "Задача 1",
            WorkDuration = 25 * 60 * 1000,
            RestDuartion = 10 * 60 * 1000,
            StartCommand = new Command(() => OnTaskStart("Задача 1"))
        },
        new TaskItem
        {
            Name = "Задача 2",
            WorkDuration = 25 * 60 * 1000,
            RestDuartion = 10 * 60 * 1000,
            StartCommand = new Command(() => OnTaskStart("Задача 2"))
        },
    };

            BindingContext = this;
            ResetTimer();
        }
        public void OnTaskStart(string taskName)
        {
            WorkDuration = Tasks
                .Where(task => task.Name == taskName)
                .First()
                .WorkDuration * 1000;
            RestDuration = Tasks
                .Where(task => task.Name == taskName)
                .First()
                .RestDuartion * 1000;
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
                await System.Threading.Tasks.Task.Delay(100);  // миллисекунды
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

       
    }
}