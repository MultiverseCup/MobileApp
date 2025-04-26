using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Forms.PlatformConfiguration;
using Hobby.DataBase;
using SQLite;

namespace Hobby
{
    public partial class MainPage : ContentPage
    {
        public ContentPage CurrentPage { get; private set; }

        private ContentPage[] _pages;
        private Button[] _menuButtons;
        private Image[] _menuButtonsImages;

        public MainPage()
        {
            InitializeComponent();

            // Инициализация страниц
            _pages = new ContentPage[]
            {
            new Shedule(),   // Ваша кастомная страница
            new TimerPage(),
            new Purposes()
            };

            // Инициализация кнопок меню
            _menuButtons = new[]
            {
            (Button)FindByName("Shedule"),
            (Button)FindByName("Timer"),
            (Button)FindByName("Purposes")
            };

            _menuButtonsImages = new[]
            {
            (Image)FindByName("SheduleIcon"),
            (Image)FindByName("TimerIcon"),
            (Image)FindByName("PurposesIcon")
        };

            ShowPage(1); // Показать первую страницу по умолчанию
        }

        private void ShowPage(int index)
        {
            PageContent.Children.Clear();
            PageContent.Children.Add(_pages[index].Content);

            CurrentPage = _pages[index];

            // Обновить стиль кнопок
            foreach (var icon in _menuButtonsImages)
            {
                icon.Opacity = 0.4;
            }
            _menuButtonsImages[index].Opacity = 1;
        }

        private void OnTabButtonClicked(object sender, EventArgs e)
        {   
            
            var button = (Button)sender;
            var index = Array.IndexOf(_menuButtons, button);

            ShowPage(index);
        }
    }
}
