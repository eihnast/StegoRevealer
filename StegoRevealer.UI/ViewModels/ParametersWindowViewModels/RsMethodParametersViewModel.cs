using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

public class RsMethodParametersViewModel : ParametersWindowViewModelBaseChild
{
    public RsMethodParametersViewModel(ParametersWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public RsMethodParametersViewModel() : base() { }
}
