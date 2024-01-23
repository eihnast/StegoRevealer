using ReactiveUI;

namespace StegoRevealer.UI.ViewModels.BaseViewModels;

/// <summary>
/// Базовый класс ViewModel
/// </summary>
public class ViewModelBase : ReactiveObject
{
    //private List<DataCardWindow> _dataCardWindows = SrUiCommonService.Instance.DataCardWindows;

    //// protected Action? UpdateDataAction = null;  // Вынесено в общий PlrUiCommonService

    //public DataCardWindow GetOrCreateDataCardWindow(Type entityType, int? entityId, DataCardObservableLists dataCardLists)
    //{
    //    var existedWindow = GetDataCardWindow(entityType, entityId);

    //    if (existedWindow is not null)
    //        return existedWindow;

    //    var vm = new DataCardWindowViewModel(entityType, entityId, dataCardLists);
    //    var newWindow = new DataCardWindow() { DataContext = vm };
    //    _dataCardWindows.Add(newWindow);
    //    newWindow.SetRemoveFromListMethod(() => _dataCardWindows.Remove(newWindow));

    //    if (entityType == typeof(Character))
    //    {
    //        newWindow.MinHeight = 900;
    //        newWindow.MinWidth = 1280;
    //    }

    //    return newWindow;
    //}

    //public DataCardWindow? GetDataCardWindow(Type entityType, int? entityId) =>
    //    _dataCardWindows.Where(w =>
    //    {
    //        var vm = w.DataContext as DataCardWindowViewModel;
    //        if (vm is not null && vm.CardType == entityType && vm.EntityId == entityId)
    //            return true;
    //        return false;
    //    }).FirstOrDefault();
}