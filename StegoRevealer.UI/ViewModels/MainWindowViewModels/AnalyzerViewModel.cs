using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
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
using StegoRevealer.UI.Windows;
using Avalonia;
using StegoRevealer.UI.Lib.ParamsHelpers;
using StegoRevealer.UI.Lib.MethodsHelper;
using StegoRevealer.StegoCore.Logger;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.DecisionModule;
using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.Common;
using StegoRevealer.StegoCore.CommonLib.Entities;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;
using System.Text;
using Accord;
using Newtonsoft.Json;

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class AnalyzerViewModel : MainWindowViewModelBaseChild
{
    // Параметры методов стегоанализа
    private ChiSquareParameters? _chiSquareParameters = null;
    private RsParameters? _rsParameters = null;
    private SpaParameters? _spaParameters = null;
    private FanParameters? _fanParameters = null;
    private ZcaParameters? _zcaParameters = null;
    private KzhaParameters? _kzhaParameters = null;
    private ComplexSaMethodParameters? _complexSaParameters = null;

    private JointAnalysisResult? _currentJointAnalysisResult = null;


    /// <summary>
    /// Путь к файлу изображения
    /// </summary>
    public string ImagePath
    {
        get => _imagePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _imagePath, value);
            HasLoadedImage = !string.IsNullOrWhiteSpace(value) && !string.IsNullOrEmpty(value);
        }
    }
    private string _imagePath = string.Empty;

    /// <summary>
    /// Загружено ли изображение для анализа
    /// </summary>
    public bool HasLoadedImage
    {
        get => _hasLoadedImage;
        set => this.RaiseAndSetIfChanged(ref _hasLoadedImage, value);
    }
    private bool _hasLoadedImage = false;

    /// <summary>
    /// Выбран ли метод Хи-квадрат
    /// </summary>
    public bool MethodChiSqrSelected
    {
        get => _methodChiSqrSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _methodChiSqrSelected, value);
            ActiveMethods[AnalysisMethod.ChiSquare] = value;
        }
    }
    private bool _methodChiSqrSelected = true;

    /// <summary>
    /// Выбран ли метод RS
    /// </summary>
    public bool MethodRsSelected
    {
        get => _methodRsSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _methodRsSelected, value);
            ActiveMethods[AnalysisMethod.RegularSingular] = value;
        }
    }
    private bool _methodRsSelected = true;

    /// <summary>
    /// Выбран ли метод SPA
    /// </summary>
    public bool MethodSpaSelected
    {
        get => _methodSpaSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _methodSpaSelected, value);
            ActiveMethods[AnalysisMethod.Spa] = value;
        }
    }
    private bool _methodSpaSelected = true;

    /// <summary>
    /// Выбран ли метод FAN
    /// </summary>
    public bool MethodFanSelected
    {
        get => _methodFanSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _methodFanSelected, value);
            ActiveMethods[AnalysisMethod.Fan] = value;
        }
    }
    private bool _methodFanSelected = true;

    /// <summary>
    /// Выбран ли метод ZCA
    /// </summary>
    public bool MethodZcaSelected
    {
        get => _methodZcaSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _methodZcaSelected, value);
            ActiveMethods[AnalysisMethod.Zca] = value;
        }
    }
    private bool _methodZcaSelected = true;

    /// <summary>
    /// Выбран ли метод КЖА
    /// </summary>
    public bool MethodKzhaSelected
    {
        get => _methodKzhaSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _methodKzhaSelected, value);
            ActiveMethods[AnalysisMethod.KochZhaoAnalysis] = value;
        }
    }
    private bool _methodKzhaSelected = true;

    /// <summary>
    /// Максимальная ширина изображения на форме
    /// </summary>
    public double ImagePreviewMaxWidth
    {
        get => _imagePreviewMaxWidth;
        private set => this.RaiseAndSetIfChanged(ref _imagePreviewMaxWidth, value);
    }
    private double _imagePreviewMaxWidth;

    /// <summary>
    /// Максимальная высота изображения на форме
    /// </summary>
    public double ImagePreviewMaxHeight
    {
        get => _imagePreviewMaxHeight;
        private set => this.RaiseAndSetIfChanged(ref _imagePreviewMaxHeight, value);
    }
    private double _imagePreviewMaxHeight;

    /// <summary>
    /// Ширина текущего изображения
    /// </summary>
    public double CurrentImageWidth
    {
        get => _currentImageWidth;
        private set => this.RaiseAndSetIfChanged(ref _currentImageWidth, value);
    }
    private double _currentImageWidth = 0.0;

    /// <summary>
    /// Высота текущего изображения
    /// </summary>
    public double CurrentImageHeight
    {
        get => _currentImageHeight;
        private set => this.RaiseAndSetIfChanged(ref _currentImageHeight, value);
    }
    private double _currentImageHeight = 0.0;

    public string ComplexSaTipText
    {
        get => _complexSaTipText;
        private set => this.RaiseAndSetIfChanged(ref _complexSaTipText, value);
    }
    private string _complexSaTipText = "Оценка на основе обученного бинарного классификатора для набора признаков: " +
        "оценка CSA, RS, CKZhA, оценки качества, количество пикселей";

    /// <summary>
    /// Существуют ли результаты проведённого стегоанализа
    /// </summary>
    public bool HasResults
    { 
        get => _hasResults;
        set => this.RaiseAndSetIfChanged(ref _hasResults, value);
    }
    private bool _hasResults = false;

    /// <summary>
    /// 
    /// </summary>
    public bool IsMethodsOpened
    {
        get => _isMethodsOpened;
        set => this.RaiseAndSetIfChanged(ref _isMethodsOpened, value);
    }
    private bool _isMethodsOpened = true;

    /// <summary>
    /// Выбран ли комплексный стегоанализ
    /// </summary>
    public bool ComplexMethodSelected
    {
        get => _complexMethodSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _complexMethodSelected, value);
            ActiveMethods[AnalysisMethod.Complex] = value;
        }
    }
    private bool _complexMethodSelected = true;

    /// <summary>
    /// Словарь активных методов (отмеченных к выполнению)
    /// </summary>
    public Dictionary<AnalysisMethod, bool> ActiveMethods { get; private set; } = new();

    /// <summary>
    /// Актуальные результаты стегоанализа
    /// </summary>
    public SteganalysisResultsDto? CurrentResults
    {
        get => _currentResults;
        private set
        {
            _currentResults = value;
            HasResults = value is not null;
        }
    }
    private SteganalysisResultsDto? _currentResults = null;

    /// <summary>
    /// Действия, которые будут выполняться при изменении размеров окна
    /// </summary>
    public Action WindowResizeAction { get; set; } = null!;

    /// <summary>
    /// Текущее выбранное изображение
    /// </summary>
    public ImageHandler? CurrentImage 
    {
        get => _currentImage;
        set
        {
            _currentImage = value;
            if (_currentImage is not null)
            {
                CurrentImageWidth = _currentImage.Width;
                CurrentImageHeight = _currentImage.Height;
            }
        }
    }
    private ImageHandler? _currentImage = null;

    // Отображаемое на форме изображение

    /// <summary>
    /// Обработчик текущего отображаемого изображения (может не соответствовать изначально выбранному)
    /// </summary>
    public ImageHandler? DrawnImage
    {
        get => _drawnImage;
        set
        {
            _drawnImage = value;
            if (_drawnImage is not null)
                DrawnImageSource = CommonTools.GetAvaloniaBitmapFromImageHandler(_drawnImage);
            else
                DrawnImageSource = null;
        }
    }
    private ImageHandler? _drawnImage;

    /// <summary>
    /// Источник текущего отображаемого изображения
    /// </summary>
    public Bitmap? DrawnImageSource
    {
        get => _drawedImageSource;
        private set => this.RaiseAndSetIfChanged(ref _drawedImageSource, value);
    }
    private Bitmap? _drawedImageSource;  // Источник для отображения


    // Конструкторы и установка начальных значений

    // Установка стандартных значений
    private void CreateDefaults()
    {
        foreach (AnalysisMethod method in Enum.GetValues(typeof(AnalysisMethod)))
            ActiveMethods.Add(method, true);
        //ActiveMethods[AnalysisMethod.Zca] = false;

        WindowResizeAction += SetImagePreviewSizes;
        if (_mainWindowViewModel.MainWindow is not null)
            _mainWindowViewModel.MainWindow.SizeChanged += (object? sender, SizeChangedEventArgs e) => WindowResizeAction();
    }

    public AnalyzerViewModel(MainWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList)
    {
        CreateDefaults();
    }

    [Experimental]
    public AnalyzerViewModel() : base()
    {
        CreateDefaults();
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

        if (_spaParameters is null)
            _spaParameters = new SpaParameters(CurrentImage);
        else
            _spaParameters.Image = CurrentImage;

        if (_fanParameters is null)
            _fanParameters = new FanParameters(CurrentImage);
        else
            _fanParameters.Image = CurrentImage;

        if (_zcaParameters is null)
            _zcaParameters = new ZcaParameters(CurrentImage);
        else
            _zcaParameters.Image = CurrentImage;

        if (_kzhaParameters is null)
            _kzhaParameters = new KzhaParameters(CurrentImage);
        else
            _kzhaParameters.Image = CurrentImage;

        if (_complexSaParameters is null)
            _complexSaParameters = new ComplexSaMethodParameters(CurrentImage);
        else
            _complexSaParameters.Image = CurrentImage;
    }

    /// <summary>
    /// Открытие модального окна установки параметров метода стегоанализа
    /// </summary>
    /// <param name="AnalysisMethod">Метод стегоанализа</param>
    public async Task OpenParametersWindow(AnalysisMethod AnalysisMethod)
    {
        if (!HasLoadedImage)
            return;

        object? parameters = AnalysisMethod switch
        {
            AnalysisMethod.ChiSquare => _chiSquareParameters,
            AnalysisMethod.RegularSingular => _rsParameters,
            AnalysisMethod.Spa => _spaParameters,
            AnalysisMethod.Fan => _fanParameters,
            AnalysisMethod.Zca => _zcaParameters,
            AnalysisMethod.KochZhaoAnalysis => _kzhaParameters,
            _ => throw new System.NotImplementedException()
        };

        if (parameters is null)
            return;

        CommonLogger.LogInfo($"Opening parameters window for steganalysis method {AnalysisMethod}");

        var receivedParameters = new ParametersStorage();
        var parametersVm = new ParametersWindowViewModel(parameters, receivedParameters);
        var parametersWindow = new ParametersWindow() { DataContext = parametersVm };

        if (_mainWindowViewModel.MainWindow is not null)
            await parametersWindow.ShowDialog(_mainWindowViewModel.MainWindow);

        if (receivedParameters.Parameters is null)
            return;

        CommonLogger.LogInfo($"Received parameters for stegoanalysis method {AnalysisMethod}");

        switch (AnalysisMethod)
        {
            case AnalysisMethod.ChiSquare:
                if (_chiSquareParameters is not null)
                {
                    var chiParamsDto = receivedParameters.Parameters as IParamsDto<ChiSquareParameters>;
                    chiParamsDto?.FillParameters(ref _chiSquareParameters);
                    CommonLogger.LogInfo("Received ChiSquare method parameters are: \n" + Common.Tools.GetFormattedJson(receivedParameters.Parameters as ChiSqrParamsDto));
                }
                break;
            case AnalysisMethod.RegularSingular:
                if (_rsParameters is not null)
                {
                    var rsParamsDto = receivedParameters.Parameters as IParamsDto<RsParameters>;
                    rsParamsDto?.FillParameters(ref _rsParameters);
                    CommonLogger.LogInfo("Received Regular-Singular method parameters are: \n" + Common.Tools.GetFormattedJson(receivedParameters.Parameters as RsParamsDto));
                }
                break;
            case AnalysisMethod.Spa:
                if (_spaParameters is not null)
                {
                    var spaParamsDto = receivedParameters.Parameters as IParamsDto<SpaParameters>;
                    spaParamsDto?.FillParameters(ref _spaParameters);
                    CommonLogger.LogInfo("Received SPA method parameters are: \n" + Common.Tools.GetFormattedJson(receivedParameters.Parameters as SpaParamsDto));
                }
                break;
            case AnalysisMethod.Fan:
                if (_fanParameters is not null)
                {
                    var fanParamsDto = receivedParameters.Parameters as IParamsDto<FanParameters>;
                    fanParamsDto?.FillParameters(ref _fanParameters);
                    CommonLogger.LogInfo("Received FAN method parameters are: \n" + Common.Tools.GetFormattedJson(receivedParameters.Parameters as FanParamsDto));
                }
                break;
            case AnalysisMethod.Zca:
                if (_zcaParameters is not null)
                {
                    var zcaParamsDto = receivedParameters.Parameters as IParamsDto<ZcaParameters>;
                    zcaParamsDto?.FillParameters(ref _zcaParameters);
                    CommonLogger.LogInfo("Received ZCA method parameters are: \n" + Common.Tools.GetFormattedJson(receivedParameters.Parameters as ZcaParamsDto));
                }
                break;
            case AnalysisMethod.KochZhaoAnalysis:
                if (_kzhaParameters is not null)
                {
                    var kzhaParamsDto = receivedParameters.Parameters as IParamsDto<KzhaParameters>;
                    kzhaParamsDto?.FillParameters(ref _kzhaParameters);
                    CommonLogger.LogInfo("Received Koch-Zhao Analysis method parameters are: \n" + Common.Tools.GetFormattedJson(receivedParameters.Parameters as KzhaParamsDto));
                }
                break;
        }
    }

    /// <summary>
    /// Открытие модального окна схемы совместного вывода
    /// </summary>
    public async Task OpenJointDecisionWindow()
    {
        var csaResult = _currentJointAnalysisResult?.ChiSquareResult;
        var rsResult = _currentJointAnalysisResult?.RsResult;

        if (csaResult is null || rsResult is null)
        {
            CommonLogger.LogWarning("No results for joint decision window, operation canceled");
            return;
        }

        var additionalInfoVm = new AdditionalInfoWindowViewModel();
        var additionalInfoWindow = new AdditionalInfoWindow() { DataContext = additionalInfoVm };
        additionalInfoVm.OpenJointDecisionInfo(csaResult, rsResult);

        if (_mainWindowViewModel.MainWindow is not null)
            await additionalInfoWindow.ShowDialog(_mainWindowViewModel.MainWindow);
    }

    /// <summary>
    /// Открытие модального окна гистограммы модулей разниц коэффициентов матриц ДКП
    /// </summary>
    public async Task OpenKzhaHistoWindow()
    {
        if (CurrentImage is null)
        {
            CommonLogger.LogWarning("No image for KZhA Histogram window, operation canceled");
            return;
        }

        var additionalInfoVm = new AdditionalInfoWindowViewModel();
        var additionalInfoWindow = new AdditionalInfoWindow() { DataContext = additionalInfoVm };
        additionalInfoVm.OpenKzhaHistoInfo(CurrentImage);

        if (_mainWindowViewModel.MainWindow is not null)
            await additionalInfoWindow.ShowDialog(_mainWindowViewModel.MainWindow);
    }


    /// <summary>
    /// Создаёт обработчик изображения
    /// </summary>
    private bool CreateCurrentImageHandler(string path)
    {
        try
        {
            CurrentImage = new ImageHandler(path);
            TempManager.Instance.RememberHandler(CurrentImage);
            ActualizeParameters();  // Обновит ссылку на изображение в параметрах методов или создат объекты параметров, если их нет
            CommonLogger.LogInfo($"Loaded new image for steganalysis: {CurrentImage.ImgPath}");

            DrawCurrentImage();  // Обновит изображение, отображаемое на форме
            return true;
        }
        catch (Exception ex)
        {
            CommonLogger.LogError($"Error while creating image handler for '{path}': {ex.Message}");
            ImagePath = string.Empty;
        }

        return false;
    }

    /// <summary>
    /// Осуществляет загрузку выбираемого пользователем изображения
    /// </summary>
    public async Task<bool> TryLoadImage()
    {
        // Выбор файла
        string path = await SelectNewImageFile();
        ResetImageAndResults();
        if (!string.IsNullOrEmpty(path))
        {
            ImagePath = path;
            CommonLogger.LogInfo($"Loading new image for steganalysis: '{path}' copying to Temp");

            // Загрузка
            var tempPath = Common.Tools.CopyFileToTemp(path);

            if (!string.IsNullOrEmpty(tempPath))
            {
                TempManager.Instance.RememberTempImage(path, tempPath);
                return CreateCurrentImageHandler(tempPath);
            }
            else
            {
                ImagePath = string.Empty;
            }
        }

        return false;
    }

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
    /// Запуск процесса стегоанализа для указанных выбранных методов
    /// </summary>
    public async Task StartAnalysis()
    {
        var image = TempManager.Instance.GetOriginalImageByTemp(CurrentImage?.ImgName ?? string.Empty);
        CommonLogger.LogInfo($"Started new steganalysis operation from UI for image '{image}'");
        if (!ActiveMethods.Any(m => m.Value))
        {
            DrawCurrentImage();
            CommonLogger.LogWarning("No active steganalysis methods, operation canceled");
            return;
        }

        JointAnalysisResult? result = null;
        try
        {
            result = await AnalysisOperationExecute();
        }
        catch (Exception ex)
        {
            CommonLogger.LogError($"Fatal error while executing analysis joint analysis operation: [{ex.GetType().Name}] {ex.Message}");
        }

        // Возврат текущего изображения в превью, если визуализированное не вернулось из методов СА - пока что только Хи-квадрат
        var chiRes = result?.ChiSquareResult;
        if (chiRes is not null)
            DrawCurrentImage();

        ProcessAnalysisResults(result);

        CommonLogger.LogInfo("Steganalysis operation completed");
    }

    private async Task<JointAnalysisResult> AnalysisOperationExecute()
    {
        var jointAnalysisParams = new JointAnalysisMethodsParameters();

        // Создание задач
        if (ActiveMethods[AnalysisMethod.ChiSquare] && _chiSquareParameters is not null)  // Хи-квадрат
            jointAnalysisParams.ChiSquareParameters = _chiSquareParameters;
        if (ActiveMethods[AnalysisMethod.RegularSingular] && _rsParameters is not null)  // RS
            jointAnalysisParams.RsParameters = _rsParameters;
        if (ActiveMethods[AnalysisMethod.Spa] && _spaParameters is not null)  // SPA
            jointAnalysisParams.SpaParameters = _spaParameters;
        if (ActiveMethods[AnalysisMethod.Fan] && _fanParameters is not null)  // FAN
            jointAnalysisParams.FanParameters = _fanParameters;
        if (ActiveMethods[AnalysisMethod.Zca] && _zcaParameters is not null)  // ZCA
            jointAnalysisParams.ZcaParameters = _zcaParameters;
        if (ActiveMethods[AnalysisMethod.KochZhaoAnalysis] && _kzhaParameters is not null)  // KZHA
            jointAnalysisParams.KzhaParameters = _kzhaParameters;
        if (CurrentImage is not null)
            jointAnalysisParams.StatmParameters = new StatmParameters(CurrentImage);
        if (ActiveMethods[AnalysisMethod.Complex] && _complexSaParameters is not null)
            jointAnalysisParams.ComplexSaMethodParameters = _complexSaParameters;

        CommonLogger.LogInfo("Starting steganalysis algorithms");

        var result = await JointAnalysisStarter.Start(jointAnalysisParams);
        return result;
    }

    /// <summary>
    /// Обработка результатов стегоанализа
    /// </summary>
    private void ProcessAnalysisResults(JointAnalysisResult? results)
    {
        if (results is null)
        {
            ResetResults();
            return;
        }

        _currentJointAnalysisResult = results;

        // Приведение к известным типам результатов
        var chiRes = results.ChiSquareResult;
        var rsRes = results.RsResult;
        var spaRes = results.SpaResult;
        var fanRes = results.FanResult;
        var zcaRes = results.ZcaResult;
        var kzhaRes = results.KzhaResult;
        var complexRes = results.ComplexSaMethodResults;
        var statmRes = results.StatmResult;

        // Вывод визуализированного изображения
        if (chiRes is not null && (_chiSquareParameters?.Visualize ?? false))
            DrawnImage = chiRes?.Image;

        // Обновление текущих сохранённых результатов
        CurrentResults = new SteganalysisResultsDto(chiRes, rsRes, spaRes, fanRes, kzhaRes, zcaRes, statmRes, complexRes, results.ElapsedTime);
        CommonLogger.LogInfo("Steganalysis operaions info:\n"
            + (chiRes is null ? string.Empty : "\tChi-Square Attack result = " + Common.Tools.GetFormattedJson(chiRes, true))
            + (rsRes is null ? string.Empty : "\n\tRegular-Singular result = " + Common.Tools.GetFormattedJson(rsRes, true))
            + (spaRes is null ? string.Empty : "\n\tSample Pair Analysis result = " + Common.Tools.GetFormattedJson(spaRes, true))
            + (fanRes is null ? string.Empty : "\n\tFast Additive Noise (HCF-COM) result = " + Common.Tools.GetFormattedJson(fanRes, true))
            + (zcaRes is null ? string.Empty : "\n\tZhilkin Compression Analysis result = " + Common.Tools.GetFormattedJson(zcaRes, true))
            + (kzhaRes is null ? string.Empty : "\n\tConsecutive Koch-Zhao Attack result = " + Common.Tools.GetFormattedJson(kzhaRes, true))
            + (statmRes is null ? string.Empty : "\n\tQuality characteristics calculation result = " + Common.Tools.GetFormattedJson(statmRes, true))
            + (complexRes is null ? string.Empty : "\n\tComplex Steganalysis Method result = " + Common.Tools.GetFormattedJson(complexRes, true))
            + $"\n\tElapsed time = {CurrentResults.ElapsedTime}"
            + (chiRes is null ? string.Empty : "\n\tLogs of Chi-Square Attack method:\n" + chiRes?.ToString(indent: 2))
            + (rsRes is null ? string.Empty : "\n\tLogs of Regular-Singular method:\n" + rsRes?.ToString(indent: 2))
            + (spaRes is null ? string.Empty : "\n\tLogs of Sample Pair Analysis method:\n" + spaRes?.ToString(indent: 2))
            + (fanRes is null ? string.Empty : "\n\tLogs of Fast Additive Noise (HCF-COM) method:\n" + fanRes?.ToString(indent: 2))
            + (zcaRes is null ? string.Empty : "\n\tLogs of Zhilkin Compression Analysis method:\n" + zcaRes?.ToString(indent: 2))
            + (kzhaRes is null ? string.Empty : "\n\tLogs of Consecutive Koch-Zhao Attack method:\n" + kzhaRes?.ToString(indent: 2))
            + (statmRes is null ? string.Empty : "\n\tLogs of Quality characteristics calculation:\n" + statmRes?.ToString(indent: 2))
            + (complexRes is null ? string.Empty : "\n\tLogs of Complex Steganalysis Method method:\n" + complexRes?.ToString(indent: 2)));
    }


    /// <summary>
    /// Заново формирует изображение для отображения из текущего сохранённого
    /// </summary>
    public void DrawCurrentImage()
    {
        if (CurrentImage is not null)
            DrawnImage = CurrentImage;
    }

    /// <summary>
    /// Сброс результатов стегоанализа
    /// </summary>
    public void ResetResults()
    {
        CurrentResults = null;
        _currentJointAnalysisResult = null;
    }


    // Сбрасывает данные об изображении и результатах
    private void ResetImageAndResults()
    {
        ImagePath = string.Empty;
        DrawnImage = null;
        ResetResults();

        if (CurrentImage is not null)
            TempManager.Instance.ForgetHandler(CurrentImage);
        CurrentImage?.CloseHandler();

        var pathToDelete = CurrentImage?.ImgPath;
        if (!string.IsNullOrEmpty(pathToDelete))
            Common.Tools.TryDeleteTempFile(pathToDelete);
    }

    // Возвращает актуальные размеры окна
    private Avalonia.Size GetWindowSize() => _mainWindowViewModel.MainWindow?.ClientSize ?? new Avalonia.Size(0.0, 0.0);

    // Метод определения максимальных размеров для картинки
    private void SetImagePreviewSizes()
    {
        var actualSize = GetWindowSize();
        ImagePreviewMaxHeight = Math.Max(0, actualSize.Height - 60 - 80 - 40 - 30);
        ImagePreviewMaxWidth = Math.Max(0, (actualSize.Width - 20 - 30) / 2);
    }

    public async Task CopyResultsTextToClipboard()
    {
        if (CurrentResults is null)
            return;

        var results = new StringBuilder();
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.HidingDecisionDetection) + GetResultStringByState(CurrentResults.ComplexMethodState, CurrentResults.IsHidingDetected));
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.ChiSqrValue) + GetResultStringByState(CurrentResults.MethodChiSqrState, CurrentResults.ChiSqrMessageRelativeVolume));
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.RsValue) + GetResultStringByState(CurrentResults.MethodRsState, CurrentResults.RsMessageRelativeVolume));
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.SpaValue) + GetResultStringByState(CurrentResults.MethodSpaState, CurrentResults.SpaMessageRelativeVolume));
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.FanValue) + GetResultStringByState(CurrentResults.MethodFanState, CurrentResults.IsFanHidingDetected) 
            + (CurrentResults.FanMahalanobisDistance is not null ? $"({Math.Round(CurrentResults.FanMahalanobisDistance.Value, 3)})" : ""));
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.ZcaValue) + GetResultStringByState(CurrentResults.MethodZcaState, CurrentResults.IsZcaHidingDetected));
        
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.KzhaDetection) + GetResultStringByState(CurrentResults.MethodKzhaState, CurrentResults.KzhaSuspiciousIntervalIsFound));
        if (CurrentResults.KzhaSuspiciousIntervalIsFound)
        {
            results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.KzhaBitsNum) + GetResultStringByState(CurrentResults.MethodKzhaState, CurrentResults.KzhaMessageBitsVolume));
            results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.KzhaIndexes) + (CurrentResults.KzhaSuspiciousInterval is null ? "" :
                $"[{GetResultStringByState(CurrentResults.MethodKzhaState, CurrentResults.KzhaSuspiciousInterval.Value.leftInd)}, " +
                $"{GetResultStringByState(CurrentResults.MethodKzhaState, CurrentResults.KzhaSuspiciousInterval.Value.rightInd)}]"));
            results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.KzhaThreshold) + GetResultStringByState(CurrentResults.MethodKzhaState, CurrentResults.KzhaThreshold));
            results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.KzhaCoeffs)
                + (CurrentResults.KzhaCoefficients is null ? "" : GetResultStringByState(CurrentResults.MethodKzhaState, CurrentResults.KzhaCoefficients.Value.ToString())));
            results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.KzhaExtractedInfo)
                + (CurrentResults.KzhaExtractedData is null ? "" : GetResultStringByState(CurrentResults.MethodKzhaState, CurrentResults.KzhaExtractedData)));
        }

        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.StatmNoise) + Common.Tools.GetLongFormattedDouble(CurrentResults.StatmNoiseValue));
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.StatmSharpness) + Common.Tools.GetLongFormattedDouble(CurrentResults.StatmSharpnessValue));
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.StatmBlur) + Common.Tools.GetLongFormattedDouble(CurrentResults.StatmBlurValue));
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.StatmContrast) + Common.Tools.GetLongFormattedDouble(CurrentResults.StatmContrastValue));
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.StatmShennon) + Common.Tools.GetLongFormattedDouble(CurrentResults.StatmEntropyShennonValue));
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.StatmRenyi) + Common.Tools.GetLongFormattedDouble(CurrentResults.StatmEntropyRenyiValue));
        results.AppendLine(Common.Tools.AddColon(Constants.ResultsNames.ElapsedTime) + CurrentResults.ElapsedTime);

        await _mainWindowViewModel.CopyToClipboard(results.ToString());
    }
    public async Task CopyResultsJsonToClipboard()
    {
        if (_currentJointAnalysisResult is null)
            return;

        var results = new
        {
            _currentJointAnalysisResult.ComplexSaMethodResults?.IsHidingDetected,
            SteganalysisResult = _currentJointAnalysisResult
        };

        await _mainWindowViewModel.CopyToClipboard(JsonConvert.SerializeObject(results, Formatting.Indented));
    }

    private string GetResultStringByState(SaMethodExecutionState state, double result) =>
        state is SaMethodExecutionState.FatalError ? Constants.ResultsDefaults.WasFatalError : Common.Tools.GetValueAsPercents(result);
    private string GetResultStringByState(SaMethodExecutionState state, int result) =>
        state is SaMethodExecutionState.FatalError ? Constants.ResultsDefaults.WasFatalError : result.ToString();
    private string GetResultStringByState(SaMethodExecutionState state, long result) =>
        state is SaMethodExecutionState.FatalError ? Constants.ResultsDefaults.WasFatalError : result.ToString();
    private string GetResultStringByState(SaMethodExecutionState state, string result) =>
        state is SaMethodExecutionState.FatalError ? Constants.ResultsDefaults.WasFatalError : result;
    private string GetResultStringByState(SaMethodExecutionState state, bool result) =>
        state is SaMethodExecutionState.FatalError ? Constants.ResultsDefaults.WasFatalError : (result ? Constants.ResultsDefaults.Detected : Constants.ResultsDefaults.NotDetected);
}
