using Microsoft.Win32;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.WinUi.Lib;
using StegoRevealer.WinUi.Views.ParametersViews;

namespace StegoRevealer.WinUi.ViewModels
{
    public class StegoAnalyzerViewModel : BaseChildViewModel
    {
        private ChiSquareParameters? _chiSquareParameters = null;
        private RsParameters? _rsParameters = null;
        private KzhaParameters? _kzhaParameters = null;

        public ImageHandler? CurrentImage { get; set; } = null;

        public StegoAnalyzerViewModel(RootViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

        private string SelectNewImageFile()
        {
            string path = string.Empty;
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                path = openFileDialog.FileName;
            return path;
        }

        private void CreateParameters()
        {
            if (CurrentImage is null)
                return;

            if (_chiSquareParameters is null)
                _chiSquareParameters = new ChiSquareParameters(CurrentImage);
            if (_rsParameters is null)
                _rsParameters = new RsParameters(CurrentImage);
            if (_kzhaParameters is null)
                _kzhaParameters = new KzhaParameters(CurrentImage);
        }

        public bool TryLoadImage()
        {
            // Выбор файла
            string path = SelectNewImageFile();

            // Загрузка
            try
            {
                CurrentImage = new ImageHandler(path);
                CreateParameters();
                return true;
            }
            catch { }

            return false;
        }

        public void OpenParametersWindow(AnalyzerMethod analyzerMethod)
        {
            if (CurrentImage is not null)
            {
                object? parameters = analyzerMethod switch
                {
                    AnalyzerMethod.ChiSquare => _chiSquareParameters,
                    AnalyzerMethod.RegularSingular => _rsParameters,
                    AnalyzerMethod.KochZhaoAnalysis => _kzhaParameters,
                    _ => throw new System.NotImplementedException()
                };

                if (parameters is null)
                    return;

                var paramsWindow = new ParametersWindow(analyzerMethod, parameters);
                paramsWindow.Owner = _rootViewModel.MainWindow;
                paramsWindow.ShowDialog();
            }
        }
    }
}
