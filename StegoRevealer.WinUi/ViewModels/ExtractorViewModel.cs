using Microsoft.Win32;
using SkiaSharp;
using StegoRevealer.StegoCore.AnalysisMethods;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using StegoRevealer.StegoCore.StegoMethods.Lsb;
using StegoRevealer.WinUi.Lib;
using StegoRevealer.WinUi.Lib.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StegoRevealer.StegoCore.StegoMethods;

namespace StegoRevealer.WinUi.ViewModels
{
    /// <summary>
    /// ViewModel представления Extractor - окно экстрактора
    /// </summary>
    public class ExtractorViewModel : BaseChildViewModel
    {
        // Параметры извлечения
        private LsbParameters? _lsbParameters = null;
        private KochZhaoParameters? _kzhParameters = null;

        private ExtractionResultsDto? _currentResults = null;

        /// <summary>
        /// Существуют ли результаты проведённого извлечения
        /// </summary>
        public bool HasResults { get => _currentResults is not null; }

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


        public ExtractorViewModel(RootViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }


        /// <summary>
        /// Перейти к главному окну
        /// </summary>
        public void SwitchToMainView()
        {
            var mainViewModel = _rootViewModel.GetOrCreateViewModel(typeof(MainViewModel)) as MainViewModel;
            if (mainViewModel is not null)
                _rootViewModel.CurrentViewModel = mainViewModel;
        }

        /// <summary>
        /// Осуществляет загрузку выбираемого пользователем изображения
        /// </summary>
        public bool TryLoadImage()
        {
            // Сразу "отпускаем" текущее изображение
            CurrentImage?.CloseHandler();
            DrawedImage = null;
            DrawedImageSource = null;

            // Выбор файла
            string path = SelectNewImageFile();

            // Загрузка
            return CreateCurrentImageHandler(path);
        }

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
        /// Заново формирует отображаемое на View изображение из текущего сохранённого
        /// </summary>
        public void DrawCurrentImage()
        {
            if (CurrentImage is not null)
                DrawedImage = CurrentImage;
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
        }

        // TODO: Перенести метод в какую-нибудь общую библиотеку
        /// <summary>
        /// Создаёт источник для отображения изображения
        /// </summary>
        public static ImageSource CreateImageSource(ImageHandler image)
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
        /// Возвращает текущие сохранённые результаты стегоанализа
        /// </summary>
        public ExtractionResultsDto? GetCurrentResults() => _currentResults;

        /// <summary>
        /// Запуск процесса извлечения для указанных выбранных методов
        /// </summary>
        public void StartExtraction()
        {
            var timer = Stopwatch.StartNew();  // Запуск таймера - подсчёт времени работы непосредственно методов стегоанализа

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

            _currentResults = results;
        }

        public void SetParameters(ExtractionParams extractionParams)
        {
            if (_lsbParameters is not null)
            {
                if (extractionParams.RandomHided)
                    _lsbParameters.Seed = extractionParams.LsbSeed;
                _lsbParameters.ToExtractBitLength = (extractionParams.LsbByteLength ?? 0) * 8;  // По умолчанию (без указания) = 0

                // Считаем, что извлекаем только: чересканально, все 3 канала задействованы (порядок R,G,B), использован 1 НЗБ
                // Фактически, указывается индекс всего пикселя (одинаковых для всех трёх каналов) - т.к. методы обхода
                //    для кодера и декодера одинаково работают: чересканальность работает от красного к синему, беря StartIndexes,
                //    нет варианта сокрытия начиная с зелёного или синего канала, могут только отличаться индексы между собой по каналам.
                if (extractionParams.LsbStartIndex is not null)
                {
                    int commonSkip = extractionParams.LsbStartIndex.Value;  // Считаем, что указан индекс всего пикселя (убрал "/ 3");
                    var startPixels = new StartValues(
                        (ImgChannel.Red, commonSkip), (ImgChannel.Green, commonSkip), (ImgChannel.Blue, commonSkip));
                }

                // LsbStartIndex не имеет значения, если задан LsbSeed!
            }

            if (_kzhParameters is not null)
            {
                if (extractionParams.RandomHided)
                    _kzhParameters.Seed = extractionParams.KzSeed;
                if (extractionParams.KzThreshold is not null)
                    _kzhParameters.Threshold = extractionParams.KzThreshold.Value;
                if (extractionParams.KzIndexFirst.HasValue)
                    _kzhParameters.StartBlocks = new StartValues((ImgChannel.Blue, extractionParams.KzIndexFirst.Value));

                // Считаем, что скрытие производилось только в синий канал
                if (extractionParams.KzIndexFirst.HasValue && extractionParams.KzIndexSecond.HasValue && extractionParams.KzIndexSecond.Value > 0)
                {
                    int bitsNum = extractionParams.KzIndexSecond.Value - extractionParams.KzIndexFirst.Value + 1;
                    _kzhParameters.ToExtractBitLength = bitsNum;
                }

                // KzIndexFirst и KzIndexSecond не имеют значения, если задан KzSeed!
            }
        }
    }
}
