using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

public class KzhaMethodParametersViewModel : ParametersWindowViewModelBaseChild
{
    public KzhaMethodParametersViewModel(ParametersWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public KzhaMethodParametersViewModel() : base() { }
}
