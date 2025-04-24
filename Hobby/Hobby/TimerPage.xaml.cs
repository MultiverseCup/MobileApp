using Hobby.DataBase;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hobby
{
    public class TaskItem : Item
    {
        // Убираем Command из базы данных
        //public Command StartCommand { get; set; }
        //public Command DeleteCommand { get; set; }

        public bool IsWork { get; set; } = true;
        public bool IsRunning { get; set; }

        public string DisplayWorkDuration => DisplayTime(WorkDuration);
        public string DisplayRestDuration => DisplayTime(RestDuration);

        private string DisplayTime(int milliseconds) => TimeSpan.FromMilliseconds(milliseconds).ToString(@"mm\:ss");
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimerPage : ContentPage
    {
        // Для Pomodoro режима
        public TaskItem CurrentTaskItem { get; set; }
        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();

        // Для FreeTimer режима
        private bool _isFreeTimerRunning;
        private int _freeTimeRemaining;

        public TimerPage()
        {
            InitializeComponent();
            BindingContext = this;
            LoadInitialData();
            ModePicker.SelectedIndex = 0; // Устанавливаем Pomodoro по умолчанию
        }

        private void TaskItemsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection == null || !e.CurrentSelection.Any())
                return;

            // Получаем выбранную задачу
            CurrentTaskItem = e.CurrentSelection.First() as TaskItem;

            if (CurrentTaskItem != null)
            {
                // Обновляем отображение для Pomodoro режима
                PomodoroTimerLabel.Text = TimeSpan.FromMilliseconds(CurrentTaskItem.TimeRemaining).ToString(@"mm\:ss");
                PomodoroStartButton.Text = "Старт";
                PomodoroStartButton.BackgroundColor = Color.Orange;

                // Обновляем отображение общего времени для FreeTimer режима
                TotalTimeLabel.Text = TimeSpan.FromMilliseconds(CurrentTaskItem.TotalWorkTime).ToString(@"mm\:ss");

                // Сбрасываем FreeTimer при смене задачи
                _isFreeTimerRunning = false;
                _freeTimeRemaining = 0;
                FreeTimerLabel.Text = "00:00";
                FreeStartButton.Text = "Старт";
            }
        }

        private async void Add_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HobbyEditor(this));
        }

        private void LoadInitialData()
        {
            if (App.Db.IsEmpty())
            {
                var item1 = new Item { Name = "Учёба", WorkDuration = 300000, RestDuration = 100000, TimeRemaining = 300000 };
                var item2 = new Item { Name = "Хобби", WorkDuration = 200000, RestDuration = 50000, TimeRemaining = 200000 };
                App.Db.SaveItem(item1);
                App.Db.SaveItem(item2);
            }

            UpdateTasksFromDB(); // Обновляем список задач
        }

        public void UpdateTasksFromDB()
        {
            Tasks.Clear();
            foreach (var item in App.Db.GetItems())
            {
                Tasks.Add(new TaskItem
                {
                    ID = item.ID,
                    Name = item.Name,
                    WorkDuration = item.WorkDuration,
                    RestDuration = item.RestDuration,
                    TimeRemaining = item.TimeRemaining == 0 ? item.WorkDuration : item.TimeRemaining,
                    TotalWorkTime = item.TotalWorkTime,  // Важно! Загружаем TotalWorkTime
                });
            }

            // Если есть текущая задача, обновите соответствующий текст
            if (CurrentTaskItem != null)
                TotalTimeLabel.Text = TimeSpan.FromMilliseconds(CurrentTaskItem.TotalWorkTime).ToString(@"mm\:ss");
        }



        private void ModePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (ModePicker.SelectedIndex)
            {
                case 0: // Pomodoro
                    PomodoroContainer.IsVisible = true;
                    FreeTimerContainer.IsVisible = false;
                    break;
                case 1: // FreeTimer
                    PomodoroContainer.IsVisible = false;
                    FreeTimerContainer.IsVisible = true;
                    break;
            }
        }

        #region Pomodoro Logic
        private async void PomodoroStartButton_Clicked(object sender, EventArgs e)
        {
            // Проверка выбора задачи
            if (CurrentTaskItem == null)
            {
                await DisplayAlert("Ошибка", "Выберите задачу", "OK");
                return;
            }

            // Обработка нажатия на кнопку "Пауза"
            if (CurrentTaskItem.IsRunning)
            {
                // Останавливаем таймер
                CurrentTaskItem.IsRunning = false;
                PomodoroStartButton.Text = "Старт";

                // Сохраняем текущие изменения в базе данных
                App.Db.SaveItem(CurrentTaskItem);

                return;
            }

            // Запуск таймера
            CurrentTaskItem.IsRunning = true;
            PomodoroStartButton.Text = "Пауза";
            PomodoroStartButton.IsEnabled = false;

            while (CurrentTaskItem.IsRunning && CurrentTaskItem.TimeRemaining > 0)
            {
                await Task.Delay(100); // Ждем 100 мс

                // Уменьшаем оставшееся время
                CurrentTaskItem.TimeRemaining -= 100;

                // Добавляем время ТОЛЬКО в рабочем режиме
                if (CurrentTaskItem.IsWork)
                {
                    CurrentTaskItem.TotalWorkTime += 100;
                    // Обновляем отображение общего времени
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        TotalTimeLabel.Text = TimeSpan.FromMilliseconds(CurrentTaskItem.TotalWorkTime).ToString(@"mm\:ss");
                    });
                }

                // Сохраняем данные в базе данных на каждом шаге
                App.Db.SaveItem(CurrentTaskItem);

                // Обновляем UI
                Device.BeginInvokeOnMainThread(() =>
                {
                    PomodoroTimerLabel.Text = TimeSpan.FromMilliseconds(CurrentTaskItem.TimeRemaining).ToString(@"mm\:ss");
                    PomodoroStartButton.IsEnabled = true;
                });
            }

            // Обработка завершения интервала
            if (CurrentTaskItem.TimeRemaining <= 0 && CurrentTaskItem.IsRunning)
            {
                CurrentTaskItem.IsRunning = false;
                PomodoroStartButton.Text = "Старт";

                // Переключаем режим (работа/отдых)
                CurrentTaskItem.IsWork = !CurrentTaskItem.IsWork;

                // Устанавливаем новое время в зависимости от режима
                CurrentTaskItem.TimeRemaining = CurrentTaskItem.IsWork
                    ? CurrentTaskItem.WorkDuration
                    : CurrentTaskItem.RestDuration;

                // Меняем цвет кнопки в зависимости от режима
                Color newColor = CurrentTaskItem.IsWork ? Color.Aqua : Color.Orange;

                // Сохраняем задачу в БД (чтобы сохранить TotalWorkTime)
                App.Db.SaveItem(CurrentTaskItem);

                // Обновляем UI
                Device.BeginInvokeOnMainThread(async () =>
                {
                    PomodoroStartButton.BackgroundColor = newColor;
                    PomodoroTimerLabel.Text = TimeSpan.FromMilliseconds(CurrentTaskItem.TimeRemaining).ToString(@"mm\:ss");

                    // Показываем уведомление
                    string message = CurrentTaskItem.IsWork ? "Время работать!" : "Время отдыхать!";
                    await DisplayAlert("Помодоро", message, "OK");
                });
            }
        }


        private async void PomodoroResetButton_Clicked(object sender, EventArgs e)
        {
            if (CurrentTaskItem == null)
            {
                await DisplayAlert("Ошибка", "Выберите задачу", "OK");
                return;
            }

            // Сбрасываем таймер
            CurrentTaskItem.IsRunning = false;
            CurrentTaskItem.IsWork = true;
            CurrentTaskItem.TimeRemaining = CurrentTaskItem.WorkDuration;
            PomodoroStartButton.Text = "Старт";
            PomodoroStartButton.BackgroundColor = Color.Orange;
            PomodoroTimerLabel.Text = TimeSpan.FromMilliseconds(CurrentTaskItem.TimeRemaining).ToString(@"mm\:ss");

            // Сохраняем данные в базе данных после сброса
            App.Db.SaveItem(CurrentTaskItem);
        }

        #endregion

        #region FreeTimer Logic
        private async void FreeStartButton_Clicked(object sender, EventArgs e)
        {
            if (CurrentTaskItem == null)
            {
                await DisplayAlert("Ошибка", "Выберите задачу", "OK");
                return;
            }

            if (_isFreeTimerRunning)
            {
                _isFreeTimerRunning = false;
                FreeStartButton.Text = "Старт";

                // Сохраняем данные в базе данных, когда останавливаем таймер
                App.Db.SaveItem(CurrentTaskItem);
                return;
            }

            _isFreeTimerRunning = true;
            FreeStartButton.Text = "Пауза";

            while (_isFreeTimerRunning)
            {
                await Task.Delay(100);
                _freeTimeRemaining += 100;

                // Добавляем время ТОЛЬКО когда задача в рабочем режиме
                if (CurrentTaskItem.IsWork)
                    CurrentTaskItem.TotalWorkTime += 100;

                Device.BeginInvokeOnMainThread(() =>
                {
                    FreeTimerLabel.Text = TimeSpan.FromMilliseconds(_freeTimeRemaining).ToString(@"mm\:ss");
                    TotalTimeLabel.Text = TimeSpan.FromMilliseconds(CurrentTaskItem.TotalWorkTime).ToString(@"mm\:ss");
                });

                // Сохраняем данные в базе данных после каждого обновления
                App.Db.SaveItem(CurrentTaskItem);
            }
        }


        private void FreeResetButton_Clicked(object sender, EventArgs e)
        {
            _isFreeTimerRunning = false;
            _freeTimeRemaining = 0;
            FreeStartButton.Text = "Старт";
            FreeTimerLabel.Text = "00:00";

            // Сохраняем данные в базе данных после сброса
            App.Db.SaveItem(CurrentTaskItem);
        }


        private void ResetTotalButton_Clicked(object sender, EventArgs e)
        {
            if (CurrentTaskItem != null)
            {
                CurrentTaskItem.TotalWorkTime = 0;
                App.Db.SaveItem(CurrentTaskItem);
                TotalTimeLabel.Text = "00:00";
                // Сохраняем изменения в БД
                App.Db.SaveItem(CurrentTaskItem);
            }
        }


        #endregion
    }
}
