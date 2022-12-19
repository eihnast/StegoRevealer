using StegoRevealer.WinUi.Lib;

namespace StegoRevealer.WinUi.ViewModels
{
    /// <summary>
    /// ViewModel представления Main - главное окно программы
    /// </summary>
    public class MainViewModel : BaseChildViewModel
    {
        public MainViewModel(RootViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

        /// <summary>
        /// Перейти к окну стегоанализатора
        /// </summary>
        public void SwitchToStegoAnalyzerView()
        {
            var saViewModel = _rootViewModel.GetOrCreateViewModel(typeof(StegoAnalyzerViewModel)) as StegoAnalyzerViewModel;
            if (saViewModel is not null)
                _rootViewModel.CurrentViewModel = saViewModel;
        }

        /// <summary>
        /// Перейти к окну хайдера
        /// </summary>
        public void SwitchToHiderView()
        {
            var hiderViewModel = _rootViewModel.GetOrCreateViewModel(typeof(HiderViewModel)) as HiderViewModel;
            if (hiderViewModel is not null)
                _rootViewModel.CurrentViewModel = hiderViewModel;
        }

        /// <summary>
        /// Перейти к окну экстрактора
        /// </summary>
        public void SwitchToExtractorView()
        {
            var extractorViewModel = _rootViewModel.GetOrCreateViewModel(typeof(ExtractorViewModel)) as ExtractorViewModel;
            if (extractorViewModel is not null)
                _rootViewModel.CurrentViewModel = extractorViewModel;
        }
    }
}
