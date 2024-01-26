using System.Reactive;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class SettingsPageViewModel : MainWindowViewModelBaseChild
{
    public SettingsPageViewModel(MainWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public SettingsPageViewModel() : base() { }
}
