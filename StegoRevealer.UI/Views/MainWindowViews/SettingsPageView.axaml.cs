using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.Common;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using System.Linq;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class SettingsPageView : UserControl
{
    private SettingsPageViewModel _vm = null!;

    public SettingsPageView()
    {
        InitializeComponent();

        base.Loaded += SettingsPageView_Loaded;
    }

    private void SettingsPageView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<SettingsPageViewModel>(this.DataContext);

        Constants.Languages.TryGetValue(Configurator.Settings.Language, out string? lang);
        if (string.IsNullOrEmpty(lang))
            lang = "Русский";

        SettingsLanguageComboBox.SelectedItem = lang;
        VersionValue.Text = CommonTools.GetAppVersion();
    }

    private void OpenLogsFolderBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm.OpenLogsFolder();
    }

    private void SettingsLanguageComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        string? lang = (string?)SettingsLanguageComboBox.SelectedItem;
        if (string.IsNullOrEmpty(lang))
            return;

        var codes = Constants.Languages.Where(x => x.Value.Equals(lang, System.StringComparison.OrdinalIgnoreCase));
        string? code = codes.Any() ? codes.First().Key : null;
        if (string.IsNullOrEmpty(code))
            return;

        Configurator.Settings.Language = code;
        _vm.L.ChangeCulture(code);
    }
}
