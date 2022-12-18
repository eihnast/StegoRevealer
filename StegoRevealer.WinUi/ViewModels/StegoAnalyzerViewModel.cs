using HandyControl.Controls;
using Microsoft.Win32;
using StegoRevealer.StegoCore.AnalysisMethods;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.WinUi.Lib;
using StegoRevealer.WinUi.Lib.ParamsHelpers;
using StegoRevealer.WinUi.Views.ParametersViews;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StegoRevealer.WinUi.ViewModels
{
    public class StegoAnalyzerViewModel : BaseChildViewModel
    {
        private ChiSquareParameters? _chiSquareParameters = null;
        private RsParameters? _rsParameters = null;
        private KzhaParameters? _kzhaParameters = null;

        public ImageHandler? CurrentImage { get; set; } = null;

        public Dictionary<AnalyzerMethod, bool> ActiveMethods { get; private set; } = new();

        public StegoAnalyzerViewModel(RootViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList)
        {
            foreach (AnalyzerMethod method in Enum.GetValues(typeof(AnalyzerMethod)))
                ActiveMethods.Add(method, false);
        }

        public void TurnOnMethod(AnalyzerMethod method) => ActiveMethods[method] = true;
        public void TurnOffMethod(AnalyzerMethod method) => ActiveMethods[method] = false;

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
                var paramsGetter = paramsWindow.ParamsReciever;
                paramsWindow.ShowDialog();

                var recievedParameters = paramsGetter();
                if (recievedParameters is null)
                    return;

                #pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
                switch (analyzerMethod)
                {
                    case AnalyzerMethod.ChiSquare:
                        var chiParamsDto = recievedParameters as BaseParamsDto<ChiSquareParameters>;
                        chiParamsDto?.FillParameters(ref _chiSquareParameters);
                        break;
                    case AnalyzerMethod.RegularSingular:
                        var rsParamsDto = recievedParameters as BaseParamsDto<RsParameters>;
                        rsParamsDto?.FillParameters(ref _rsParameters);
                        break;
                    case AnalyzerMethod.KochZhaoAnalysis:
                        var kzhaParamsDto = recievedParameters as BaseParamsDto<KzhaParameters>;
                        kzhaParamsDto?.FillParameters(ref _kzhaParameters);
                        break;
                }
                #pragma warning restore CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
            }
        }

        public void StartAnalysis(Dictionary<AnalyzerMethod, bool> activeMethods)
        {
            ActiveMethods = activeMethods;

            var results = CreateValuesByAnalyzerMethodDictionary<IAnalysisResult>();  // Словарь результатов
            var methodTasks = CreateValuesByAnalyzerMethodDictionary<Task<IAnalysisResult>>();  // Словарь задач

            // Создание задач
            if (ActiveMethods[AnalyzerMethod.ChiSquare] && _chiSquareParameters is not null)  // Хи-квадрат
            {
                var chiSqrMethodAnalyzer = new ChiSquareAnalyser(_chiSquareParameters);
                methodTasks[AnalyzerMethod.ChiSquare] = new Task<IAnalysisResult>(() => chiSqrMethodAnalyzer.Analyse());
            }
            if (ActiveMethods[AnalyzerMethod.RegularSingular] && _rsParameters is not null)  // RS
            {
                var rsMethodAnalyzer = new RsAnalyser(_rsParameters);
                methodTasks[AnalyzerMethod.RegularSingular] = new Task<IAnalysisResult>(() => rsMethodAnalyzer.Analyse());
            }
            if (ActiveMethods[AnalyzerMethod.KochZhaoAnalysis] && _kzhaParameters is not null)  // KZHA
            {
                var kzhaMethodAnalyzer = new KzhaAnalyser(_kzhaParameters);
                methodTasks[AnalyzerMethod.KochZhaoAnalysis] = new Task<IAnalysisResult>(() => kzhaMethodAnalyzer.Analyse());
            }

            // Запуск задач
            var analysisTask = new Task(() =>
            {
                // Запуск
                foreach (var methodTask in methodTasks)
                    methodTask.Value?.Start();

                // Ожидание
                foreach (var methodTask in methodTasks)
                    methodTask.Value?.Wait();
            });
            analysisTask.Start();
            analysisTask.Wait();

            // Сбор результатов
            foreach (AnalyzerMethod method in Enum.GetValues(typeof(AnalyzerMethod)))
            {
                if (ActiveMethods[method])
                    results[method] = methodTasks[method]?.Result;
            }

            var chiRes = results[AnalyzerMethod.ChiSquare] as ChiSquareResult;
            var rsRes = results[AnalyzerMethod.RegularSingular] as RsResult;
            var kzhaRes = results[AnalyzerMethod.KochZhaoAnalysis] as KzhaResult;
            MessageBox.Show($"Results\n" +
                (chiRes is not null ? $"Chisqr: {chiRes?.MessageRelativeVolume}\n" : "ChiSqr not analyzed\n") +
                (rsRes is not null ? $"Rs: {rsRes?.MessageRelativeVolume}\n" : "Rs not analyzed\n") +
                (kzhaRes  is not null ? $"Kzha: {kzhaRes?.Threshold}, {kzhaRes?.MessageBitsVolume}\n" : "Kzha is not analyzed"));
        }

        private Dictionary<AnalyzerMethod, T?> CreateValuesByAnalyzerMethodDictionary<T>()
        {
            var dict = new Dictionary<AnalyzerMethod, T?>();
            foreach (AnalyzerMethod method in Enum.GetValues(typeof(AnalyzerMethod)))
                dict.Add(method, default(T));
            return dict;
        }
    }
}
