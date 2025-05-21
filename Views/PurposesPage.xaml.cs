using PomodoroProject.ViewModels;

namespace PomodoroProject.Views
{
	public partial class PurposesPage : ContentPage
    {
        private readonly PurposesViewModel _viewModel;
        
        public PurposesPage()
		{
            InitializeComponent();
            _viewModel = new PurposesViewModel();
            BindingContext = _viewModel;
        }
	}
}