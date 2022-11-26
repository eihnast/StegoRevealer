using StegoRevealer.WinUi.Lib;

namespace StegoRevealer.WinUi.ViewModels
{
    public class MainViewModel : BaseChildViewModel
    {
        public MainViewModel(RootViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

        public void SwitchToStegoAnalyzerView()
        {
            var saViewModel = _rootViewModel.GetOrCreateViewModel(typeof(StegoAnalyzerViewModel)) as StegoAnalyzerViewModel;
            if (saViewModel is not null)
                _rootViewModel.CurrentViewModel = saViewModel;
        }
    }
}
