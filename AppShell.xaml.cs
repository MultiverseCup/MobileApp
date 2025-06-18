namespace PomodoroProject
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            this.Appearing += async (sender, e) =>
            {
                await Shell.Current.GoToAsync("//TimerPage");
            };
        }
    }
}
