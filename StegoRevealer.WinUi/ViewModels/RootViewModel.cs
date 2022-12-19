using System;
using System.Linq;
using StegoRevealer.WinUi.Lib;

namespace StegoRevealer.WinUi.ViewModels
{
    /// <summary>
    /// Корневая ViewModel для Main-окна
    /// </summary>
    public class RootViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;

        /// <summary>
        /// Текущая ViewModel
        /// </summary>
        public BaseViewModel CurrentViewModel 
        { 
            get => _currentViewModel; 
            set => SetField(ref _currentViewModel, value); 
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


        #pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public RootViewModel()
        {
            // Установка стандартного MainViewModel
            var newVm = GetNewViewModel(typeof(MainViewModel)) as MainViewModel;
            if (newVm is null)
                throw new Exception("Не удалось создать основное представление окна");
            CurrentViewModel = newVm;
        }
        #pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.


        /// <summary>
        /// Созданое новой ViewModel указанного типа с добавлением в корневое хранилище
        /// </summary>
        public object? GetNewViewModel(Type viewModelType)
        {
            var modelViewsAccessor = new InstancesListAccessor(_viewModelsInstances, AccessMode.Get);
            if (viewModelType.IsSubclassOf(typeof(BaseChildViewModel)))
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
}
