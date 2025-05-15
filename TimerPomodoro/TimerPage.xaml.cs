using TimerPomodoro.ViewModel;

namespace TimerPomodoro;

public partial class TimerPage : ContentPage
{
	public TimerPage(TimerViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}