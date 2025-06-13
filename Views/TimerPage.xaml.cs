using PomodoroProject.Data.Models;
using Microsoft.Maui.Controls;
using PomodoroProject.ViewModels;
using Plugin.Maui.Audio;

namespace PomodoroProject.Views
{
    public partial class TimerPage : ContentPage
    {

        private readonly TimerViewModel _viewModel;

        public TimerPage(IAudioManager audio)
        {
            InitializeComponent();
            _viewModel = new TimerViewModel(ConfirmDeleteAsync, audio);
            BindingContext = _viewModel;
        }

        private async Task<bool> ConfirmDeleteAsync(PomodoroTask task)
        {
            return await DisplayAlert("Удаление", $"Удалить задачу \"{task.Name}\"?", "Да", "Нет");
        }

    }
}
