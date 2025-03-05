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
        private const int WorkDuration = 10; // 25 минут в секундах
        private const int BreakDuration = 5;  // 5 минут в секундах

        private int _timeRemaining;
        private bool _isWorking;
        private bool _isRunning;

        public TimerPage()
        {
            InitializeComponent();
            ResetTimer();
        }

        private void ResetTimer()
        {
            StartButton.BackgroundColor = Color.Orange;
            Status.Text = "Работаем";
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
            if (sender is null)
                throw new ArgumentNullException(nameof(sender));

            if (_isRunning)
            {
                _isRunning = false;
                StartButton.Text = "Старт";

                return;
            }

            _isRunning = true;
            StartButton.Text = "Пауза";


            while (_isRunning && _timeRemaining > 0)
            {
                await System.Threading.Tasks.Task.Delay(1000); // Ждем 1 секунду
                _timeRemaining--;
                UpdateTimerDisplay();
            }

            if (_timeRemaining == 0)
            {
                _isRunning = false;
                StartButton.Text = "Старт";

                // Переключаемся между работой и отдыхом
                _isWorking = !_isWorking;
            if (_isWorking)
            {
                StartButton.BackgroundColor = Color.Orange;
                Status.Text = "Работаем";
            }
            else
            {
                StartButton.BackgroundColor = Color.Aquamarine;
                Status.Text = "Откисаем";
            }
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
    }
}