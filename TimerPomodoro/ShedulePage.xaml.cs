using TimerPomodoro.ViewModel;

namespace TimerPomodoro;

public partial class ShedulePage : ContentPage
{
	public ShedulePage(SheduleViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}