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

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class AnalyzerViewModel : MainWindowViewModelBaseChild
{
    // Стандартные значения текстовых полей
    private const string ImageNotSelectedText = "Изображение не выбрано";

    // Параметры методов стегоанализа
    private ChiSquareParameters? _chiSquareParameters = null;
    private RsParameters? _rsParameters = null;
    private KzhaParameters? _kzhaParameters = null;
    private ComplexSaMethodParameters? _complexSaParameters = null;


    /// <summary>
    /// Путь к файлу изображения
    /// </summary>
    public string ImagePath
    {
        get => _imagePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _imagePath, value);
            HasLoadedImage = !string.IsNullOrWhiteSpace(value) && !value.Equals(ImageNotSelectedText);
        }
    }
    private string _imagePath = ImageNotSelectedText;

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
    public ImageHandler? CurrentImage { get; set; } = null;

    // Отображаемое на форме изображение

    /// <summary>
    /// Обработчик текущего отображаемого изображения (может не соответствовать изначально выбранному)
    /// </summary>
    public ImageHandler? DrawedImage
    {
        get => _drawedImage;
        set
        {
            _drawedImage = value;
            if (_drawedImage is not null)
                DrawedImageSource = CommonTools.GetAvaloniaBitmapFromImageHandler(_drawedImage);
            else
                DrawedImageSource = null;
        }
    }
    private ImageHandler? _drawedImage;

    /// <summary>
    /// Источник текущего отображаемого изображения
    /// </summary>
    public Bitmap? DrawedImageSource
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
            AnalysisMethod.KochZhaoAnalysis => _kzhaParameters,
            _ => throw new System.NotImplementedException()
        };

        if (parameters is null)
            return;

        Logger.LogInfo($"Opening parameters window for steganalysis method {AnalysisMethod}");

        var receivedParameters = new ParametersStorage();
        var parametersVm = new ParametersWindowViewModel(parameters, receivedParameters);
        var parametersWindow = new ParametersWindow() { DataContext = parametersVm };

        if (_mainWindowViewModel.MainWindow is not null)
            await parametersWindow.ShowDialog(_mainWindowViewModel.MainWindow);

        if (receivedParameters.Parameters is null)
            return;

        Logger.LogInfo($"Received parameters for stegoanalysis method {AnalysisMethod}");

        switch (AnalysisMethod)
        {
            case AnalysisMethod.ChiSquare:
                if (_chiSquareParameters is not null)
                {
                    var chiParamsDto = receivedParameters.Parameters as IParamsDto<ChiSquareParameters>;
                    chiParamsDto?.FillParameters(ref _chiSquareParameters);
                    Logger.LogInfo("Received ChiSquare method parameters are: \n" + Common.Tools.GetFormattedJson(receivedParameters.Parameters as ChiSqrParamsDto));
                }
                break;
            case AnalysisMethod.RegularSingular:
                if (_rsParameters is not null)
                {
                    var rsParamsDto = receivedParameters.Parameters as IParamsDto<RsParameters>;
                    rsParamsDto?.FillParameters(ref _rsParameters);
                    Logger.LogInfo("Received Regular-Singular method parameters are: \n" + Common.Tools.GetFormattedJson(receivedParameters.Parameters as RsParamsDto));
                }
                break;
            case AnalysisMethod.KochZhaoAnalysis:
                if (_kzhaParameters is not null)
                {
                    var kzhaParamsDto = receivedParameters.Parameters as IParamsDto<KzhaParameters>;
                    kzhaParamsDto?.FillParameters(ref _kzhaParameters);
                    Logger.LogInfo("Received Koch-Zhao Analysis method parameters are: \n" + Common.Tools.GetFormattedJson(receivedParameters.Parameters as KzhaParamsDto));
                }
                break;
        }
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
            Logger.LogInfo($"Loaded new image for steganalysis: {CurrentImage.ImgPath}");

            DrawCurrentImage();  // Обновит изображение, отображаемое на форме
            return true;
        }
        catch
        {
            Logger.LogError($"Не удалось создать обработчик изображния '{path}'");
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
            Logger.LogInfo($"Loading new image for steganalysis: '{path}' copying to Temp");

            // Загрузка
            var tempPath = Common.Tools.CopyFileToTemp(path);

            if (!string.IsNullOrEmpty(tempPath))
            {
                TempManager.Instance.RememberTempImage(path, tempPath);
                return CreateCurrentImageHandler(tempPath);
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
        Logger.LogInfo($"Started new steganalysis operation from UI for image '{image}'");
        if (!ActiveMethods.Any(m => m.Value))
        {
            DrawCurrentImage();
            Logger.LogWarning("No active steganalysis methods, operation canceled");
            return;
        }

        var jointAnalysisParams = new JointAnalysisMethodsParameters();

        // Создание задач
        if (ActiveMethods[AnalysisMethod.ChiSquare] && _chiSquareParameters is not null)  // Хи-квадрат
            jointAnalysisParams.ChiSquareParameters = _chiSquareParameters;
        if (ActiveMethods[AnalysisMethod.RegularSingular] && _rsParameters is not null)  // RS
            jointAnalysisParams.RsParameters = _rsParameters;
        if (ActiveMethods[AnalysisMethod.KochZhaoAnalysis] && _kzhaParameters is not null)  // KZHA
            jointAnalysisParams.KzhaParameters = _kzhaParameters;
        if (CurrentImage is not null)
            jointAnalysisParams.StatmParameters = new StatmParameters(CurrentImage);
        if (ActiveMethods[AnalysisMethod.Complex] && _complexSaParameters is not null)
            jointAnalysisParams.ComplexSaMethodParameters = _complexSaParameters;

        Logger.LogInfo("Starting steganalysis algorithms");

        var result = await JointAnalysisStarter.Start(jointAnalysisParams);

        // Возврат текущего изображения в превью, если визуализированное не вернулось из методов СА - пока что только Хи-квадрат
        var chiRes = result.ChiSquareResult;
        if (chiRes is not null)
            DrawCurrentImage();

        ProcessAnalysisResults(result);

        Logger.LogInfo("Steganalysis operation completed");
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

        // Приведение к известным типам результатов
        var chiRes = results.ChiSquareResult;
        var rsRes = results.RsResult;
        var spaRes = results.SpaResult;
        var kzhaRes = results.KzhaResult;
        var complexRes = results.ComplexSaMethodResults;
        var statmRes = results.StatmResult;

        // Вывод визуализированного изображения
        if (chiRes is not null && (_chiSquareParameters?.Visualize ?? false))
            DrawedImage = chiRes?.Image;

        // Обновление текущих сохранённых результатов
        CurrentResults = new SteganalysisResultsDto(chiRes, rsRes, kzhaRes, statmRes, complexRes, results.ElapsedTime);
        Logger.LogInfo("Steganalysis operaions info:\n"
            + (chiRes is null ? string.Empty : "\tChi-Square Attack result = " + Common.Tools.GetFormattedJson(chiRes, true))
            + (rsRes is null ? string.Empty : "\n\tRegular-Singular result = " + Common.Tools.GetFormattedJson(rsRes, true))
            + (spaRes is null ? string.Empty : "\n\tSample Pair Analysis result = " + Common.Tools.GetFormattedJson(spaRes, true))
            + (kzhaRes is null ? string.Empty : "\n\tConsecutive Koch-Zhao Attack result = " + Common.Tools.GetFormattedJson(kzhaRes, true))
            + (statmRes is null ? string.Empty : "\n\tQuality characteristics calculation result = " + Common.Tools.GetFormattedJson(statmRes, true))
            + (complexRes is null ? string.Empty : "\n\tComplex Steganalysis Method result = " + Common.Tools.GetFormattedJson(complexRes, true))
            + $"\n\tElapsed time = {CurrentResults.ElapsedTime}"
            + (chiRes is null ? string.Empty : "\n\tLogs of Chi-Square Attack method:\n" + chiRes?.ToString(indent: 2))
            + (rsRes is null ? string.Empty : "\n\tLogs of Regular-Singular method:\n" + rsRes?.ToString(indent: 2))
            + (spaRes is null ? string.Empty : "\n\tLogs of Sample Pair Analysis method:\n" + spaRes?.ToString(indent: 2))
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
            DrawedImage = CurrentImage;
    }

    /// <summary>
    /// Сброс результатов стегоанализа
    /// </summary>
    public void ResetResults() => CurrentResults = null;


    // Сбрасывает данные об изображении и результатах
    private void ResetImageAndResults()
    {
        ImagePath = ImageNotSelectedText;
        DrawedImage = null;
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
}
