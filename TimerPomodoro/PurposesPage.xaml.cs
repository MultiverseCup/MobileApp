using TimerPomodoro.ViewModel;

namespace TimerPomodoro;

public partial class PurposesPage : ContentPage
{
	public PurposesPage(PurposesViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}