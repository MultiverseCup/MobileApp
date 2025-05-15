using Microsoft.Maui.Controls;
using PomodoroProject.ViewModels;

namespace PomodoroProject.Views
{
    public partial class TimerPage : ContentPage
    {
        public TimerPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is TimerViewModel vm)
                vm.LoadTasksCommand.Execute(null);
        }
    }
}
