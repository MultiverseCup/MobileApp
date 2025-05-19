
using System.ComponentModel;

namespace PomodoroProject.ViewModels;

public partial class PurposesViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


    bool isModePickerOpen;
    public bool IsModePickerOpen
    {
        get => isModePickerOpen;
        set
        {
            isModePickerOpen = value;
            OnPropertyChanged(nameof(IsModePickerOpen));
        }
    }
    


}
