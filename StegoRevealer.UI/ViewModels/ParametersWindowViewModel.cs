using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using StegoRevealer.UI.ViewModels.ParametersWindowViewModels;
using StegoRevealer.UI.Windows;
using ReactiveUI;
using System;
using System.Linq;

namespace StegoRevealer.UI.ViewModels;

public class ParametersWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel = null!;

    /// <summary>
    /// Текущая ViewModel
    /// </summary>
    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }


    private ParametersWindow? _parametersWindow = null;

    /// <summary>
    /// Ссылка на окно
    /// </summary>
    public ParametersWindow? ParametersWindow
    {
        get => _parametersWindow;
        set => _parametersWindow = _parametersWindow is null ? value : _parametersWindow;
    }


    private InstancesList _viewModelsInstances = new();  // Список объектов ViewModel


    public ParametersWindowViewModel()
    {
        // Установка стандартного ParametersViewModel
        var newVm = GetNewViewModel(typeof(EmptyParametersViewModel)) as EmptyParametersViewModel;
        if (newVm is null)
            throw new Exception("Не удалось создать стартовое представление окна");
        CurrentViewModel = newVm;
    }


    /// <summary>
    /// Созданое новой ViewModel указанного типа с добавлением в корневое хранилище
    /// </summary>
    public object? GetNewViewModel(Type viewModelType)
    {
        var modelViewsAccessor = new InstancesListAccessor(_viewModelsInstances, AccessMode.Get);
        if (viewModelType.IsSubclassOf(typeof(ViewModelBase)))
        {
            var newViewModel = Activator.CreateInstance(viewModelType, this, modelViewsAccessor);
            if (newViewModel is not null)
            {
                _viewModelsInstances.Add(newViewModel);
                return newViewModel;
            }
        }

        return null;
    }

    /// <summary>
    /// Возвращает объект ViewModel указанного типа из хранилища или создаёт и возвращает новый объект ViewModel, если его нет
    /// </summary>
    public object? GetOrCreateViewModel(Type viewModelType)
    {
        var viewModels = _viewModelsInstances.GetByType(viewModelType);
        if (viewModels.Count == 0)
            return GetNewViewModel(viewModelType);
        else
            return viewModels.First();
    }
}
