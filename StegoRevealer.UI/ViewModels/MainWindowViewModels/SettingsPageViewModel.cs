using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reflection;
using ReactiveUI;
using StegoRevealer.Common;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class SettingsPageViewModel : MainWindowViewModelBaseChild
{
    private bool _settingLoggingEnabled = Configurator.Settings.IsLoggingEnabled;
    public bool SettingLoggingEnabled
    {
        get => _settingLoggingEnabled;
        set
        {
            Configurator.Settings.IsLoggingEnabled = value;
            Configurator.SaveConfig();
            this.RaiseAndSetIfChanged(ref _settingLoggingEnabled, value);
        }
    }

    public List<string> Languages { get; } = Constants.Languages.Select(x => x.Value).ToList();


    public SettingsPageViewModel(MainWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public SettingsPageViewModel() : base() { }


    public void OpenLogsFolder()
    {
        CommonTools.OpenExplorerFolder(Common.Tools.GetOrCreateTempDirPath());
    }
}
