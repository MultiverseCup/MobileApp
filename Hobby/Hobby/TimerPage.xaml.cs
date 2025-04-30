using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Hobby.DataBase;
using SQLite;

namespace Hobby
{
    // Класс TaskItem с улучшениями из новой версии
    public class TaskItem : Item
    {
        [Ignore]
        public Command DeleteCommand { get; set; }

        public bool IsWork { get; set; } = true;
        public bool IsRunning { get; set; }

        public string DisplayWorkDuration => DisplayTime(WorkDuration);
        public string DisplayRestDuration => DisplayTime(RestDuration);

        private string DisplayTime(int milliseconds) =>
            TimeSpan.FromMilliseconds(milliseconds).ToString(@"mm\:ss");
    }



    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimerPage : ContentPage
    {
        // Восстанавливаем старый нейминг с новым функционалом
        public TaskItem CurrentTaskItem { get; set; }
        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();

        // Переносим новый функционал с сохранением старого нейминга
        private bool _isFreeTimerRunning;
        private int _freeTimeRemaining;

        public TimerPage()
        {
            InitializeComponent();
            BindingContext = this;
            LoadInitialData();

            // Инициализация режимов как в новой версии
            ModePicker.SelectedIndex = 0;
        }

        private void TaskItemsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection == null || !e.CurrentSelection.Any()) return;

            // Получаем выбранную задачу
            CurrentTaskItem = e.CurrentSelection.First() as TaskItem;

            if (CurrentTaskItem != null)
            {
                // Обновляем отображение для Pomodoro режима
                PomodoroTimerLabel.Text = TimeSpan.FromMilliseconds(CurrentTaskItem.TimeRemaining).ToString(@"mm\:ss");
                PomodoroStartButton.ImageSource = "play.png";
                PomodoroStartButton.BackgroundColor = Color.Orange;

                // Обновляем отображение общего времени для FreeTimer режима
                TotalTimeLabel.Text = TimeSpan.FromMilliseconds(CurrentTaskItem.TotalWorkTime).ToString(@"mm\:ss");

                // Сбрасываем FreeTimer при смене задачи
                _isFreeTimerRunning = false;
                _freeTimeRemaining = 0;
                FreeTimerLabel.Text = "00:00";
                FreeStartButton.ImageSource = "play.png";
            }
        }

        private async void Add_Clicked(object sender, EventArgs e) 
            => await Navigation.PushAsync(new HobbyEditor(this));


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

        // Старый метод удаления с улучшениями из новой версии
        private async void DeleteTask(int taskId)
        {
            bool confirm = await DisplayAlert("Удаление", "Удалить задачу?", "Да", "Нет");
            if (!confirm) return;

            App.Db.DeleteItem(taskId);

            var taskToRemove = Tasks.FirstOrDefault(t => t.ID == taskId);
            if (taskToRemove != null)
            {
                Tasks.Remove(taskToRemove);

                if (CurrentTaskItem?.ID == taskId)
                {
                    CurrentTaskItem = null;
                    // Обновляем UI как в новой версии
                    PomodoroTimerLabel.Text = "00:00";
                    PomodoroStartButton.ImageSource = "play.png";
                    
                }
            }
        }

        // Обновлённый метод загрузки задач
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
                    TotalWorkTime = item.TotalWorkTime,
                    DeleteCommand = new Command(() => DeleteTask(item.ID))
                });
            }
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
                PomodoroStartButton.ImageSource = "play.png";

                // Сохраняем текущие изменения в базе данных
                App.Db.SaveItem(CurrentTaskItem);

                return;
            }

            // Запуск таймера
            CurrentTaskItem.IsRunning = true;
            PomodoroStartButton.ImageSource = "pause.png";
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
                PomodoroStartButton.ImageSource = "play.png";

                // Переключаем режим (работа/отдых)
                CurrentTaskItem.IsWork = !CurrentTaskItem.IsWork;

                // Устанавливаем новое время в зависимости от режима
                CurrentTaskItem.TimeRemaining = CurrentTaskItem.IsWork
                    ? CurrentTaskItem.WorkDuration
                    : CurrentTaskItem.RestDuration;

                // Меняем цвет кнопки в зависимости от режима
                //Color newColor = CurrentTaskItem.IsWork ? Color.Aqua : Color.Orange;

                // Сохраняем задачу в БД (чтобы сохранить TotalWorkTime)
                App.Db.SaveItem(CurrentTaskItem);

                // Обновляем UI
                Device.BeginInvokeOnMainThread(async () =>
                {
                    //PomodoroStartButton.BackgroundColor = newColor;
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
            PomodoroStartButton.ImageSource = "play.png";
            //PomodoroStartButton.BackgroundColor = Color.Orange;
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
                FreeStartButton.ImageSource = "play.png";

                // Сохраняем данные в базе данных, когда останавливаем таймер
                App.Db.SaveItem(CurrentTaskItem);
                return;
            }

            _isFreeTimerRunning = true;
            FreeStartButton.ImageSource = "pause.png";

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
            FreeStartButton.ImageSource = "play.png";
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