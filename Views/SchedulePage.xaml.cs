using PomodoroProject.Data.Models;
using PomodoroProject.ViewModels;


namespace PomodoroProject.Views;


public partial class SchedulePage : ContentPage
{
    private readonly ScheduleViewModel _viewModel;


    public SchedulePage()
	{
		InitializeComponent();
        _viewModel = new ScheduleViewModel();
        BindingContext = _viewModel;
    }
}