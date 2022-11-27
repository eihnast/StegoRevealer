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

        public void SwitchToHiderView()
        {
            var hiderViewModel = _rootViewModel.GetOrCreateViewModel(typeof(HiderViewModel)) as HiderViewModel;
            if (hiderViewModel is not null)
                _rootViewModel.CurrentViewModel = hiderViewModel;
        }

        public void SwitchToExtractorView()
        {
            var extractorViewModel = _rootViewModel.GetOrCreateViewModel(typeof(ExtractorViewModel)) as ExtractorViewModel;
            if (extractorViewModel is not null)
                _rootViewModel.CurrentViewModel = extractorViewModel;
        }
    }
}
