using StegoRevealer.UI.Tools.MvvmTools;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.BaseViewModels;

/// <summary>
/// Базовый класс для ViewModel конкретных представлений окна MainWindow
/// </summary>
public abstract class MainWindowViewModelBaseChild : ViewModelBase
{
    protected InstancesListAccessor _viewModels;  // Список ViewModel
    protected MainWindowViewModel _mainWindowViewModel;  // Ссылка на корневую ViewModel

    public MainWindowViewModelBaseChild(MainWindowViewModel rootViewModel, InstancesListAccessor viewModelsList)
    {
        _mainWindowViewModel = rootViewModel;
        _viewModels = viewModelsList;
    }

    [Experimental]
    public MainWindowViewModelBaseChild()
    {
        _mainWindowViewModel = new();
        _viewModels = new(new InstancesList(), AccessMode.Get);
    }
}
