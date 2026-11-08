using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Beadnado_Oroszi_Sandor_Y3ZAKR.ViewModels;

/// <summary>
/// Minden ViewModel őse. Az INotifyPropertyChanged interfész teszi lehetővé,
/// hogy a XAML-ben lévő adatkötések (Binding) értesüljenek a tulajdonságok változásáról.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Beállítja a mező értékét, és ha az ténylegesen változott, értesíti a nézetet.
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
