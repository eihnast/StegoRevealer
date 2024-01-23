using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using ReactiveUI;
using SkiaSharp;
using StegoRevealer.StegoCore.AnalysisMethods;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.UI.Lib;
using StegoRevealer.UI.Lib.Entities;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class AnalyzerViewModel : MainWindowViewModelBaseChild
{
    // Параметры методов стегоанализа
    private ChiSquareParameters? _chiSquareParameters = null;
    private RsParameters? _rsParameters = null;
    private KzhaParameters? _kzhaParameters = null;


    private const string ImageNotSelectedText = "Изображение не выбрано";

    private string _imagePath = ImageNotSelectedText;
    public string ImagePath
    {
        get => _imagePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _imagePath, value);
            HasLoadedImage = !string.IsNullOrWhiteSpace(value) && !value.Equals(ImageNotSelectedText);
        }
    }

    private bool _hasLoadedImage = false;
    public bool HasLoadedImage
    {
        get => _hasLoadedImage;
        set => this.RaiseAndSetIfChanged(ref _hasLoadedImage, value);
    }

    private bool _methodChiSqrSelected = true;
    public bool MethodChiSqrSelected
    {
        get => _methodChiSqrSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _methodChiSqrSelected, value);
            ActiveMethods[AnalyzerMethod.ChiSquare] = value;
        }
    }
    private bool _methodRsSelected = true;
    public bool MethodRsSelected
    {
        get => _methodRsSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _methodRsSelected, value);
            ActiveMethods[AnalyzerMethod.RegularSingular] = value;
        }
    }
    private bool _methodKzhaSelected = true;
    public bool MethodKzhaSelected
    {
        get => _methodKzhaSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _methodKzhaSelected, value);
            ActiveMethods[AnalyzerMethod.KochZhaoAnalysis] = value;
        }
    }

    // Становится true, если есть результаты СА (фактически, после успешного вызова СА)
    private bool _hasResults = false;
    /// <summary>
    /// Существуют ли результаты проведённого стегоанализа
    /// </summary>
    public bool HasResults
    { 
        get => _hasResults;
        set => this.RaiseAndSetIfChanged(ref _hasResults, value); 
    }

    /// <summary>
    /// Текущее выбранное изображение
    /// </summary>
    public ImageHandler? CurrentImage { get; set; } = null;


    // Отображаемое изображение
    private Bitmap? _drawedImageSource;  // Источник для отображения
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
            else
                DrawedImageSource = null;
        }
    }

    /// <summary>
    /// Источник текущего отображаемого изображения
    /// </summary>
    public Bitmap? DrawedImageSource
    {
        get => _drawedImageSource;
        private set => this.RaiseAndSetIfChanged(ref _drawedImageSource, value);
    }


    /// <summary>
    /// Словарь активных методов (отмеченных к выполнению)
    /// </summary>
    public Dictionary<AnalyzerMethod, bool> ActiveMethods { get; private set; } = new();


    private SteganalysisResultsDto? _currentResults = null;


    public AnalyzerViewModel(MainWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList)
    {
        foreach (AnalyzerMethod method in Enum.GetValues(typeof(AnalyzerMethod)))
            ActiveMethods.Add(method, true);

        WindowResizeAction += SetImagePreviewSizes;
        if (_mainWindowViewModel.MainWindow is not null)
            _mainWindowViewModel.MainWindow.SizeChanged += (object? sender, SizeChangedEventArgs e) => WindowResizeAction();
    }

    [Experimental]
    public AnalyzerViewModel() : base() { }


    public void SetImagePreviewSizes()
    {
        var actualSize = GetWindowSize();
        ImagePreviewMaxHeight = Math.Max(0, actualSize.Height - 60 - 80 - 40 - 30);
        ImagePreviewMaxWidth = Math.Max(0, (actualSize.Width - 20 - 30) / 2);
    }

    public Action WindowResizeAction { get; set; } = null!;


    /// <summary>
    /// Вызывает диалог выбора изображения и возвращает путь к выбранному изображению
    /// </summary>
    private async Task<string> SelectNewImageFile()
    {
        var topLevel = TopLevel.GetTopLevel(_mainWindowViewModel.MainWindow);
        if (topLevel is null)
            return string.Empty;

        string path = string.Empty;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выбор файла изображения",
            AllowMultiple = false,
            FileTypeFilter = new FilePickerFileType[]
            {
                new("Image files") { Patterns = new[] { "*.png", "*.bmp" }, MimeTypes = new[] { "*/*" } }
            }
        });

        if (files is not null && files.Count > 0)
            path = files[0].Path.LocalPath;
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
            _chiSquareParameters = new ChiSquareParameters(CurrentImage) { Visualize = true };
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
    public async Task<bool> TryLoadImage()
    {
        // Выбор файла
        string path = await SelectNewImageFile();
        if (!string.IsNullOrEmpty(path))
            ImagePath = path;
        else
            ResetVmData();

        // Загрузка
        return CreateCurrentImageHandler(path);
    }

    private void ResetVmData()
    {
        ImagePath = ImageNotSelectedText;
        DrawedImage = null;
        HasResults = false;
    }

    /// <summary>
    /// Создаёт обработчик изображения
    /// </summary>
    private bool CreateCurrentImageHandler(string path)
    {
        try
        {
            CurrentImage?.CloseHandler();
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
//        if (CurrentImage is null)
//            return;

//        object? parameters = analyzerMethod switch
//        {
//            AnalyzerMethod.ChiSquare => _chiSquareParameters,
//            AnalyzerMethod.RegularSingular => _rsParameters,
//            AnalyzerMethod.KochZhaoAnalysis => _kzhaParameters,
//            _ => throw new System.NotImplementedException()
//        };

//        if (parameters is null)
//            return;

//        var paramsWindow = new ParametersWindow(analyzerMethod, parameters);
//        paramsWindow.Owner = _rootViewModel.MainWindow;
//        var paramsGetter = paramsWindow.ParamsReciever;
//        paramsWindow.ShowDialog();

//        var recievedParameters = paramsGetter();
//        if (recievedParameters is null)
//            return;

//#pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
//        switch (analyzerMethod)
//        {
//            case AnalyzerMethod.ChiSquare:
//                var chiParamsDto = recievedParameters as BaseParamsDto<ChiSquareParameters>;
//                chiParamsDto?.FillParameters(ref _chiSquareParameters);
//                break;
//            case AnalyzerMethod.RegularSingular:
//                var rsParamsDto = recievedParameters as BaseParamsDto<RsParameters>;
//                rsParamsDto?.FillParameters(ref _rsParameters);
//                break;
//            case AnalyzerMethod.KochZhaoAnalysis:
//                var kzhaParamsDto = recievedParameters as BaseParamsDto<KzhaParameters>;
//                kzhaParamsDto?.FillParameters(ref _kzhaParameters);
//                break;
//        }
//#pragma warning restore CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
    }

    /// <summary>
    /// Запуск процесса стегоанализа для указанных выбранных методов
    /// </summary>
    public void StartAnalysis()
    {
        if (ActiveMethods.Count(m => m.Value is true) == 0)
        {
            DrawCurrentImage();
            return;
        }

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

        timer.Stop();  // Остановка таймера

        // Возврат текущего изображения в превью, если визуализированное не вернулось из методов СА - пока что только Хи-квадрат
        var chiRes = results[AnalyzerMethod.ChiSquare] as ChiSquareResult;
        if (chiRes is not null)
            DrawCurrentImage();

        ProcessAnalysisResults(results, timer);
    }

    /// <summary>
    /// Обработка результатов стегоанализа
    /// </summary>
    private void ProcessAnalysisResults(Dictionary<AnalyzerMethod, ILoggedAnalysisResult?>? results, Stopwatch timer)
    {
        if (results is null)
            return;
        HasResults = true;

        // Приведение к известным типам результатов
        var chiRes = results[AnalyzerMethod.ChiSquare] as ChiSquareResult;
        var rsRes = results[AnalyzerMethod.RegularSingular] as RsResult;
        var kzhaRes = results[AnalyzerMethod.KochZhaoAnalysis] as KzhaResult;

        // Вывод визуализированного изображения
        if (chiRes is not null && (_chiSquareParameters?.Visualize ?? false))
            DrawedImage = chiRes?.Image;

        // Обновление текущих сохранённых результатов
        _currentResults = new SteganalysisResultsDto(chiRes, rsRes, kzhaRes, timer.ElapsedMilliseconds);
    }

    /// <summary>
    /// Создаёт словарь с null-(default-)значениями указанного типа по методам стегоанализа
    /// </summary>
    private static Dictionary<AnalyzerMethod, T?> CreateValuesByAnalyzerMethodDictionary<T>()
    {
        var dict = new Dictionary<AnalyzerMethod, T?>();
        foreach (AnalyzerMethod method in Enum.GetValues(typeof(AnalyzerMethod)))
            dict.Add(method, default);
        return dict;
    }

    /// <summary>
    /// Создаёт источник для отображения изображения
    /// </summary>
    public static Bitmap CreateImageSource(ImageHandler image) => CommonTools.GetAvaloniaBitmapFromImageHandler(image);

    /// <summary>
    /// Заново формирует отображаемое на View изображение из текущего сохранённого
    /// </summary>
    public void DrawCurrentImage()
    {
        if (CurrentImage is not null)
            DrawedImage = CurrentImage;
    }

    /// <summary>
    /// Возвращает текущие сохранённые результаты стегоанализа
    /// </summary>
    public SteganalysisResultsDto? GetCurrentResults() => _currentResults;

    private Avalonia.Size GetWindowSize() => _mainWindowViewModel.MainWindow?.ClientSize ?? new Avalonia.Size(0.0, 0.0);


    private double _imagePreviewMaxWidth;
    public double ImagePreviewMaxWidth
    {
        get => _imagePreviewMaxWidth;
        private set => this.RaiseAndSetIfChanged(ref _imagePreviewMaxWidth, value);
    }

    private double _imagePreviewMaxHeight;
    public double ImagePreviewMaxHeight
    {
        get => _imagePreviewMaxHeight;
        private set => this.RaiseAndSetIfChanged(ref _imagePreviewMaxHeight, value);
    }
}
