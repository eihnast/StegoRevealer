using StegoRevealer.WinUi.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoRevealer.WinUi.ViewModels
{
    public abstract class BaseChildViewModel : BaseViewModel
    {
        protected InstancesListAccessor _viewModels;
        protected RootViewModel _rootViewModel;

        public BaseChildViewModel(RootViewModel rootViewModel, InstancesListAccessor viewModelsList)
        {
            _rootViewModel = rootViewModel;
            _viewModels = viewModelsList;
        }
    }
}
