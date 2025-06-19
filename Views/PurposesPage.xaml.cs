using PomodoroProject.Data.Models;
using PomodoroProject.ViewModels;

namespace PomodoroProject.Views
{
	public partial class PurposesPage : ContentPage
    {
        private readonly PurposesViewModel _viewModel;
        
        public PurposesPage()
		{
            InitializeComponent();
            _viewModel = new PurposesViewModel(ConfirmDeleteAsync);
            BindingContext = _viewModel;
        }

        private async Task<bool> ConfirmDeleteAsync(TaskDeadline deadline)
        {
            return await DisplayAlert("Удаление", $"Удалить дэдлайн \"{deadline.DeadlineName}\"?", "Да", "Нет");
        }
    }
}