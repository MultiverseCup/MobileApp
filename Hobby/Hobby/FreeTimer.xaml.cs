using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.IO;

namespace Hobby
{
    public partial class FreeTimer : ContentPage
    {
        private bool _isRunning;
        private int _timeRemaining = 0; // В миллисекундах
        private int _totalTime = 0; // Общее время в миллисекундах

        private string _filePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "total_time.txt");

        public FreeTimer()
        {
            InitializeComponent();
            LoadTotalTime();
        }

        private void LoadTotalTime()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string savedTime = File.ReadAllText(_filePath);
                    _totalTime = int.Parse(savedTime);
                    UpdateTimerDisplay();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке времени: {ex.Message}");
            }
        }

        private void SaveTotalTime()
        {
            try
            {
                File.WriteAllText(_filePath, _totalTime.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении времени: {ex.Message}");
            }
        }


        private void ResetTotalButton_Clicked(object sender, EventArgs e)
        {
            // Подтверждение действия
            Device.BeginInvokeOnMainThread(async () =>
            {
                bool confirm = await DisplayAlert("Сброс", "Обнулить общее время?", "Да", "Нет");
                if (confirm)
                {
                    _totalTime = 0;
                    _timeRemaining = 0;
                    _isRunning = false;

                    // Обновляем отображение
                    UpdateTimerDisplay();

                    // Удаляем файл с сохраненным временем
                    try
                    {
                        if (File.Exists(_filePath))
                        {
                            File.Delete(_filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при сбросе: {ex.Message}");
                    }

                    StartButton.Text = "Старт";
                }
            });
        }

        private async void StartButton_Clicked(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                _isRunning = false;
                StartButton.Text = "Старт";
                SaveTotalTime(); // Сохраняем при остановке
                return;
            }

            _isRunning = true;
            StartButton.Text = "Пауза";

            while (_isRunning)
            {
                await Task.Delay(100);
                _timeRemaining += 100;
                _totalTime += 100;
                UpdateTimerDisplay();
            }
        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            _isRunning = false;
            _timeRemaining = 0;
            _totalTime = (_totalTime / 1000) * 1000;
            StartButton.Text = "Старт";
            UpdateTimerDisplay();
            SaveTotalTime(); // Сохраняем при сбросе
        }

        private void UpdateTimerDisplay()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                TotalTime.Text = TimeSpan.FromMilliseconds(_totalTime).ToString(@"mm\:ss");
                TimerLabel.Text = TimeSpan.FromMilliseconds(_timeRemaining).ToString(@"mm\:ss");
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            SaveTotalTime(); // Сохраняем при закрытии страницы
        }
    }
}