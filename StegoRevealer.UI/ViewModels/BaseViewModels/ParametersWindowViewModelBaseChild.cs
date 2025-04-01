using StegoRevealer.UI.Tools.MvvmTools;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.BaseViewModels;

/// <summary>
/// Базовый класс для ViewModel конкретных представлений окна ParametersWindow
/// </summary>
public abstract class ParametersWindowViewModelBaseChild : ViewModelBase
{
    protected InstancesListAccessor _viewModels;  // Список ViewModel
    protected ParametersWindowViewModel _parametersWindowViewModel;  // Ссылка на корневую ViewModel

    protected ParametersWindowViewModelBaseChild(ParametersWindowViewModel rootViewModel, InstancesListAccessor viewModelsList)
    {
        _parametersWindowViewModel = rootViewModel;
        _viewModels = viewModelsList;
    }

    [Experimental]
    protected ParametersWindowViewModelBaseChild()
    {
        _parametersWindowViewModel = new();
        _viewModels = new(new InstancesList(), AccessMode.Get);
    }
}
