using PomodoroProject.Data.Models;
using Microsoft.Maui.Controls;
using PomodoroProject.ViewModels;
using Plugin.Maui.Audio;
using System.ComponentModel;
using PomodoroProject.Data;

namespace PomodoroProject.Views
{
    public partial class TimerPage : ContentPage
    {

        private readonly TimerViewModel _viewModel;

        public TimerPage(IAudioManager audio)
        {
            InitializeComponent();

            _viewModel = new TimerViewModel(ConfirmDeleteAsync, audio);
            BindingContext = _viewModel;

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TimerViewModel.KarmaValue) ||
                    e.PropertyName == nameof(TimerViewModel.IsWorkPhase) ||
                    e.PropertyName == nameof(TimerViewModel.IsTimerRunning))
                {
                    UpdateCursorPosition(_viewModel.KarmaValue);
                }
            };
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            Device.BeginInvokeOnMainThread(() => UpdateCursorPosition(_viewModel.KarmaValue));
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Обновляем позицию курсора, когда меняется карма или состояние работы/отдыха или таймера
            if (e.PropertyName == nameof(TimerViewModel.KarmaValue) ||
                e.PropertyName == nameof(TimerViewModel.IsWorkPhase) ||
                e.PropertyName == nameof(TimerViewModel.IsTimerRunning))
            {
                UpdateCursorPosition(_viewModel.KarmaValue);
            }
        }

        private async Task<bool> ConfirmDeleteAsync(PomodoroProject.Data.Models.PomodoroTask task)
        =>
            await DisplayAlert("Удаление", $"Удалить задачу \"{task.Name}\"?", "Да", "Нет");

        private void UpdateCursorPosition(double karma)
        {
            if (KarmaBar == null || KarmaCursor == null) return;
            if (KarmaBar.Width <= 0) return;

            double barWidth = KarmaBar.Width;
            double cursorWidth = KarmaCursor.Width;

            // Рассчитываем позицию курсора внутри полоски, чтобы он не вышел за края
            double x = (barWidth - cursorWidth) * karma;
            x = Math.Clamp(x, 0, barWidth - cursorWidth);

            // Смещаем курсор по горизонтали через Margin или TranslationX
            KarmaCursor.TranslationX = x;
        }


        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            UpdateCursorPosition(_viewModel.KarmaValue);
        }
    }
}
