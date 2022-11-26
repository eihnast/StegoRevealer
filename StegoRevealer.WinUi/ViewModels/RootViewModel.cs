using StegoRevealer.WinUi.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoRevealer.WinUi.ViewModels
{
    public class RootViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel 
        { 
            get => _currentViewModel; 
            set => SetField(ref _currentViewModel, value); 
        }


        private InstancesList _viewModelsInstances = new();


        public RootViewModel()
        {
            // Установка стандартного MainViewModel
            var newVm = GetNewViewModel(typeof(MainViewModel)) as MainViewModel;
            if (newVm is null)
                throw new Exception("Не удалось создать основное представление окна");
            CurrentViewModel = newVm;
        }

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
