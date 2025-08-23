using Avalonia.Threading;
using System.ComponentModel;
using System.Globalization;

namespace StegoRevealer.UI.Tools;

public class LocalizationService : INotifyPropertyChanged
{
    // Описание синглтона
    private static LocalizationService? _instance;
    private static readonly object _lock = new object();
    public static LocalizationService Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    if (_instance is null)
                        _instance = new LocalizationService();
                }
            }
            return _instance;
        }
    }


    public string this[string key] => Resources.Localization.LozalizationData.ResourceManager.GetString(key, Resources.Localization.LozalizationData.Culture) ?? key;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void ChangeCulture(string culture)
    {
        Resources.Localization.LozalizationData.Culture = new CultureInfo(culture);
        CultureInfo.CurrentUICulture = Resources.Localization.LozalizationData.Culture;
        CultureInfo.CurrentCulture = Resources.Localization.LozalizationData.Culture;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
    }
}
