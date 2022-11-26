using StegoRevealer.WinUi.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StegoRevealer.WinUi.ViewModels
{
    public class MainViewModel : BaseChildViewModel
    {
        public MainViewModel(RootViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

        public void Btn_Click()
        {
            var saViewModel = _rootViewModel.GetOrCreateViewModel(typeof(StegoAnalyzerViewModel)) as StegoAnalyzerViewModel;
            if (saViewModel is not null)
                _rootViewModel.CurrentViewModel = saViewModel;
        }
    }
}
