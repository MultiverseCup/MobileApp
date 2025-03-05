using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Shapes;

namespace Test_mobile
{
    public partial class MainPage : ContentPage
    {
        private const int WorkDuration = 2; // 25 минут в секундах
        private const int BreakDuration = 2;  // 5 минут в секундах

        private int _timeRemaining;
        private bool _isWorking;
        private bool _isRunning;

        public MainPage()
        {
            InitializeComponent();
            ResetTimer();
        }

        private void ResetTimer()
        {
            _isWorking = true;
            _timeRemaining = WorkDuration;
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            TimerLabel.Text = TimeSpan.FromSeconds(_timeRemaining).ToString(@"mm\:ss");
        }

        private async void StartButton_Clicked(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                _isRunning = false;
                StartButton.Text = "Start";
                return;
            }

            _isRunning = true;
            StartButton.Text = "Pause";

            while (_isRunning && _timeRemaining > 0)
            {
                await System.Threading.Tasks.Task.Delay(1000); // Ждем 1 секунду
                _timeRemaining--;
                UpdateTimerDisplay();
            }

            if (_timeRemaining == 0)
            {
                _isRunning = false;
                StartButton.Text = "Start";

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
            StartButton.Text = "Start";
            ResetTimer();
        }
    }
}
