using System.Reactive;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class ApiLauncherViewModel : MainWindowViewModelBaseChild
{


    public ApiLauncherViewModel(MainWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public ApiLauncherViewModel() : base() { }
}
