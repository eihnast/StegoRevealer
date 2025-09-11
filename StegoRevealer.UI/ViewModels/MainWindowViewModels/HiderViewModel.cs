using Accord.Math;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ReactiveUI;
using SharpCompress.Common;
using StegoRevealer.Common;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.CommonLib.Entities;
using StegoRevealer.StegoCore.CommonLib.Exceptions;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using StegoRevealer.StegoCore.StegoMethods.Lsb;
using StegoRevealer.UI.Lib.Entities;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class HiderViewModel : MainWindowViewModelBaseChild
{
    // Параметры скрытия
    private LsbParameters? _lsbParameters = null;
    private KochZhaoParameters? _kzhParameters = null;


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
    /// Путь к файлу данных
    /// </summary>
    public string DataPath
    {
        get => _dataPath;
        set => this.RaiseAndSetIfChanged(ref _dataPath, value);
    }
    private string _dataPath = string.Empty;

    /// <summary>
    /// Данные для скрытия (из текстового поля)
    /// </summary>
    public string DataToHide
    {
        get => _dataToHide;
        set
        {
            this.RaiseAndSetIfChanged(ref _dataToHide, value);
            UpdateHasDataForHiding();
        }
    }
    private string _dataToHide = string.Empty;

    /// <summary>
    /// Данные для скрытия из файла
    /// </summary>
    public string LoadedDataToHide
    {
        get => _loadedDataToHide;
        set => this.RaiseAndSetIfChanged(ref _loadedDataToHide, value);
    }
    private string _loadedDataToHide = string.Empty;

    /// <summary>
    /// Загружено ли изображение для встраивания
    /// </summary>
    public bool HasLoadedImage
    {
        get => _hasLoadedImage;
        set
        {
            this.RaiseAndSetIfChanged(ref _hasLoadedImage, value);
            UpdateHasDataForHiding();
        }
    }
    private bool _hasLoadedImage = false;

    /// <summary>
    /// Загружены ли все необходимые данные для встраивания
    /// </summary>
    public bool HasDataForHiding
    {
        get => _hasDataForHiding;
        set => this.RaiseAndSetIfChanged(ref _hasDataForHiding, value);
    }
    private bool _hasDataForHiding = false;

    /// <summary>
    /// Загружен ли файл данных для скрытия
    /// </summary>
    public bool HasLoadedDataFile
    {
        get => _hasLoadedDataFile;
        set
        {
            this.RaiseAndSetIfChanged(ref _hasLoadedDataFile, value);
            UpdateHasDataForHiding();
        }
    }
    private bool _hasLoadedDataFile = false;

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

    /// <summary>
    /// Выбран ли метод НЗБ
    /// </summary>
    public bool MethodLsbSelected
    {
        get => _methodLsbSelected;
        set
        {
            if (!Common.Tools.IsActionWhileTabChanged())
                this.RaiseAndSetIfChanged(ref _methodLsbSelected, value);
        }
    }
    private bool _methodLsbSelected = true;

    /// <summary>
    /// Выбран ли метод Коха-Жао
    /// </summary>
    public bool MethodKzSelected
    {
        get => _methodKzSelected;
        set
        {
            if (!Common.Tools.IsActionWhileTabChanged())
                this.RaiseAndSetIfChanged(ref _methodKzSelected, value);
        }
    }
    private bool _methodKzSelected = false;

    /// <summary>
    /// Выбран ли последовательный способ
    /// </summary>
    public bool LinearModeSelected
    {
        get => _linearModeSelected;
        set
        {
            if (!Common.Tools.IsActionWhileTabChanged())
                this.RaiseAndSetIfChanged(ref _linearModeSelected, value);
        }
    }
    private bool _linearModeSelected = true;

    /// <summary>
    /// Выбран ли псевдослучайный способ
    /// </summary>
    public bool RandomModeSelected
    {
        get => _randomModeSelected;
        set
        {
            if (!Common.Tools.IsActionWhileTabChanged())
                this.RaiseAndSetIfChanged(ref _randomModeSelected, value);
        }
    }
    private bool _randomModeSelected = false;

    /// <summary>
    /// Значение ключа ГПСЧ для НЗБ
    /// </summary>
    public int LsbRandomSeedValue
    {
        get => _lsbRandomSeedValue;
        set => this.RaiseAndSetIfChanged(ref _lsbRandomSeedValue, value);
    }
    private int _lsbRandomSeedValue = 0;

    /// <summary>
    /// Выбран ли ключ ГПСЧ для НЗБ
    /// </summary>
    public bool LsbRandomSeedSelected
    {
        get => _lsbRandomSeedSelected;
        set => this.RaiseAndSetIfChanged(ref _lsbRandomSeedSelected, value);
    }
    private bool _lsbRandomSeedSelected = false;

    /// <summary>
    /// Стартовый индекс встраивания для НЗБ
    /// </summary>
    public int LsbStartIndexValue
    {
        get => _lsbStartIndexValue;
        set => this.RaiseAndSetIfChanged(ref _lsbStartIndexValue, value);
    }
    private int _lsbStartIndexValue = 0;

    /// <summary>
    /// Выбран ли стартовый индекс встраивания для НЗБ
    /// </summary>
    public bool LsbStartIndexSelected
    {
        get => _lsbStartIndexSelected;
        set => this.RaiseAndSetIfChanged(ref _lsbStartIndexSelected, value);
    }
    private bool _lsbStartIndexSelected = false;

    /// <summary>
    /// Значение ключа ГПСЧ для Коха-Жао
    /// </summary>
    public int KzRandomSeedValue
    {
        get => _kzRandomSeedValue;
        set => this.RaiseAndSetIfChanged(ref _kzRandomSeedValue, value);
    }
    private int _kzRandomSeedValue = 0;

    /// <summary>
    /// Выбран ли ключ ГПСЧ для Коха-Жао
    /// </summary>
    public bool KzRandomSeedSelected
    {
        get => _kzRandomSeedSelected;
        set => this.RaiseAndSetIfChanged(ref _kzRandomSeedSelected, value);
    }
    private bool _kzRandomSeedSelected = false;

    /// <summary>
    /// Порог для встраивания информации по Коха-Жао
    /// </summary>
    public double KzThresholdValue
    {
        get => _kzThresholdValue;
        set => this.RaiseAndSetIfChanged(ref _kzThresholdValue, value);
    }
    private double _kzThresholdValue = 120.0;

    /// <summary>
    /// Выбран ли порог для встраивания информации по Коха-Жао
    /// </summary>
    public bool KzThresholdSelected
    {
        get => _kzThresholdSelected;
        set => this.RaiseAndSetIfChanged(ref _kzThresholdSelected, value);
    }
    private bool _kzThresholdSelected = false;

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
    /// Существуют ли результаты проведённого встраивания
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
    public bool IsParamsOpened
    {
        get => _isParamsOpened;
        set => this.RaiseAndSetIfChanged(ref _isParamsOpened, value);
    }
    private bool _isParamsOpened = true;

    /// <summary>
    /// Актуальные результаты встраивания
    /// </summary>
    public HidingResultsDto? CurrentResults
    {
        get => _currentResults;
        private set
        {
            _currentResults = value;
            HasResults = value is not null;
        }
    }
    private HidingResultsDto? _currentResults = null;

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
        get => _drawedImage;
        set
        {
            _drawedImage = value;
            if (_drawedImage is not null)
                DrawnImageSource = CommonTools.GetAvaloniaBitmapFromImageHandler(_drawedImage);
            else
                DrawnImageSource = null;
        }
    }
    private ImageHandler? _drawedImage;

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
        WindowResizeAction += SetImagePreviewSizes;
        if (_mainWindowViewModel.MainWindow is not null)
            _mainWindowViewModel.MainWindow.SizeChanged += (object? sender, SizeChangedEventArgs e) => WindowResizeAction();
    }


    public HiderViewModel(MainWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList)
    {
        CreateDefaults();
    }

    [Experimental]
    public HiderViewModel() : base()
    {
        CreateDefaults();
        CurrentResults = new HidingResultsDto
        {
            ElapsedTime = 1234,
            NewFilePath = "C:\\Temp\\LongDirNameForTestingTextWrapping\\new_image.png"
        };
    }


    /// <summary>
    /// Создание объектов параметров
    /// </summary>
    private void ActualizeParameters()
    {
        if (CurrentImage is null)
            return;

        _lsbParameters = new LsbParameters(CurrentImage);
        _kzhParameters = new KochZhaoParameters(CurrentImage);

        UpdateParameters();
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
            ActualizeParameters();  // Обновит ссылку на изображение в параметрах или создат объекты параметров, если их нет
            Logger.LogInfo($"Loaded new image for extraction: {CurrentImage.ImgPath}");

            DrawCurrentImage();  // Обновит изображение, отображаемое на форме
            return true;
        }
        catch
        {
            Logger.LogError($"Не удалось создать обработчик изображния '{path}'");
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
            Logger.LogInfo($"Loading new image for extraction: '{path}' copying to Temp");

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
    /// Загружает данные файла для встраивания
    /// </summary>
    private async Task<bool> LoadHidingDataFromFile(string path)
    {
        try
        {
            string data = await File.ReadAllTextAsync(path);
            LoadedDataToHide = data;
            Logger.LogInfo($"Loaded new data for hiding: {path}");
            return true;
        }
        catch
        {
            Logger.LogError($"Не удалось загрузить данные из файла '{path}'");
            ResetDataFile();
        }

        return false;
    }

    /// <summary>
    /// Осуществляет загрузку выбираемого пользователем файла данных
    /// </summary>
    public async Task<bool> TryLoadData()
    {
        // Выбор файла
        string path = await SelectNewDataFile();
        ResetDataFile();
        if (!string.IsNullOrEmpty(path))
        {
            DataPath = path;
            Logger.LogInfo($"Loading new data file for hiding: '{path}' copying to Temp");

            // Загрузка
            var tempPath = Common.Tools.CopyFileToTemp(path);

            if (!string.IsNullOrEmpty(tempPath))
            {
                TempManager.Instance.RememberTempFile(path, tempPath);
                return await LoadHidingDataFromFile(tempPath);
            }
            else
            {
                ResetDataFile();
            }
        }

        return false;
    }

    /// <summary>
    /// Вызывает диалог выбора изображения и возвращает путь к выбранному изображению
    /// </summary>
    private async Task<string> SelectNewImageFile() =>
        await CommonTools.ChooseSingleFile(
            _mainWindowViewModel.MainWindow,
            new FilePickerFileType[]
            {
                new("Image files") { Patterns = new[] { "*.png", "*.bmp" }, MimeTypes = new[] { "*/*" } }
            },
            "Выбор файла изображения");

    /// <summary>
    /// Вызывает диалог выбора файла данных и возвращает путь к выбранному файлу
    /// </summary>
    private async Task<string> SelectNewDataFile() =>
        await CommonTools.ChooseSingleFile(
            _mainWindowViewModel.MainWindow,
            new FilePickerFileType[]
            {
                new("Text files") { Patterns = new[] { "*.txt" }, MimeTypes = new[] { "*/*" } },
                new("All files") { Patterns = new[] { "*.*" }, MimeTypes = new[] { "*/*" } }
            },
            "Выбор файла скрываемых данных");


    /// <summary>
    /// Запуск процесса встраивания выбранным методом
    /// </summary>
    public async Task StartHiding()
    {
        Logger.LogInfo("Starting hiding");
        UpdateParameters();

        var timer = Stopwatch.StartNew();  // Запуск таймера - подсчёт времени работы непосредственно встраивания
        Logger.LogInfo("Starting hiding operations");

        // Запуск
        if (_lsbParameters is null && _kzhParameters is null)
            throw new IncorrectValueException("No parameters");

        string dataToHide = GetDataToHide();
        string newPath = Common.Tools.CreateTempPath(Common.Tools.CreateTempFilenameWithoutExt() + Path.GetExtension(CurrentImage?.ImgPath));

        var results = new HidingResultsDto();
        if (MethodLsbSelected && _lsbParameters is not null)  // Встраивание по НЗБ
        {
            var hider = new LsbHider(_lsbParameters.Image);
            hider.Params.Seed = _lsbParameters.Seed;
            hider.Params.StartPixels = _lsbParameters.StartPixels;
            hider.Params.TraverseType = _lsbParameters.TraverseType;

            var hidingResult = (await Task.Run(() => hider.Hide(dataToHide, newPath))) as LsbHideResult;
            results.NewFilePath = hidingResult?.Path;
        }
        else if (MethodKzSelected && _kzhParameters is not null)  // Встраивание по Коха-Жао
        {
            var hider = new KochZhaoHider(_kzhParameters.Image);
            hider.Params.Seed = _kzhParameters.Seed;
            hider.Params.Threshold = _kzhParameters.Threshold;

            var hidingResult = (await Task.Run(() => hider.Hide(dataToHide, newPath))) as KochZhaoHideResult;
            results.NewFilePath = hidingResult?.Path;
        }

        Logger.LogInfo("Hiding operations completed");
        timer.Stop();  // Остановка таймера
        results.ElapsedTime = timer.ElapsedMilliseconds;

        ProcessAnalysisResults(results);

        Logger.LogInfo("Results of hiding:\n" + Logger.Separator
            + $"\nElapsed time = {CurrentResults?.ElapsedTime}\n" + Logger.Separator);
    }

    /// <summary>
    /// Обработка результатов встраивания
    /// </summary>
    private void ProcessAnalysisResults(HidingResultsDto? results)
    {
        if (results is null)
        {
            ResetResults();
            return;
        }

        CurrentResults = results;

        // Вывод нового изображения
        if (!string.IsNullOrEmpty(results.NewFilePath))
        {
            DrawnImage = new ImageHandler(results.NewFilePath);
        }
    }

    public void UpdateParameters()
    {
        if (MethodLsbSelected && _lsbParameters is not null)
        {
            _lsbParameters.Reset();

            if (RandomModeSelected)
                _lsbParameters.Seed = LsbRandomSeedValue;

            // Считаем, что встраивание только: чересканально, все 3 канала задействованы (порядок R,G,B), использован 1 НЗБ
            // Фактически, указывается индекс всего пикселя (одинаковых для всех трёх каналов) - т.к. методы обхода
            //    для кодера и декодера одинаково работают: чересканальность работает от красного к синему, беря StartIndexes,
            //    нет варианта сокрытия начиная с зелёного или синего канала, могут только отличаться индексы между собой по каналам.
            if (LsbStartIndexSelected)
            {
                int commonSkip = LsbStartIndexValue;  // Считаем, что указан индекс всего пикселя (убрал "/ 3")
                _lsbParameters.StartPixels = new StartValues(
                    (ImgChannel.Red, commonSkip), (ImgChannel.Green, commonSkip), (ImgChannel.Blue, commonSkip));
            }

            // LsbStartIndex не имеет значения, если задан LsbSeed!
        }

        if (MethodKzSelected && _kzhParameters is not null)
        {
            _kzhParameters.Reset();

            if (RandomModeSelected)
                _kzhParameters.Seed = KzRandomSeedValue;
            if (KzThresholdSelected)
                _kzhParameters.Threshold = KzThresholdValue;

            // KzIndexFirst и KzIndexSecond не имеют значения, если задан KzSeed!
        }
    }


    /// <summary>
    /// Осуществляет сохранение изображения со встраиванием
    /// </summary>
    public async Task TrySaveCoveredImage()
    {
        var topLevel = TopLevel.GetTopLevel(_mainWindowViewModel.MainWindow);
        if (topLevel is null || string.IsNullOrEmpty(CurrentResults?.NewFilePath))
            return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить изображение",
            FileTypeChoices = new FilePickerFileType[]
            {
                new("Image files") { Patterns = new[] { "*.png", "*.bmp" }, MimeTypes = new[] { "*/*" } }
            },
            DefaultExtension = Path.GetExtension(CurrentResults?.NewFilePath),
            SuggestedFileName = Path.GetFileNameWithoutExtension(ImagePath) + "Covered"
        });

        if (file is not null)
        {
            File.Copy(CurrentResults!.NewFilePath, file.Path.LocalPath, true);
            Logger.LogInfo($"Saved covered image file: '{file.Path.LocalPath}'");
        }
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
    /// Сброс результатов извлечения
    /// </summary>
    public void ResetResults() => CurrentResults = null;


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

    // Сбрасывает данные об файле данных
    public void ResetDataFile()
    {
        DataPath = string.Empty;
        LoadedDataToHide = string.Empty;
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


    public void SelectLsbMethod()
    {
        MethodLsbSelected = true;
        MethodKzSelected = false;
    }
    public void SelectKzMethod()
    {
        MethodLsbSelected = false;
        MethodKzSelected = true;
    }
    public void SelectLinearMode()
    {
        LinearModeSelected = true;
        RandomModeSelected = false;
    }
    public void SelectRandomMode()
    {
        LinearModeSelected = false;
        RandomModeSelected = true;
    }

    private string GetDataToHide()
    {
        if (string.IsNullOrEmpty(DataPath) || !File.Exists(DataPath))
            return DataToHide;

        if (string.IsNullOrEmpty(LoadedDataToHide))
            throw new IncorrectValueException("Data file path is set, but data is not loaded");

        return LoadedDataToHide;
    }

    private void UpdateHasDataForHiding() =>
        HasDataForHiding = HasLoadedImage && (HasLoadedDataFile || !string.IsNullOrEmpty(DataToHide));
}
