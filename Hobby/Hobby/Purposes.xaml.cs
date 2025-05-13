using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Hobby.DataBase;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hobby
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PurposesPage : ContentPage
    {
        public ObservableCollection<ScheduleItemViewModel> ScheduleItems { get; }
            = new ObservableCollection<ScheduleItemViewModel>();

        private List<DbItem> _tasks;
        DateTime _lastReload = DateTime.MinValue;

        public ICommand ItemTappedCommand { get; }

        public PurposesPage()
        {
            InitializeComponent();
            BindingContext = this;

            ItemTappedCommand = new Command<ScheduleItemViewModel>(OnItemTapped);

            MessagingCenter.Subscribe<App>(this, "TaskTimeChanged", _ =>
                Device.BeginInvokeOnMainThread(UpdateCompletionStates));

            LoadTasks();
            LoadSchedule();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // один раз подписываемся
            MessagingCenter.Subscribe<App>(this, "TaskTimeChanged", _ =>
                Device.BeginInvokeOnMainThread(UpdateCompletionStates));
            LoadTasks();
            LoadSchedule(); // создаём VM один раз

            MessagingCenter.Subscribe<TimerPage, int>(this, "TaskDeleted", (sender, deletedTaskId) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    // Удаляем все элементы планировщика, привязанные к этой задаче
                    var toRemove = ScheduleItems.Where(si => si.TaskID == deletedTaskId).ToList();
                    foreach (var vm in toRemove)
                        ScheduleItems.Remove(vm);
                });
            });

        }

        void OnItemTapped(ScheduleItemViewModel item)
        {
            if (!item.IsRemovable) return;

            App.Db.DeletePurposesItemAsync(item.ID).Wait();
            ScheduleItems.Remove(item);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<App>(this, "TaskTimeChanged");
        }

        void LoadTasks()
        {
            _tasks = App.Db.GetItems();
            TaskPicker.ItemsSource = _tasks.Select(t => t.Name).ToList();
            if (_tasks.Any()) TaskPicker.SelectedIndex = 0;
        }

        void LoadSchedule()
        {
            ScheduleItems.Clear();
            var list = App.Db.GetPurposesItems();
            foreach (var item in list)
            {
                // ищем задачу
                var task = _tasks.FirstOrDefault(t => t.ID == item.TaskID);
                if (task == null)
                {
                    // если хотите сразу подчистить «мусор» из БД:
                    App.Db.DeletePurposesItemAsync(item.ID).Wait();
                    continue;
                }

                long elapsed = (task.TotalWorkTime) - item.InitialTotalTime;
                if (elapsed < 0) elapsed = 0;

                ScheduleItems.Add(new ScheduleItemViewModel
                {
                    ID = item.ID,
                    TaskID = item.TaskID,
                    TaskName = task.Name,
                    PlannedHours = item.PlannedTime / 3_600_000.0,
                    ElapsedHours = elapsed / 3_600_000.0,
                    Deadline = DateTime.Parse(item.Deadline),
                    IsCompleted = elapsed >= item.PlannedTime
                });
            }
        }

        void DebounceReload()
        {
            var now = DateTime.UtcNow;
            if (now - _lastReload < TimeSpan.FromSeconds(1)) return;
            _lastReload = now;
            Device.BeginInvokeOnMainThread(LoadSchedule);
        }

        void OnCompleteClicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (btn.BindingContext is ScheduleItemViewModel vm)
            {
                App.Db.DeletePurposesItemAsync(vm.ID).Wait();
                // сразу убираем и из коллекции
                ScheduleItems.Remove(vm);
            }
        }

        async void OnShowMenuClicked(object sender, EventArgs e)
        {
            Overlay.IsVisible = true;
            await Overlay.FadeTo(0.7, 250);
            await BottomMenu.TranslateTo(0, 0, 300, Easing.SinOut);
        }

        async void OnCloseMenuTapped(object sender, EventArgs e)
        {
            await BottomMenu.TranslateTo(0, 600, 300, Easing.SinIn);
            await Overlay.FadeTo(0, 200);
            Overlay.IsVisible = false;
        }

        async void OnConfirmAddSchedule(object sender, EventArgs e)
        {
            if (TaskPicker.SelectedIndex < 0
                || !double.TryParse(PlannedTimeEntry.Text, out double hours))
            {
                await DisplayAlert("Ошибка", "Заполните все поля корректно", "OK");
                return;
            }

            const double maxHours = int.MaxValue / 3600000.0; // ≈596.5
            if (hours > maxHours)
            {
                await DisplayAlert(
                    "Ошибка",
                    $"Слишком большое время (максимум {maxHours:F1} ч).",
                    "OK");
                return;
            }

            var picked = _tasks[TaskPicker.SelectedIndex];
            var plannedMs = (int)(hours * 3_600_000);

            var item = new DbPurposesItem
            {
                TaskID = picked.ID,
                PlannedTime = plannedMs,
                Deadline = DeadlineChooser.Date.ToString("o"),
                InitialTotalTime = picked.TotalWorkTime
            };

            await App.Db.SavePurposesItemAsync(item);

            LoadSchedule();

            PlannedTimeEntry.Text = "";
            DeadlineChooser.Date = DateTime.Today;

            // вместо await OnCloseMenuTapped(...)
            await ClosePopupAsync();
        }


        // Вызывается из кода, чтобы закрыть попап с await
        async Task ClosePopupAsync()
        {
            await BottomMenu.TranslateTo(0, 600, 300, Easing.SinIn);
            await Overlay.FadeTo(0, 200);
            Overlay.IsVisible = false;
        }


        void UpdateCompletionStates()
        {
            var list = App.Db.GetPurposesItems();
            foreach (var vm in ScheduleItems)
            {
                var item = list.FirstOrDefault(i => i.ID == vm.ID);
                if (item == null) continue;
                var task = _tasks.FirstOrDefault(t => t.ID == item.TaskID);
                long elapsed = (task?.TotalWorkTime ?? 0) - item.InitialTotalTime;
                if (elapsed < 0) elapsed = 0;
                bool shouldComplete = elapsed >= item.PlannedTime;
                // просто меняем свойство, которое уведомит UI
                vm.IsCompleted = shouldComplete;
            }
        }
    }

    public class ScheduleItemViewModel : INotifyPropertyChanged
    {
        public int ID { get; set; }
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public double PlannedHours { get; set; }
        public double ElapsedHours { get; set; }
        public DateTime Deadline { get; set; }

        public bool IsCompleted { get; set; }
        /// <summary>
        /// Истек ли дедлайн (включительно до дня Deadline)?
        /// </summary>
        public bool IsOverdue =>
            !IsCompleted
            && DateTime.Now.Date > Deadline.Date;

        public bool IsRemovable => IsCompleted || IsOverdue;

        public string StatusText => IsCompleted ? "Выполнено" :
                                    IsOverdue ? "Просрочено" : "";

        public Color StatusColor => IsCompleted ? Color.Green :
                                     IsOverdue ? Color.Gray : Color.Transparent;

        public bool HasStatus => IsCompleted || IsOverdue;

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class BoolToTextDecorationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? TextDecorations.Strikethrough : TextDecorations.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}