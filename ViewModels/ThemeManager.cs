using Microsoft.Maui.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PomodoroProject.ViewModels;

// ThemeManager.cs
public class ThemeManager : INotifyPropertyChanged
{
    private static ThemeManager _instance;
    public static ThemeManager Instance => _instance ??= new ThemeManager();

    private Color _globalColor = Color.FromArgb("#FF7E7E"); // Начальный цвет
    public Color GlobalColor
    {
        get => _globalColor;
        set
        {
            if (_globalColor != value)
            {
                _globalColor = value;
                OnPropertyChanged();
                UpdateApplicationResources();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateApplicationResources()
    {
        if (Application.Current != null && Application.Current.Resources.TryGetValue("GlobalColor", out object _))
        {
            Application.Current.Resources["GlobalColor"] = GlobalColor;
        }
        if (Application.Current != null && Application.Current.Resources.TryGetValue("GlobalLightColor", out object _))
        {

            Application.Current.Resources["GlobalLightColor"] 
                = GlobalColor.Lerp(Colors.White, 0.6);
        }
    }
}