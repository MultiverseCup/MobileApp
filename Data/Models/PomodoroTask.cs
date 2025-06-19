using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PomodoroProject.Data.Models;
public class PomodoroTask : INotifyPropertyChanged
{
    private long _totalWorkTime;

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public long WorkDuration { get; set; }
    public long RestDuration { get; set; }
    public long TimeRemaining { get; set; }

    public long TotalWorkTime { 
        get => _totalWorkTime; 
        set { _totalWorkTime = value; OnPropertyChanged(nameof(DisplayTotalTime));
        } }

    public string DisplayWorkDuration => TimeSpan.FromMilliseconds(WorkDuration).ToString(@"hh\:mm");
    public string DisplayRestDuration => TimeSpan.FromMilliseconds(RestDuration).ToString(@"hh\:mm");
    public string DisplayTotalTime => TimeSpan.FromMilliseconds(TotalWorkTime).ToString(@"hh\:mm\:ss");




    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}