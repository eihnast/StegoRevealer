using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

public class ChiSqrMethodParametersViewModel : ParametersWindowViewModelBaseChild
{
    public ChiSqrMethodParametersViewModel(ParametersWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public ChiSqrMethodParametersViewModel() : base() { }
}
