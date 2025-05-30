using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.AdditionalInfoWindowViewModels;

public class JointDecisionInfoViewModel : AdditionalInfoWindowViewModelBaseChild
{
    public JointDecisionInfoViewModel(AdditionalInfoWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public JointDecisionInfoViewModel() : base() { }
}
