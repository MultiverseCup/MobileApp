using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public PurposesPage()
        {
            InitializeComponent();
            BindingContext = this;
            LoadTasks();
            // Добавляем тестовый элемент, если список пуст
            if (!App.Db.GetScheduleItems().Any() && _tasks.Any())
            {
                var first = _tasks.First();
                App.Db.SaveScheduleItem(new DbPurposesItem
                {
                    TaskID = first.ID,
                    PlannedTime = 3600_000, // 1 час
                    Deadline = DateTime.Today.AddDays(1).ToString("o")
                });
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadSchedule();
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
            var list = App.Db.GetScheduleItems();
            foreach (var item in list)
            {
                var task = _tasks.FirstOrDefault(t => t.ID == item.TaskID);
                var vm = new ScheduleItemViewModel
                {
                    ID = item.ID,
                    TaskName = task?.Name ?? "<удалена>",
                    PlannedHours = item.PlannedTime / 3600000.0,
                    Deadline = DateTime.Parse(item.Deadline)
                };
                vm.RemainingTime = vm.PlannedHours
                    - (task?.TotalWorkTime ?? 0) / 3600000.0;
                if (vm.RemainingTime < 0) vm.RemainingTime = 0;
                ScheduleItems.Add(vm);
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

            var picked = _tasks[TaskPicker.SelectedIndex];
            var item = new DbPurposesItem
            {
                TaskID = picked.ID,
                PlannedTime = (int)(hours * 3600_000),
                Deadline = DeadlineChooser.Date.ToString("o")
            };
            App.Db.SaveScheduleItem(item);

            // Очистка полей
            PlannedTimeEntry.Text = "";
            DeadlineChooser.Date = DateTime.Today;

            LoadSchedule();
            OnCloseMenuTapped(this, EventArgs.Empty);
        }
    }

    public class ScheduleItemViewModel
    {
        public int ID { get; set; }
        public string TaskName { get; set; }
        public double PlannedHours { get; set; }
        public DateTime Deadline { get; set; }
        public double RemainingTime { get; set; }
    }
}
