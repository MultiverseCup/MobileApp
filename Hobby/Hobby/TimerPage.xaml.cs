using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static System.Net.Mime.MediaTypeNames;

namespace Hobby
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimerPage : ContentPage
    {
        private const int WorkDuration = 10 * 1000;   // миллисекунды
        private const int BreakDuration = 10 * 1000;  

        private int _timeRemaining;
        private bool _isWorking;
        private bool _isRunning;

        private List<HobbyElement> HobbyList = new List<HobbyElement>();
        private HobbyElement CurrentHobby;


        public TimerPage()
        {
            InitializeComponent();
            ResetTimer();
            Editor.IsVisible = false;
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

            if (_timeRemaining == 0)
            {
                _isRunning = false;
                StartButton.Text = "Старт";

                // Переключаемся между работой и отдыхом
                _isWorking = !_isWorking;

            _timeRemaining = _isWorking ? WorkDuration : BreakDuration;

            // Уведомление пользователя
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

        private void Add_Clicked(object sender, EventArgs e)
        {
            CurrentTimer.IsVisible = false;
            Editor.IsVisible = true;
        }

        private void Confirm_Clicked(object sender, EventArgs e)
        {
            var hob = new HobbyElement(0, int.Parse(WorkDurationEntry.Text), int.Parse(RestDurationEntry.Text));
            hob.Text = NameEntry.Text;
            HobbyList.Add(hob);
            HobbysLayout.Children.Add(hob);

            CurrentTimer.IsVisible = true;
            Editor.IsVisible = false;
        }
    }
    class HobbyElement : Button
    {
        int TotalTime = 0;
        int WorkDuration = 10 * 1000; 
        int BreakDuration = 5 * 1000;

        public HobbyElement(int totalTime, int workDuration, int breakDuration)
        {
            TotalTime = totalTime;
            WorkDuration = workDuration;
            BreakDuration = breakDuration;
        }
    }
}