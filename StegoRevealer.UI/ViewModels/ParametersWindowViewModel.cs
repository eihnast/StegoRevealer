using System;
using System.Linq;
using ReactiveUI;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using StegoRevealer.UI.ViewModels.ParametersWindowViewModels;
using StegoRevealer.UI.Windows;

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


    private readonly InstancesList _viewModelsInstances = new();  // Список объектов ViewModel


    public object? ParametersDto { get; set; } = null;
    public Action FillParametersDtoAction { get; set; } = delegate { };


    public ParametersWindowViewModel() { }

    public ParametersWindowViewModel(object? currentParameters, ParametersStorage parametersToReceive) 
    {
        if (currentParameters is not null)
        {
            if (currentParameters is ChiSquareParameters)
                SelectChiSqrParameters(currentParameters);
            else if (currentParameters is RsParameters)
                SelectRsParameters(currentParameters);
            else if (currentParameters is SpaParameters)
                SelectSpaParameters(currentParameters);
            else if (currentParameters is KzhaParameters)
                SelectKzhaParameters(currentParameters);
        }

        FillParametersDtoAction += () => parametersToReceive.Parameters = ParametersDto;
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
            return viewModels[0];
    }


    private void SelectChiSqrParameters(object parameters)
    {
        var paramsVm = GetOrCreateViewModel(typeof(ChiSqrMethodParametersViewModel)) as ChiSqrMethodParametersViewModel;
        if (paramsVm is not null)
        {
            CurrentViewModel = paramsVm;
            paramsVm.SetParameters(parameters);
            FillParametersDtoAction += () => ParametersDto = paramsVm.CollectParameters();
        }
    }
    private void SelectRsParameters(object parameters)
    {
        var paramsVm = GetOrCreateViewModel(typeof(RsMethodParametersViewModel)) as RsMethodParametersViewModel;
        if (paramsVm is not null)
        {
            CurrentViewModel = paramsVm;
            paramsVm.SetParameters(parameters);
            FillParametersDtoAction += () => ParametersDto = paramsVm.CollectParameters();
        }
    }
    private void SelectSpaParameters(object parameters)
    {
        var paramsVm = GetOrCreateViewModel(typeof(SpaMethodParametersViewModel)) as SpaMethodParametersViewModel;
        if (paramsVm is not null)
        {
            CurrentViewModel = paramsVm;
            paramsVm.SetParameters(parameters);
            FillParametersDtoAction += () => ParametersDto = paramsVm.CollectParameters();
        }
    }
    private void SelectKzhaParameters(object parameters)
    {
        var paramsVm = GetOrCreateViewModel(typeof(KzhaMethodParametersViewModel)) as KzhaMethodParametersViewModel;
        if (paramsVm is not null)
        {
            CurrentViewModel = paramsVm;
            paramsVm.SetParameters(parameters);
            FillParametersDtoAction += () => ParametersDto = paramsVm.CollectParameters();
        }
    }
}
