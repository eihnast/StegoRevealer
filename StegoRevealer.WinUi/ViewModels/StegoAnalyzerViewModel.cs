using HandyControl.Controls;
using Microsoft.Win32;
using SkiaSharp;
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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StegoRevealer.WinUi.ViewModels
{
    // TODO: Реализовать форму или окно для нормального вывода результатов
    // TODO: Иногда баг при открытии окна опций - не нажимается. Отследить точный сценарий воспроизведения бага не удалось.

    /// <summary>
    /// ViewModel представления StegoAnalyzer - окно стегоанализатора
    /// </summary>
    public class StegoAnalyzerViewModel : BaseChildViewModel
    {
        // Параметры методов стегоанализа
        private ChiSquareParameters? _chiSquareParameters = null;
        private RsParameters? _rsParameters = null;
        private KzhaParameters? _kzhaParameters = null;


        /// <summary>
        /// Текущее выбранное изображение
        /// </summary>
        public ImageHandler? CurrentImage { get; set; } = null;


        // Отображаемое изображение
        private ImageSource? _drawedImageSource;  // Источник для отображения
        private ImageHandler? _drawedImage;  // Изображение

        /// <summary>
        /// Обработчик текущего отображаемого изображения
        /// </summary>
        public ImageHandler? DrawedImage
        {
            get => _drawedImage;
            set
            {
                _drawedImage = value;
                if (_drawedImage is not null)
                    DrawedImageSource = CreateImageSource(_drawedImage);
            }
        }

        /// <summary>
        /// Источник текущего отображаемого изображения
        /// </summary>
        public ImageSource? DrawedImageSource
        {
            get => _drawedImageSource;
            private set => SetField(ref _drawedImageSource, value);
        }


        /// <summary>
        /// Словарь активных методов (отмеченных к выполнению)
        /// </summary>
        public Dictionary<AnalyzerMethod, bool> ActiveMethods { get; private set; } = new();


        public StegoAnalyzerViewModel(RootViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList)
        {
            foreach (AnalyzerMethod method in Enum.GetValues(typeof(AnalyzerMethod)))
                ActiveMethods.Add(method, false);
        }


        /// <summary>
        /// Активировать метод стегоанализа
        /// </summary>
        public void TurnOnMethod(AnalyzerMethod method) => ActiveMethods[method] = true;

        /// <summary>
        /// Деактивировать метод стегоанализа
        /// </summary>
        public void TurnOffMethod(AnalyzerMethod method) => ActiveMethods[method] = false;


        /// <summary>
        /// Вызывает диалог выбора изображения и возвращает путь к выбранному изображению
        /// </summary>
        private string SelectNewImageFile()
        {
            string path = string.Empty;
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                path = openFileDialog.FileName;
            return path;
        }

        /// <summary>
        /// Создание объектов параметров
        /// </summary>
        private void ActualizeParameters()
        {
            if (CurrentImage is null)
                return;

            if (_chiSquareParameters is null)
                _chiSquareParameters = new ChiSquareParameters(CurrentImage);
            else
                _chiSquareParameters.Image = CurrentImage;

            if (_rsParameters is null)
                _rsParameters = new RsParameters(CurrentImage);
            else
                _rsParameters.Image = CurrentImage;

            if (_kzhaParameters is null)
                _kzhaParameters = new KzhaParameters(CurrentImage);
            else
                _kzhaParameters.Image = CurrentImage;
        }

        /// <summary>
        /// Осуществляет загрузку выбираемого пользователем изображения
        /// </summary>
        public bool TryLoadImage()
        {
            // Выбор файла
            string path = SelectNewImageFile();

            // Загрузка
            return CreateCurrentImageHandler(path);
        }

        /// <summary>
        /// Создаёт обработчик изображения
        /// </summary>
        private bool CreateCurrentImageHandler(string path)
        {
            try
            {
                CurrentImage = new ImageHandler(path);
                ActualizeParameters();  // Обновит ссылку на изображение в параметрах методов
                DrawCurrentImage();  // Обновит изображение, отображаемое на форме
                return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Открытие модального окна установки параметров метода стегоанализа
        /// </summary>
        /// <param name="analyzerMethod">Метод стегоанализа</param>
        public void OpenParametersWindow(AnalyzerMethod analyzerMethod)
        {
            if (CurrentImage is null)
                return;

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

        /// <summary>
        /// Запуск процесса стегоанализа для указанных выбранных методов
        /// </summary>
        public void StartAnalysis(Dictionary<AnalyzerMethod, bool> activeMethods)
        {
            ActiveMethods = activeMethods;

            var results = CreateValuesByAnalyzerMethodDictionary<ILoggedAnalysisResult>();  // Словарь результатов
            var methodTasks = CreateValuesByAnalyzerMethodDictionary<Task<ILoggedAnalysisResult>>();  // Словарь задач

            // Создание задач
            if (ActiveMethods[AnalyzerMethod.ChiSquare] && _chiSquareParameters is not null)  // Хи-квадрат
            {
                var chiSqrMethodAnalyzer = new ChiSquareAnalyser(_chiSquareParameters);
                methodTasks[AnalyzerMethod.ChiSquare] = new Task<ILoggedAnalysisResult>(() => chiSqrMethodAnalyzer.Analyse());
            }
            if (ActiveMethods[AnalyzerMethod.RegularSingular] && _rsParameters is not null)  // RS
            {
                var rsMethodAnalyzer = new RsAnalyser(_rsParameters);
                methodTasks[AnalyzerMethod.RegularSingular] = new Task<ILoggedAnalysisResult>(() => rsMethodAnalyzer.Analyse());
            }
            if (ActiveMethods[AnalyzerMethod.KochZhaoAnalysis] && _kzhaParameters is not null)  // KZHA
            {
                var kzhaMethodAnalyzer = new KzhaAnalyser(_kzhaParameters);
                methodTasks[AnalyzerMethod.KochZhaoAnalysis] = new Task<ILoggedAnalysisResult>(() => kzhaMethodAnalyzer.Analyse());
            }

            var timer = Stopwatch.StartNew();  // Запуск таймера - подсчёт времени работы непосредственно методов стегоанализа

            // Запуск
            foreach (var methodTask in methodTasks)
                methodTask.Value?.Start();

            // Ожидание
            foreach (var methodTask in methodTasks)
                methodTask.Value?.Wait();

            // Сбор результатов
            foreach (AnalyzerMethod method in Enum.GetValues(typeof(AnalyzerMethod)))
            {
                if (ActiveMethods[method])
                    results[method] = methodTasks[method]?.Result;
            }

            // Возврат текущего изображения в превью, если визуализированное не вернулось из методов СА - пока что только Хи-квадрат
            var chiRes = results[AnalyzerMethod.ChiSquare] as ChiSquareResult;
            if (chiRes is not null)
                DrawCurrentImage();
            
            timer.Stop();  // Остановка таймера
            ProcessAnalysisResults(results, timer);
        }

        /// <summary>
        /// Обработка и вывод результатов стегоанализа
        /// </summary>
        private void ProcessAnalysisResults(Dictionary<AnalyzerMethod, ILoggedAnalysisResult?>? results, Stopwatch timer)
        {
            if (results is null) 
                return;

            // Временный вывод результатов в Alert-окне
            var chiRes = results[AnalyzerMethod.ChiSquare] as ChiSquareResult;
            var rsRes = results[AnalyzerMethod.RegularSingular] as RsResult;
            var kzhaRes = results[AnalyzerMethod.KochZhaoAnalysis] as KzhaResult;

            // Вывод визуализированного изображения
            if (_chiSquareParameters?.Visualize ?? false)
                DrawedImage = chiRes?.Image;

            MessageBox.Show($"Results\n" +
                (chiRes is not null ? $"Chisqr: {chiRes?.MessageRelativeVolume}\n" : "ChiSqr not analyzed\n") +
                (rsRes is not null ? $"Rs: {rsRes?.MessageRelativeVolume}\n" : "Rs not analyzed\n") +
                (kzhaRes is not null ? $"Kzha: {kzhaRes?.Threshold}, {kzhaRes?.MessageBitsVolume}\n" : "Kzha is not analyzed\n") +
                $"Time (ms): {timer.ElapsedMilliseconds}");
        }

        /// <summary>
        /// Создаёт словарь с null-(default-)значениями указанного типа по методам стегоанализа
        /// </summary>
        private Dictionary<AnalyzerMethod, T?> CreateValuesByAnalyzerMethodDictionary<T>()
        {
            var dict = new Dictionary<AnalyzerMethod, T?>();
            foreach (AnalyzerMethod method in Enum.GetValues(typeof(AnalyzerMethod)))
                dict.Add(method, default(T));
            return dict;
        }

        /// <summary>
        /// Создаёт источник для отображения изображения
        /// </summary>
        public ImageSource CreateImageSource(ImageHandler image)
        {
            var imgInfo = new SKImageInfo(image.Width, image.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

            var writeableBitmap = new WriteableBitmap(imgInfo.Width, imgInfo.Height, 96.0, 96.0, PixelFormats.Pbgra32, null);
            writeableBitmap.Lock();

            var surface = SKSurface.Create(imgInfo, writeableBitmap.BackBuffer, writeableBitmap.BackBufferStride);
            surface.Canvas.DrawBitmap(image.GetScImage().GetBitmap(), default(SKPoint));

            writeableBitmap.Unlock();
            writeableBitmap.Freeze();

            return writeableBitmap;
        }

        /// <summary>
        /// Заново формирует отображаемое на View изображение из текущего сохранённого
        /// </summary>
        public void DrawCurrentImage()
        {
            if (CurrentImage is not null)
                DrawedImage = CurrentImage;
        }
    }
}