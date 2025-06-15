using PomodoroProject.Data.Models;
using Microsoft.Maui.Controls;
using PomodoroProject.ViewModels;
using Plugin.Maui.Audio;
using System.ComponentModel;

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

        private async Task<bool> ConfirmDeleteAsync(PomodoroProject.Data.Models.PomodoroTask task)
        =>
            await DisplayAlert("Удаление", $"Удалить задачу \"{task.Name}\"?", "Да", "Нет");
    }
}
