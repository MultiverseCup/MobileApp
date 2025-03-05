using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static System.Net.Mime.MediaTypeNames;

namespace Hobby
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TimerPage : ContentPage
	{
        private bool IsHobbyTimerRunning = false;
        private bool IsRestTimerRunning = false;
        private int HobbySeconds = 60;
        private int RestSeconds = 30;
        public TimerPage ()
		{
			InitializeComponent ();
		}
        private bool OnHobbyTimerTick()
        {
            if (HobbySeconds == 0 || !IsHobbyTimerRunning)
            {
                
                Reset();
                return IsHobbyTimerRunning;
            }

            HobbySeconds -= 1;
            TimeDisplay.Text = $"{HobbySeconds / 60}".PadLeft(2, '0') + ":" + $"{HobbySeconds % 60}".PadLeft(2, '0');
            return IsHobbyTimerRunning;
        }

        private bool OnRestTimerTick()
        {
            if (RestSeconds == 0 || !IsRestTimerRunning)
            {
                Reset();
                return IsRestTimerRunning;
            }

            RestSeconds -= 1;
            TimeDisplay.Text = $"{RestSeconds / 60}".PadLeft(2, '0') + ":" + $"{RestSeconds % 60}".PadLeft(2, '0');
            return IsRestTimerRunning;
        }
        private void StartPauseHobby_Clicked(object sender, EventArgs e)
        {
            if (!IsHobbyTimerRunning)
            {
                
                
                IsHobbyTimerRunning = true;
                StartPauseHobby.Text = "Пауза";
                StartPauseRest.IsEnabled = false;
                Device.StartTimer(TimeSpan.FromSeconds(1), OnHobbyTimerTick);
            }
            else
            {
                IsHobbyTimerRunning = false;
                StartPauseRest.IsEnabled = true;
                StartPauseHobby.Text = "Начать работу";
                
            }
        }

        private void StartPauseRest_Clicked(object sender, EventArgs e)
        {
            if (!IsRestTimerRunning)
            {
                
                
                IsRestTimerRunning = true;
                StartPauseRest.Text = "Пауза";
                StartPauseHobby.IsEnabled = false;
                Device.StartTimer(TimeSpan.FromSeconds(1), OnRestTimerTick);
            }
            else
            {
                IsRestTimerRunning = false;
                StartPauseRest.Text = "Пора отдохнуть";
                StartPauseHobby.IsEnabled = true;
            }
        }
        private void Reset()
        {
            IsRestTimerRunning = false;
            StartPauseRest.Text = "Пора отдохнуть";
            IsHobbyTimerRunning = false;
            StartPauseHobby.Text = "Начать работу";
            StartPauseHobby.IsEnabled = true;
            StartPauseRest.IsEnabled = true;
            HobbySeconds = 60;
            RestSeconds = 30;
            TimeDisplay.Text = "00:00";

        }
        private void ResetTimer_Clicked(object sender, EventArgs e)
        {
            Reset();
        }
    }
}