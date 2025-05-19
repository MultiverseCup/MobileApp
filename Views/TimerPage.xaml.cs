using PomodoroProject.Data.Models;
using Microsoft.Maui.Controls;
using PomodoroProject.ViewModels;

namespace PomodoroProject.Views
{
    public partial class TimerPage : ContentPage
    {

        private readonly TimerViewModel _viewModel;

        public TimerPage()
        {
            InitializeComponent();
            _viewModel = new TimerViewModel(ConfirmDeleteAsync);
            BindingContext = _viewModel;
        }

        private async Task<bool> ConfirmDeleteAsync(PomodoroTask task)
        {
            return await DisplayAlert("Удаление", $"Удалить задачу \"{task.Name}\"?", "Да", "Нет");
        }

    }
}
