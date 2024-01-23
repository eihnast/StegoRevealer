using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

public class EmptyParametersViewModel : ParametersWindowViewModelBaseChild
{
    public EmptyParametersViewModel(ParametersWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public EmptyParametersViewModel() : base() { }
}
