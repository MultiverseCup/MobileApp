using SQLite;

namespace PomodoroProject.Data.Models;
public class PomodoroTask
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public long WorkDuration { get; set; }
    public long RestDuration { get; set; }
    public long TimeRemaining { get; set; }
    public long TotalWorkTime { get; set; }

    public string DisplayWorkDuration => TimeSpan.FromMilliseconds(WorkDuration).ToString(@"mm\:ss");
    public string DisplayRestDuration => TimeSpan.FromMilliseconds(RestDuration).ToString(@"mm\:ss");
    public string DisplayTotalTime => TimeSpan.FromMilliseconds(TotalWorkTime).ToString(@"hh\:mm\:ss");
}