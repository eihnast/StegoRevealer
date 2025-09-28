using System;
using ReactiveUI;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using StegoRevealer.UI.ViewModels.AdditionalInfoWindowViewModels;
using StegoRevealer.UI.Windows;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Threading.Tasks;

namespace StegoRevealer.UI.ViewModels;

public class AdditionalInfoWindowViewModel : ViewModelBase
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


    private AdditionalInfoWindow? _AdditionalInfoWindow = null;

    /// <summary>
    /// Ссылка на окно
    /// </summary>
    public AdditionalInfoWindow? AdditionalInfoWindow
    {
        get => _AdditionalInfoWindow;
        set => _AdditionalInfoWindow = _AdditionalInfoWindow is null ? value : _AdditionalInfoWindow;
    }


    private readonly InstancesList _viewModelsInstances = new();  // Список объектов ViewModel


    public AdditionalInfoWindowViewModel() { }

    public AdditionalInfoWindowViewModel(object? inputData = null) 
    {
        // SelectJointDecisionInfo();
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


    public void OpenJointDecisionInfo(ChiSquareResult csaRes, RsResult rsRes)
    {
        var jointDecisionVm = GetOrCreateViewModel(typeof(JointDecisionInfoViewModel)) as JointDecisionInfoViewModel;
        if (jointDecisionVm is not null)
        {
            CurrentViewModel = jointDecisionVm;
            jointDecisionVm.ProcessResults(csaRes, rsRes);
        }
    }

    public async Task OpenKzhaHistoInfo(ImageHandler img)
    {
        var kzhaHistoVm = GetOrCreateViewModel(typeof(KzhaHistoInfoViewModel)) as KzhaHistoInfoViewModel;
        if (kzhaHistoVm is not null)
        {
            CurrentViewModel = kzhaHistoVm;
            await kzhaHistoVm.CreateFrequencyView(img);
        }
    }
}
