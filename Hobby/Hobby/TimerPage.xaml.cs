using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Hobby.DataBase;
using SQLite;

namespace Hobby
{
    public class TaskItem : DbItem
    {
        [Ignore]
        public Command DeleteCommand { get; set; }
        public bool IsWork { get; set; } = true;
        public bool IsRunning { get; set; }
        public string DisplayWorkDuration =>
            TimeSpan.FromMilliseconds(WorkDuration).ToString(@"mm\:ss");
        public string DisplayRestDuration =>
            TimeSpan.FromMilliseconds(RestDuration).ToString(@"mm\:ss");
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimerPage : ContentPage
    {
        public TaskItem CurrentTaskItem { get; set; }
        public ObservableCollection<TaskItem> Tasks { get; set; }
            = new ObservableCollection<TaskItem>();

        private bool _isFreeTimerRunning;
        private int _freeTimeRemaining;

        void Log(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DebugConsole.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            });
        }

        private void SaveCurrentTaskToDb()
        {
            try
            {
                // Создаем копию ВСЕХ полей из TaskItem в DbItem
                var dbItem = new DbItem
                {
                    ID = CurrentTaskItem.ID,
                    Name = CurrentTaskItem.Name,
                    WorkDuration = CurrentTaskItem.WorkDuration,
                    RestDuration = CurrentTaskItem.RestDuration,
                    TimeRemaining = CurrentTaskItem.TimeRemaining,
                    TotalWorkTime = CurrentTaskItem.TotalWorkTime,

                    // Эти поля должны быть в DbItem:
                    WorkTimePerDay = CurrentTaskItem.WorkTimePerDay,
                    Schedule = CurrentTaskItem.Schedule,
                    BoxColor = CurrentTaskItem.BoxColor
                };

                App.Db.SaveItem(dbItem);
                Log($"Сохранено: ID={dbItem.ID}, Total={dbItem.TotalWorkTime}");
            }
            catch (Exception ex)
            {
                Log($"Ошибка сохранения: {ex.Message}");
            }
        }

