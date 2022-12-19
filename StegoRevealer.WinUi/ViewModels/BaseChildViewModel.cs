using StegoRevealer.WinUi.Lib;

namespace StegoRevealer.WinUi.ViewModels
{
    /// <summary>
    /// Базовый класс для ViewModel конкретных представлений
    /// </summary>
    public abstract class BaseChildViewModel : BaseViewModel
    {
        protected InstancesListAccessor _viewModels;  // Список ViewModel
        protected RootViewModel _rootViewModel;  // Ссылка на корневую ViewModel

        public BaseChildViewModel(RootViewModel rootViewModel, InstancesListAccessor viewModelsList)
        {
            _rootViewModel = rootViewModel;
            _viewModels = viewModelsList;
        }
    }
}
