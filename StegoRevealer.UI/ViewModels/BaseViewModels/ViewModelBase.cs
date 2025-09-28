using ReactiveUI;
using StegoRevealer.UI.Tools;

namespace StegoRevealer.UI.ViewModels.BaseViewModels;

/// <summary>
/// Базовый класс ViewModel
/// </summary>
public class ViewModelBase : ReactiveObject
{
    public LocalizationService L { get; } = LocalizationService.Instance;
}
