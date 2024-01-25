using System;
using System.Reactive;
using ReactiveUI;
using Avalonia.Media.Imaging;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.UI.Lib.Entities;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using StegoRevealer.StegoCore.StegoMethods.Lsb;
using System.Diagnostics;
using StegoRevealer.StegoCore.StegoMethods;

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class ExtractorViewModel : MainWindowViewModelBaseChild
{
    // Стандартные значения текстовых полей
    private const string ImageNotSelectedText = "Изображение не выбрано";

    // Параметры извлечения
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
    /// Выбран ли метод НЗБ
    /// </summary>
    public bool MethodLsbSelected
    {
        get => _methodLsbSelected;
        set => this.RaiseAndSetIfChanged(ref _methodLsbSelected, value);
    }
    private bool _methodLsbSelected = true;

    /// <summary>
    /// Выбран ли метод Коха-Жао
    /// </summary>
    public bool MethodKzSelected
    {
        get => _methodKzSelected;
        set => this.RaiseAndSetIfChanged(ref _methodKzSelected, value);
    }
    private bool _methodKzSelected = false;

    /// <summary>
    /// Выбран ли последовательный способ
    /// </summary>
    public bool LinearModeSelected
    {
        get => _linearModeSelected;
        set => this.RaiseAndSetIfChanged(ref _linearModeSelected, value);
    }
    private bool _linearModeSelected = true;

    /// <summary>
    /// Выбран ли псевдослучайный способ
    /// </summary>
    public bool RandomModeSelected
    {
        get => _randomModeSelected;
        set => this.RaiseAndSetIfChanged(ref _randomModeSelected, value);
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
    /// 
    /// </summary>
    public bool LsbRandomSeedSelected
    {
        get => _lsbRandomSeedSelected;
        set => this.RaiseAndSetIfChanged(ref _lsbRandomSeedSelected, value);
    }
    private bool _lsbRandomSeedSelected = false;

    /// <summary>
    /// Стартовый индекс извлечения для НЗБ
    /// </summary>
    public int LsbStartIndexValue
    {
        get => _lsbStartIndexValue;
        set => this.RaiseAndSetIfChanged(ref _lsbStartIndexValue, value);
    }
    private int _lsbStartIndexValue = 0;

    /// <summary>
    /// 
    /// </summary>
    public bool LsbStartIndexSelected
    {
        get => _lsbStartIndexSelected;
        set => this.RaiseAndSetIfChanged(ref _lsbStartIndexSelected, value);
    }
    private bool _lsbStartIndexSelected = false;

    /// <summary>
    /// Байтовая длина для извлечения по НЗБ
    /// </summary>
    public int LsbByteLengthValue
    {
        get => _lsbByteLengthValue;
        set => this.RaiseAndSetIfChanged(ref _lsbByteLengthValue, value);
    }
    private int _lsbByteLengthValue = 0;

    /// <summary>
    /// 
    /// </summary>
    public bool LsbByteLengthSelected
    {
        get => _lsbByteLengthSelected;
        set => this.RaiseAndSetIfChanged(ref _lsbByteLengthSelected, value);
    }
    private bool _lsbByteLengthSelected = false;

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
    /// 
    /// </summary>
    public bool KzRandomSeedSelected
    {
        get => _kzRandomSeedSelected;
        set => this.RaiseAndSetIfChanged(ref _kzRandomSeedSelected, value);
    }
    private bool _kzRandomSeedSelected = false;

    /// <summary>
    /// Индекс начального блока для извлечения по Коха-Жао
    /// </summary>
    public int KzIndexFirstValue
    {
        get => _kzIndexFirstValue;
        set => this.RaiseAndSetIfChanged(ref _kzIndexFirstValue, value);
    }
    private int _kzIndexFirstValue = 0;

    /// <summary>
    /// Индекс конечного блока для извлечения по Коха-Жао
    /// </summary>
    public int KzIndexSecondValue
    {
        get => _kzIndexSecondValue;
        set => this.RaiseAndSetIfChanged(ref _kzIndexSecondValue, value);
    }
    private int _kzIndexSecondValue = 0;

    /// <summary>
    /// 
    /// </summary>
    public bool KzIndexesSelected
    {
        get => _kzIndexesSelected;
        set => this.RaiseAndSetIfChanged(ref _kzRandomSeedSelected, value);
    }
    private bool _kzIndexesSelected = false;

    /// <summary>
    /// Порог для извлечения информации по Коха-Жао
    /// </summary>
    public double KzThresholdValue
    {
        get => _kzThresholdValue;
        set => this.RaiseAndSetIfChanged(ref _kzThresholdValue, value);
    }
    private double _kzThresholdValue = 120.0;

    /// <summary>
    /// 
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
    /// Существуют ли результаты проведённого извлечения
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
    /// Актуальные результаты стегоанализа
    /// </summary>
    public ExtractionResultsDto? CurrentResults
    {
        get => _currentResults;
        private set
        {
            _currentResults = value;
            HasResults = value is not null;
        }
    }
    private ExtractionResultsDto? _currentResults = null;

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
        WindowResizeAction += SetImagePreviewSizes;
        if (_mainWindowViewModel.MainWindow is not null)
            _mainWindowViewModel.MainWindow.SizeChanged += (object? sender, SizeChangedEventArgs e) => WindowResizeAction();
    }

    public ExtractorViewModel(MainWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList)
    {
        CreateDefaults();
    }

    [Experimental]
    public ExtractorViewModel() : base()
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
            CurrentImage?.CloseHandler();
            CurrentImage = new ImageHandler(path);
            ActualizeParameters();  // Обновит ссылку на изображение в параметрах или создат объекты параметров, если их нет
            DrawCurrentImage();  // Обновит изображение, отображаемое на форме
            return true;
        }
        catch { }

        return false;
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
            ResetImageAndResults();

        // Загрузка
        return CreateCurrentImageHandler(path);
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
    /// Запуск процесса извлечения для указанных выбранных методов
    /// </summary>
    public void StartExtraction()
    {
        UpdateParameters();

        var timer = Stopwatch.StartNew();  // Запуск таймера - подсчёт времени работы непосредственно методов стеганографии

        // Запуск
        if (_lsbParameters is null && _kzhParameters is null)
            throw new Exception("Параметры не указаны");

        var results = new ExtractionResultsDto();
        if (_lsbParameters is not null)  // Извлечение из НЗБ
        {
            var extractor = new LsbExtractor(_lsbParameters.Image);
            extractor.Params.Seed = _lsbParameters.Seed;
            extractor.Params.ToExtractBitLength = _lsbParameters.ToExtractBitLength;
            extractor.Params.StartPixels = _lsbParameters.StartPixels;
            extractor.Params.TraverseType = _lsbParameters.TraverseType;

            var lsbResult = extractor.Extract() as LsbExtractResult;
            results.ExtractedMessage = lsbResult?.ResultData ?? string.Empty;
        }
        else if (_kzhParameters is not null)  // Извлечение по Коха-Жао
        {
            var extractor = new KochZhaoExtractor(_kzhParameters.Image);
            extractor.Params.Seed = _kzhParameters.Seed;
            extractor.Params.Threshold = _kzhParameters.Threshold;
            extractor.Params.ToExtractBitLength = _kzhParameters.ToExtractBitLength;
            extractor.Params.StartBlocks = _kzhParameters.StartBlocks;
            extractor.Params.TraverseType = _kzhParameters.TraverseType;

            var kzResult = extractor.Extract() as KochZhaoExtractResult;
            results.ExtractedMessage = kzResult?.ResultData ?? string.Empty;
        }

        timer.Stop();  // Остановка таймера
        results.ElapsedTime = timer.ElapsedMilliseconds;

        CurrentResults = results;
    }

    public void UpdateParameters()
    {
        if (MethodLsbSelected && _lsbParameters is not null)
        {
            if (RandomModeSelected)
                _lsbParameters.Seed = LsbRandomSeedValue;
            _lsbParameters.ToExtractBitLength = (LsbByteLengthSelected ? LsbByteLengthValue : 0) * 8;  // По умолчанию (без указания) = 0

            // Считаем, что извлекаем только: чересканально, все 3 канала задействованы (порядок R,G,B), использован 1 НЗБ
            // Фактически, указывается индекс всего пикселя (одинаковых для всех трёх каналов) - т.к. методы обхода
            //    для кодера и декодера одинаково работают: чересканальность работает от красного к синему, беря StartIndexes,
            //    нет варианта сокрытия начиная с зелёного или синего канала, могут только отличаться индексы между собой по каналам.
            if (LsbStartIndexSelected)
            {
                int commonSkip = LsbStartIndexValue;  // Считаем, что указан индекс всего пикселя (убрал "/ 3");
                var startPixels = new StartValues(
                    (ImgChannel.Red, commonSkip), (ImgChannel.Green, commonSkip), (ImgChannel.Blue, commonSkip));
            }

            // LsbStartIndex не имеет значения, если задан LsbSeed!
        }

        if (MethodKzSelected && _kzhParameters is not null)
        {
            if (RandomModeSelected)
                _kzhParameters.Seed = KzRandomSeedValue;
            if (KzThresholdSelected)
                _kzhParameters.Threshold = KzThresholdValue;

            if (KzIndexesSelected)
            {
                _kzhParameters.StartBlocks = new StartValues((ImgChannel.Blue, KzIndexFirstValue));

                // Считаем, что скрытие производилось только в синий канал
                if (KzIndexSecondValue > 0)
                {
                    int bitsNum = KzIndexSecondValue - KzIndexFirstValue + 1;
                    _kzhParameters.ToExtractBitLength = bitsNum;
                }
            }

            // KzIndexFirst и KzIndexSecond не имеют значения, если задан KzSeed!
        }
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
    /// Сброс результатов извлечения
    /// </summary>
    public void ResetResults() => CurrentResults = null;


    // Сбрасывает данные об изображении и результатах
    private void ResetImageAndResults()
    {
        ImagePath = ImageNotSelectedText;
        DrawedImage = null;
        ResetResults();
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
}
