using System.Reactive;
using ReactiveUI;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class InfoPageViewModel : MainWindowViewModelBaseChild
{
    private string _aboutMessage = """
        Stego Revealer - программный комплекс, предназначенный для осуществления стегоанализа и работы со стеганографическими методами скрытия и извлечения информации.
        
        Доступные методы стегоанализа:
            - метод оценки по критерию Хи-Квадрат;
            - метод Regular-Singular;
            - стегоанализ метода Коха-Жао.

        Доступные методы извлечения информации:
            - метод НЗБ;
            - метод Коха-Жао.

        Разработчик: Грачев Я.Л.
        Научный руководитель: Сидоренко В.Г.
        """;
    public string AboutMessage
    {
        get => _aboutMessage;
        set => this.RaiseAndSetIfChanged(ref _aboutMessage, value);
    }


    public InfoPageViewModel(MainWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public InfoPageViewModel() : base() { }
}
