using StegoRevealer.UI.Tools.MvvmTools;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.BaseViewModels;

/// <summary>
/// Базовый класс для ViewModel конкретных представлений окна AdditionalInfoWindow
/// </summary>
public abstract class AdditionalInfoWindowViewModelBaseChild : ViewModelBase
{
    protected InstancesListAccessor _viewModels;  // Список ViewModel
    protected AdditionalInfoWindowViewModel _additionalInfoWindowViewModel;  // Ссылка на корневую ViewModel

    protected AdditionalInfoWindowViewModelBaseChild(AdditionalInfoWindowViewModel rootViewModel, InstancesListAccessor viewModelsList)
    {
        _additionalInfoWindowViewModel = rootViewModel;
        _viewModels = viewModelsList;
    }

    [Experimental]
    protected AdditionalInfoWindowViewModelBaseChild()
    {
        _additionalInfoWindowViewModel = new();
        _viewModels = new(new InstancesList(), AccessMode.Get);
    }
}
