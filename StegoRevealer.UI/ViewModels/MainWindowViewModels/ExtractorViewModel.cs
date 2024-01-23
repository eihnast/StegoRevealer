using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class ExtractorViewModel : MainWindowViewModelBaseChild
{
    public ExtractorViewModel(MainWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public ExtractorViewModel() : base() { }
}
