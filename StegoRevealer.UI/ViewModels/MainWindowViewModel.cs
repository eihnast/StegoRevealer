using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using StegoRevealer.UI.Windows;
using ReactiveUI;
using System;
using System.Linq;
using Avalonia.Media;
using StegoRevealer.UI.Tools;

namespace StegoRevealer.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
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


    private MainWindow? _mainWindow = null;

    /// <summary>
    /// Ссылка на окно
    /// </summary>
    public MainWindow? MainWindow
    {
        get => _mainWindow;
        set => _mainWindow = _mainWindow is null ? value : _mainWindow;
    }


    private InstancesList _viewModelsInstances = new();  // Список объектов ViewModel

    public MainWindowViewModel() { }


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


    public void TurnToAnalyzer()
    {
        var vm = GetOrCreateViewModel(typeof(AnalyzerViewModel)) as AnalyzerViewModel;
        if (vm is not null)
            CurrentViewModel = vm;
    }
    public void TurnToHider()
    {
        var vm = GetOrCreateViewModel(typeof(HiderViewModel)) as HiderViewModel;
        if (vm is not null)
            CurrentViewModel = vm;
    }
    public void TurnToExtractor()
    {
        var vm = GetOrCreateViewModel(typeof(ExtractorViewModel)) as ExtractorViewModel;
        if (vm is not null)
            CurrentViewModel = vm;
    }
    public void TurnToInfoPage()
    {
        var vm = GetOrCreateViewModel(typeof(InfoPageViewModel)) as InfoPageViewModel;
        if (vm is not null)
            CurrentViewModel = vm;
    }
    public void TurnToSettingsPage()
    {
        var vm = GetOrCreateViewModel(typeof(SettingsPageViewModel)) as SettingsPageViewModel;
        if (vm is not null)
            CurrentViewModel = vm;
    }
}