        public TimerPage()
        {
            InitializeComponent();
            BindingContext = this;

            // Дебаг-консоль: выводим список колонок БД
            var cols = App.Db.GetColumns();
            Log("DB columns: " + string.Join(", ", cols));

            LoadInitialData();
            FreeTimerContainer.IsVisible = false;

            MessagingCenter.Subscribe<App>(this, "AppGoingToSleep", sender =>
            {
                if (CurrentTaskItem != null)
                    App.Db.SaveItem(CurrentTaskItem);
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateTasksFromDB();

            if (CurrentTaskItem == null && Tasks.Any())
                CurrentTaskItem = Tasks.First();

            if (CurrentTaskItem != null)
                RefreshAllTimersInUI();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<App>(this, "AppGoingToSleep");
            if (CurrentTaskItem != null)
                App.Db.SaveItem(CurrentTaskItem);
        }

        private void RefreshAllTimersInUI()
        {
            PomodoroTimerLabel.Text =
                TimeSpan.FromMilliseconds(CurrentTaskItem.TimeRemaining)
                .ToString(@"mm\:ss");
            FreeTimerLabel.Text =
                TimeSpan.FromMilliseconds(_freeTimeRemaining)
                .ToString(@"mm\:ss");
            TotalTimeLabel.Text =
                TimeSpan.FromMilliseconds(CurrentTaskItem.TotalWorkTime)
                .ToString(@"hh\:mm\:ss");
        }

        private void LoadInitialData()
        {

            if (App.Db.GetItems().Count == 0)
            {
                var item1 = new DbItem
                {
                    Name = "Учёба",
                    WorkDuration = 300_000,
                    RestDuration = 100_000,
                    TimeRemaining = 300_000,
                    TotalWorkTime = 0
                };
                App.Db.SaveItem(item1);

                var item2 = new DbItem
                {
                    Name = "Хобби",
                    WorkDuration = 200_000,
                    RestDuration = 50_000,
                    TimeRemaining = 200_000,
                    TotalWorkTime = 0
                };
                App.Db.SaveItem(item2);
            }
        }

        private void UpdateTasksFromDB()
        {
            Tasks.Clear();
            foreach (var itm in App.Db.GetItems())
            {
                var ti = new TaskItem
                {
                    ID = itm.ID,
                    Name = itm.Name,
                    WorkDuration = itm.WorkDuration,
                    RestDuration = itm.RestDuration,
                    TimeRemaining = itm.TimeRemaining == 0
                        ? itm.WorkDuration
                        : itm.TimeRemaining,
                    TotalWorkTime = itm.TotalWorkTime,
                    DeleteCommand = new Command(() => DeleteTask(itm.ID))
                };
                Tasks.Add(ti);
            }
            TaskItemsCollection.HeightRequest = Tasks.Count * 90;
        }

        private async void TaskItemsCollection_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (!e.CurrentSelection.Any()) return;
            CurrentTaskItem = e.CurrentSelection.First() as TaskItem;
            _isFreeTimerRunning = false;
            _freeTimeRemaining = 0;
            RefreshAllTimersInUI();
        }

        #region Pomodoro Logic

        private async void PomodoroStartButton_Clicked(object sender, EventArgs e)
        {
            if (CurrentTaskItem == null)
            {
                await DisplayAlert("Ошибка", "Выберите задачу", "OK");
                return;
            }

            if (CurrentTaskItem.IsRunning)
            {
                CurrentTaskItem.IsRunning = false;
                PomodoroStartButtonImage.Source = PicSource("play.png");

                // Сохраняем при остановке
                Device.BeginInvokeOnMainThread(() => SaveCurrentTaskToDb());
                return;
            }

            CurrentTaskItem.IsRunning = true;
            PomodoroStartButtonImage.Source = PicSource("pause.png");

            while (CurrentTaskItem.IsRunning && CurrentTaskItem.TimeRemaining > 0)
            {
                await Task.Delay(100);
                CurrentTaskItem.TimeRemaining -= 100;

                if (CurrentTaskItem.IsWork)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        CurrentTaskItem.TotalWorkTime += 100;

                        SaveCurrentTaskToDb();

                        PomodoroTimerLabel.Text = TimeSpan
                            .FromMilliseconds(CurrentTaskItem.TimeRemaining)
                            .ToString(@"mm\:ss");
                        TotalTimeLabel.Text = TimeSpan
                            .FromMilliseconds(CurrentTaskItem.TotalWorkTime)
                            .ToString(@"hh\:mm\:ss");
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        PomodoroTimerLabel.Text = TimeSpan
                            .FromMilliseconds(CurrentTaskItem.TimeRemaining)
                            .ToString(@"mm\:ss");
                    });
                }
            }
        }


        private void PomodoroResetButton_Clicked(
            object sender, EventArgs e)
        {
            if (CurrentTaskItem == null) return;
            CurrentTaskItem.IsRunning = false;
            CurrentTaskItem.IsWork = true;
            CurrentTaskItem.TimeRemaining = CurrentTaskItem.WorkDuration;
            PomodoroStartButtonImage.Source = PicSource("play.png");
            PomodoroTimerLabel.Text =
                TimeSpan.FromMilliseconds(
                    CurrentTaskItem.TimeRemaining)
                .ToString(@"mm\:ss");
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
                FreeStartButtonImage.Source = PicSource("play.png");

                // Сохраняем при остановке таймера
                Device.BeginInvokeOnMainThread(() => SaveCurrentTaskToDb());
                return;
            }

            _isFreeTimerRunning = true;
            FreeStartButtonImage.Source = PicSource("pause.png");

            while (_isFreeTimerRunning)
            {
                await Task.Delay(100);
                _freeTimeRemaining += 100;

                if (CurrentTaskItem.IsWork)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        CurrentTaskItem.TotalWorkTime += 100;

                        SaveCurrentTaskToDb();

                        FreeTimerLabel.Text = TimeSpan
                            .FromMilliseconds(_freeTimeRemaining)
                            .ToString(@"mm\:ss");
                        TotalTimeLabel.Text = TimeSpan
                            .FromMilliseconds(CurrentTaskItem.TotalWorkTime)
                            .ToString(@"hh\:mm\:ss");
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        FreeTimerLabel.Text = TimeSpan
                            .FromMilliseconds(_freeTimeRemaining)
                            .ToString(@"mm\:ss");
                    });
                }

                var dbItem = App.Db.GetItems().FirstOrDefault(i => i.ID == CurrentTaskItem.ID);
                Log($"FreeTimer старт. ID из БД: {CurrentTaskItem.ID}");
                Log($"FreeTimer старт. TotalWorkTime из БД: {CurrentTaskItem.TotalWorkTime}");
            }
        }

        private void FreeResetButton_Clicked(
            object sender, EventArgs e)
        {
            if (CurrentTaskItem == null) return;
            _isFreeTimerRunning = false;
            _freeTimeRemaining = 0;
            FreeStartButtonImage.Source = PicSource("play.png");
            FreeTimerLabel.Text = "00:00";
        }

        private void ResetTotalButton_Clicked(
            object sender, EventArgs e)
        {
            if (CurrentTaskItem == null) return;
            CurrentTaskItem.TotalWorkTime = 0;
            TotalTimeLabel.Text = "00:00:00";
        }

        #endregion

        private void SelectedMode_Clicked(object sender, EventArgs e)
        {
            PickerBackground.IsVisible = !PickerBackground.IsVisible;
            UnSelectedMode.IsVisible = !UnSelectedMode.IsVisible;
            ModePickerArrow.Source = PickerBackground.IsVisible
                ? PicSource("arrowUp.png")
                : PicSource("arrowDown.png");
        }

        private void UnSelectedMode_Clicked(object sender, EventArgs e)
        {
            string tmp = SelectedMode.Text;
            SelectedMode.Text = UnSelectedMode.Text;
            UnSelectedMode.Text = tmp;
            PomodoroContainer.IsVisible = !PomodoroContainer.IsVisible;
            FreeTimerContainer.IsVisible = !FreeTimerContainer.IsVisible;
        }

        private ImageSource PicSource(string s) =>
            ImageSource.FromResource("Hobby.Images." + s);

        private async void OnShowMenuClicked(object sender, EventArgs e)
        {
            Overlay.IsVisible = true;
            await Overlay.FadeTo(0.7, 250);
            await BottomMenu.TranslateTo(0, 0, 300, Easing.SinOut);
        }

        private async void OnCloseMenuTapped(object sender, EventArgs e)
        {
            await BottomMenu.TranslateTo(0, 500, 300, Easing.SinIn);
            await Overlay.FadeTo(0, 250);
            Overlay.IsVisible = false;
        }

        private async void DeleteTask(int taskId)
        {
            bool ok = await DisplayAlert("Удаление", "Удалить задачу?", "Да", "Нет");
            if (!ok) return;
            App.Db.DeleteItem(taskId);
            var toRemove = Tasks.FirstOrDefault(t => t.ID == taskId);
            if (toRemove != null)
                Tasks.Remove(toRemove);
            TaskItemsCollection.HeightRequest = Tasks.Count * 90;
        }

        private async void ConfirmNew_Clicked(object sender, EventArgs e)
        {
            if (WorkDurationEntryMinutes.Text == null ||
                RestDurationEntryMinutes.Text == null ||
                WorkDurationEntrySeconds.Text == null ||
                RestDurationEntrySeconds.Text == null ||
                NameEntry.Text == null)
            {
                await DisplayAlert("Ошибка", "Поле не должно быть пустым", "ОК");
                return;
            }

            if (int.TryParse(WorkDurationEntryMinutes.Text, out int wMin) &&
                int.TryParse(WorkDurationEntrySeconds.Text, out int wSec) &&
                int.TryParse(RestDurationEntryMinutes.Text, out int rMin) &&
                int.TryParse(RestDurationEntrySeconds.Text, out int rSec))
            {
                var newItem = new DbItem
                {
                    Name = NameEntry.Text,
                    WorkDuration = (wMin * 60 + wSec) * 1000,
                    RestDuration = (rMin * 60 + rSec) * 1000,
                    TimeRemaining = (wMin * 60 + wSec) * 1000,
                    TotalWorkTime = 0
                };
                App.Db.SaveItem(newItem);
                await BottomMenu.TranslateTo(0, 500, 300, Easing.SinIn);
                await Overlay.FadeTo(0, 250);
                Overlay.IsVisible = false;
                UpdateTasksFromDB();
                NameEntry.Text = "";
                WorkDurationEntryMinutes.Text = "25";
                WorkDurationEntrySeconds.Text = "00";
                RestDurationEntryMinutes.Text = "5";
                RestDurationEntrySeconds.Text = "00";
            }
            else
            {
                await DisplayAlert("Ошибка", "Неправильные значения ввода", "ОК");
            }
        }
    }
}
