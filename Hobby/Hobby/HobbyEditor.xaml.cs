using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hobby;
using Hobby.DataBase;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hobby
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HobbyEditor : ContentPage
	{
        private TimerPage _timerPage;

        public HobbyEditor (TimerPage timerPage)
		{
            InitializeComponent();
            _timerPage = timerPage;
        }

        private async void Confirm_Clicked(object sender, EventArgs e)
        {
            if (WorkDurationEntry.Text is null || RestDurationEntry.Text is null || NameEntry.Text is null)
                await DisplayAlert(title: "Ошибка", message: "Поле не должно быть пустым", cancel: "ОК");
            else if (int.TryParse(WorkDurationEntry.Text, out var workDur) && int.TryParse(RestDurationEntry.Text, out var restDur))
            {
                App.Db.SaveItem(
                    new DbItem
                    {
                        Name = NameEntry.Text,
                        WorkDuration = workDur * 1000,
                        RestDuration = restDur * 1000,
                    }
                    );
                _timerPage.UpdateTasksFromDB();
                await Navigation.PopAsync();
            }
            else
                await DisplayAlert(title: "Ошибка", message: "Неправильные значения ввода", cancel: "ОК");
        }

        private async void Cancle_Clicked(object sender, EventArgs e)
        {
            // Возвращаемся назад, а не пушим новую страницу
            await Navigation.PopAsync();
        }
    }
}