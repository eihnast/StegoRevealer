using StegoRevealer.WinUi.Lib;

namespace StegoRevealer.WinUi.ViewModels
{
    public abstract class BaseChildViewModel : BaseViewModel
    {
        protected InstancesListAccessor _viewModels;
        protected RootViewModel _rootViewModel;

        public BaseChildViewModel(RootViewModel rootViewModel, InstancesListAccessor viewModelsList)
        {
            _rootViewModel = rootViewModel;
            _viewModels = viewModelsList;
        }
    }
}
