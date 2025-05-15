using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Hobby.DataBase;
using SQLite;
using System.ComponentModel;

namespace Hobby
{
    public class TaskItem : DbItem, INotifyPropertyChanged
    {
        [Ignore] public Command DeleteCommand { get; set; }

        private bool _isWork = true;
        public bool IsWork
        {
            get => _isWork;
            set { _isWork = value; OnPropertyChanged(nameof(IsWork)); }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(nameof(IsRunning)); }
        }

        // Используем базовые поля WorkDuration и RestDuration из DbItem
        public string DisplayWorkDuration =>
            TimeSpan.FromMilliseconds(WorkDuration).ToString(@"mm\:ss");
        public string DisplayRestDuration =>
            TimeSpan.FromMilliseconds(RestDuration).ToString(@"mm\:ss");

        // Новый свойство для отображения всего
        public string DisplayTotalTime =>
            TimeSpan.FromMilliseconds(TotalWorkTime).ToString(@"hh\:mm\:ss");

        // Когда меняется TotalWorkTime, отсылаем PropertyChanged для DisplayTotalTime
        public new long TotalWorkTime
        {
            get => base.TotalWorkTime;
            set
            {
                base.TotalWorkTime = value;
                OnPropertyChanged(nameof(TotalWorkTime));
                OnPropertyChanged(nameof(DisplayTotalTime));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimerPage : ContentPage
    {
        public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();
        public TaskItem CurrentTaskItem { get; set; }

        private bool _isFreeTimerRunning;
        private long _freeTimeRemaining;

        public TimerPage()
        {
            InitializeComponent();
            BindingContext = this;

            LoadInitialData();
            FreeTimerContainer.IsVisible = false;

            // Сохраняем при уходе в фон
            MessagingCenter.Subscribe<App>(this, "AppGoingToSleep", _ =>
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
            RefreshAllTimersInUI();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<App>(this, "AppGoingToSleep");
            if (CurrentTaskItem != null)
                App.Db.SaveItem(CurrentTaskItem);
        }

        void LoadInitialData()
        {
            if (!App.Db.GetItems().Any())
            {
                App.Db.SaveItem(new DbItem
                {
                    Name = "Учёба",
                    WorkDuration = 300_000L,
                    RestDuration = 100_000L,
                    TimeRemaining = 300_000L,
                    TotalWorkTime = 0L
                });
                App.Db.SaveItem(new DbItem
                {
                    Name = "Хобби",
                    WorkDuration = 200_000L,
                    RestDuration = 50_000L,
                    TimeRemaining = 200_000L,
                    TotalWorkTime = 0L
                });
            }
        }

        void UpdateTasksFromDB()
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
                    TimeRemaining = itm.TimeRemaining == 0L ? itm.WorkDuration : itm.TimeRemaining,
                    TotalWorkTime = itm.TotalWorkTime,
                    DeleteCommand = new Command(() => DeleteTask(itm.ID))
                };
                Tasks.Add(ti);
            }
            TaskItemsCollection.HeightRequest = Tasks.Count * 90;
        }

        void RefreshAllTimersInUI()
        {
            if (CurrentTaskItem == null) return;
            PomodoroTimerLabel.Text = TimeSpan
                .FromMilliseconds(CurrentTaskItem.TimeRemaining)
                .ToString(@"mm\:ss");
            FreeTimerLabel.Text = TimeSpan
                .FromMilliseconds(_freeTimeRemaining)
                .ToString(@"mm\:ss");
            TotalTimeLabel.Text = TimeSpan
                .FromMilliseconds(CurrentTaskItem.TotalWorkTime)
                .ToString(@"hh\:mm\:ss");
        }

        async void TaskItemsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!e.CurrentSelection.Any()) return;
            CurrentTaskItem = e.CurrentSelection.First() as TaskItem;
            _isFreeTimerRunning = false;
            _freeTimeRemaining = 0;
            RefreshAllTimersInUI();
        }


        #region Pomodoro

        async void PomodoroStartButton_Clicked(object sender, EventArgs e)
        {
            if (CurrentTaskItem == null)
            {
                await DisplayAlert("Ошибка", "Выберите задачу", "OK");
                return;
            }

            if (CurrentTaskItem.IsRunning)
            {
                // остановка
                CurrentTaskItem.IsRunning = false;
                PomodoroStartButtonImage.Source = PicSource("play.png");
                App.Db.SaveItem(CurrentTaskItem);
                return;
            }

            // запуск
            CurrentTaskItem.IsRunning = true;
            PomodoroStartButtonImage.Source = PicSource("pause.png");

            while (CurrentTaskItem.IsRunning)
            {
                await Task.Delay(100);
                CurrentTaskItem.TimeRemaining -= 100;
                if (CurrentTaskItem.IsWork)
                {
                    // при увеличении TotalWorkTime автоматически вызовется OnPropertyChanged
                    CurrentTaskItem.TotalWorkTime += 100;
                }
                Device.BeginInvokeOnMainThread(RefreshAllTimersInUI);
                App.Db.SaveItem(CurrentTaskItem);

                if (CurrentTaskItem.TimeRemaining <= 0)
                {
                    CurrentTaskItem.IsRunning = false;
                    CurrentTaskItem.IsWork = !CurrentTaskItem.IsWork;
                    CurrentTaskItem.TimeRemaining = CurrentTaskItem.IsWork
                        ? CurrentTaskItem.WorkDuration
                        : CurrentTaskItem.RestDuration;
                    PomodoroStartButtonImage.Source = PicSource("play.png");
                    await DisplayAlert("Помодоро",
                        CurrentTaskItem.IsWork ? "Пора работать!" : "Пора отдыхать!",
                        "OK");
                    App.Db.SaveItem(CurrentTaskItem);
                    break;
                }
            }
        }

        void PomodoroResetButton_Clicked(object sender, EventArgs e)
        {
            if (CurrentTaskItem == null) return;
            CurrentTaskItem.IsRunning = false;
            CurrentTaskItem.IsWork = true;
            CurrentTaskItem.TimeRemaining = CurrentTaskItem.WorkDuration;
            PomodoroStartButtonImage.Source = PicSource("play.png");
            PomodoroTimerLabel.Text = TimeSpan
                .FromMilliseconds(CurrentTaskItem.TimeRemaining)
                .ToString(@"mm\:ss");
            App.Db.SaveItem(CurrentTaskItem);
        }


        #endregion

        #region FreeTimer

        async void FreeStartButton_Clicked(object sender, EventArgs e)
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
                App.Db.SaveItem(CurrentTaskItem);
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
                    CurrentTaskItem.TotalWorkTime += 100;
                    App.Db.SaveItem(CurrentTaskItem);
                }
                Device.BeginInvokeOnMainThread(RefreshAllTimersInUI);
            }
        }

        void FreeResetButton_Clicked(object sender, EventArgs e)
        {
            if (CurrentTaskItem == null) return;
            _isFreeTimerRunning = false;
            _freeTimeRemaining = 0;
            FreeStartButtonImage.Source = PicSource("play.png");
            FreeTimerLabel.Text = "00:00";
        }


        void ResetTotalButton_Clicked(object sender, EventArgs e)
        {
            if (CurrentTaskItem == null) return;
            CurrentTaskItem.TotalWorkTime = 0;
            TotalTimeLabel.Text = "00:00:00";
            App.Db.SaveItem(CurrentTaskItem);
        }


        #endregion

        #region ModePicker

        void SelectedMode_Clicked(object sender, EventArgs e)
        {
            PickerBackground.IsVisible = !PickerBackground.IsVisible;
            UnSelectedMode.IsVisible = !UnSelectedMode.IsVisible;
            ModePickerArrow.Source = PickerBackground.IsVisible
                ? PicSource("arrowUp.png")
                : PicSource("arrowDown.png");
        }

        void UnSelectedMode_Clicked(object sender, EventArgs e)
        {
            var tmp = SelectedMode.Text;
            SelectedMode.Text = UnSelectedMode.Text;
            UnSelectedMode.Text = tmp;
            PomodoroContainer.IsVisible = !PomodoroContainer.IsVisible;
            FreeTimerContainer.IsVisible = !FreeTimerContainer.IsVisible;
        }

        #endregion

        #region PopupMenu

        ImageSource PicSource(string file) =>
            ImageSource.FromResource("Hobby.Images." + file);

        async void OnShowMenuClicked(object sender, EventArgs e)
        {
            Overlay.IsVisible = true;
            await Overlay.FadeTo(0.7, 250);
            await BottomMenu.TranslateTo(0, 0, 300, Easing.SinOut);
        }

        async void OnCloseMenuTapped(object sender, EventArgs e)
        {
            await BottomMenu.TranslateTo(0, 500, 300, Easing.SinIn);
            await Overlay.FadeTo(0, 250);
            Overlay.IsVisible = false;
        }

        #endregion

        async void ConfirmNew_Clicked(object sender, EventArgs e)
        {
            if (NameEntry.Text == null ||
                WorkDurationEntryMinutes.Text == null ||
                WorkDurationEntrySeconds.Text == null ||
                RestDurationEntryMinutes.Text == null ||
                RestDurationEntrySeconds.Text == null)
            {
                await DisplayAlert("Ошибка", "Все поля обязательны", "OK");
                return;
            }

            if (int.TryParse(WorkDurationEntryMinutes.Text, out var wMin) &&
                int.TryParse(WorkDurationEntrySeconds.Text, out var wSec) &&
                int.TryParse(RestDurationEntryMinutes.Text, out var rMin) &&
                int.TryParse(RestDurationEntrySeconds.Text, out var rSec))
            {
                var newItem = new DbItem
                {
                    Name = NameEntry.Text,
                    WorkDuration = (wMin * 60 + wSec) * 1000L,
                    RestDuration = (rMin * 60 + rSec) * 1000L,
                    TimeRemaining = (wMin * 60 + wSec) * 1000L,
                    TotalWorkTime = 0L
                };
                App.Db.SaveItem(newItem);
                OnCloseMenuTapped(null, null);
                UpdateTasksFromDB();
                NameEntry.Text = "";
            }
            else
            {
                await DisplayAlert("Ошибка", "Неверный формат времени", "OK");
            }
        }

        private async void DeleteTask(int taskId)
        {
            bool ok = await DisplayAlert("Удаление", "Удалить задачу?", "Да", "Нет");
            if (!ok) return;

            // 1) Удаляем саму задачу
            App.Db.DeleteItem(taskId);

            // 2) Удаляем все связанные планы
            App.Db.DeletePurposesForTask(taskId);

            // 3) Уведомляем другие страницы, что задача удалена
            MessagingCenter.Send(this, "TaskDeleted", taskId);

            // 4) Обновляем UI текущей страницы
            var toRemove = Tasks.FirstOrDefault(t => t.ID == taskId);
            if (toRemove != null)
                Tasks.Remove(toRemove);
            TaskItemsCollection.HeightRequest = Tasks.Count * 90;
        }
    }
}
