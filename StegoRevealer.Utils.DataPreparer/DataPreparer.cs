using CsvHelper;
using CsvHelper.Configuration;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using StegoRevealer.StegoCore.StegoMethods.Lsb;
using StegoRevealer.Utils.DataPreparer.Entities;
using StegoRevealer.Utils.DataPreparer.Lib;
using StegoRevealer.Utils.DataPreparer.Lib.TaskPool;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;

namespace StegoRevealer.Utils.DataPreparer;

public class DataPreparer
{
    private StartParams StartParams { get; init; }

    private CsvConfiguration CsvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = ",",
        HasHeaderRecord = true
    };

    private Lazy<Logger> LoggerFactory = new Lazy<Logger>(() => new Logger(Constants.OutputLogFilePath));
    private Logger Logger => LoggerFactory.Value;

    private TaskPool ImgProcessingPool;
    private TaskPool CalculationsPool;


    public DataPreparer() : this(new StartParams()) { }
    public DataPreparer(StartParams startParams)
    {
        StartParams = startParams;

        ImgProcessingPool = new TaskPool(considerOnlyRunningTasks: false);
        CalculationsPool = new TaskPool(considerOnlyRunningTasks: startParams.UseWeakPoolForCalculations);

        // "Weak pool" - "Слабый пул" - т.е. с проверкой только реально работающих (OnlyRunningTasks).
        // "Strong pool" - "Сильный пул" - держит число задач жёстко согласно лимиту, и неважно, в каком они состоянии
    }


    // Для каждого изображения делаем следующее:
    // - выбираем случайное количество диапазонов (из 10 доступных) и выбираем нужное количество случайных различных диапазонов;
    // - в каждом из выбранных диапазонов объёма скрываемой информации (0%-10%, ..., 90%-100%, в % от допустимого объёма контейнера):
    //    - "подбрасываем монетку": если да - скрываем случайно выбранную информацию случайно выбранного объёма в рамках диапазона последовательно в НЗБ;
    //    - "подбрасываем монетку": если да - скрываем случайно выбранную информацию случайно выбранного объёма в рамках диапазона псевдослучайно в НЗБ;
    //    - "подбрасываем монетку": если да - скрываем случайно выбранную информацию случайно выбранного объёма в рамках диапазона последовательно по Коха-Жао;
    // - для каждой полученной таким образом картинки, а также оригинальной создаём .txt-файл, где записываем:
    //     имя оригинального изображения, метод скрытия, вид скрытия, объём скрытой информации в %, битовый объём скрытой информации, стартовый индекс данных, скрытые данные;
    // - все эти .txt-файлы, оригинальные и новые полученные изображения записываем в каталог output;
    // - все имена файлов и информацию о том, есть ли в нём скрытая информация, записываем в список.

    // После скрытия данных осуществляем анализ и составляем данные для обучения:
    // - перемешиваем список файлов и данных о скрытии;
    // - каждую картинку анализируем и собираем следующие данные:
    //     ChiSqr, RS, шум, резкость, размытость, контрастность, оценки энтропии, ширина и высота...;
    // - собранные данные записываем в CSV-файл.
    public void Execute()
    {
        Logger.LogInfo($"Запущен скрипт подготовки данных анализа для обучения нейросети модуля принятия решений Stego Revealer в {DateTime.Now:ss:mm:HH dd:MM:yyyy}");
        var overallTimer = Stopwatch.StartNew();  // Глобальный таймер
        
        Logger.LogInfo($"Стартовые параметры:\n\tInputDataImagesDirPath = {Constants.InputDataImagesDirPath}\n\tInputDataTextFilePath = {Constants.InputDataTextFilePath}\n" +
            $"\tOutputDirPath = {Constants.OutputDirPath}\n\tOutputLogFilePath = {Constants.OutputLogFilePath}\n\tOutputAnalysisDataFilePath = {Constants.OutputAnalysisDataFilePath}\n" +
            $"\tNoHidingChangeAdvantage = {Constants.NoHidingChangeAdvantage}\n\tProcessorCount = {Environment.ProcessorCount}");

        // Очистка выходного каталога
        ClearOutputDirectory(onlyAnalysisFilesClear: StartParams.SkipPreparing);

        // Запуск операций
        var preparingResult = PrepareOperationForManyHidings();
        var analysisResult = AnalyseOperation(preparingResult.OutputImages);

        // Остановка таймера, окончание работы программы
        overallTimer.Stop();
        Logger.LogInfo("Завершена работа скрипта подготовки данных анализа для обучения нейросети модуля принятия решений Stego Revealer, длительность: " +
            Helper.GetFormattedElapsedTime(overallTimer.ElapsedMilliseconds) +
            (!(StartParams.SkipPreparing && StartParams.SkipAnalysis) ? ", из них:" : "") +
            (!StartParams.SkipPreparing ? ("\n\tвремя подготовки картинок: " + Helper.GetFormattedElapsedTime(preparingResult.ElapsedTime)) : "") +
            (!StartParams.SkipAnalysis ? ("\n\tвремя анализа картинок: " + Helper.GetFormattedElapsedTime(analysisResult.ElapsedTime)) : ""));
    }

    // //////
    // Подготовка картинок со скрытой информацией
    // //////
    private ImagesPreparingResult PrepareOperationForManyHidings()
    {
        var result = new ImagesPreparingResult();

        if (StartParams.SkipPreparing)
        {
            Logger.LogWarning("Шаг подготовки изображений и скрытия информации пропущен согласно входной настройке");
            return result;
        }


        var rnd = new Random();
        var imagePreparingTimer = Stopwatch.StartNew();  // Таймер подготовки картинок

        // Инициализация данных
        var inputImages = Directory.GetFiles(Constants.InputDataImagesDirPath);
        TextDataHelper.InitializeTextFile(Constants.InputDataTextFilePath);

        inputImages = inputImages.Where(imgPath => Constants.ImagesExtensions.Contains(Path.GetExtension(imgPath))).ToArray();
        Logger.LogInfo("Изображения, взятые в обработку:");
        Logger.LogRawEnumerable(inputImages, asColumn: true, toString: str => $"\t{str}");
        Logger.LogSeparator();

        Logger.LogInfo("Начат процесс обработки изображений: подготовки скрытия информации в них различными способами");

        // Формирование и запуск задач по каждому изображению
        var imagePreparingTasks = new List<Task>();
        int k = 1;
        foreach (var imgPath in inputImages)
        {
            int index = k;
            imagePreparingTasks.Add(CreateTask(() => PreparingAction(imgPath, index, inputImages.Length, result.OutputImages), taskPool: ImgProcessingPool));

            k++;
        }

        // Ожидание задач подготовки изображений
        foreach (var imagePreparingTask in imagePreparingTasks)
        {
            imagePreparingTask.Wait();
            imagePreparingTask.Dispose();
        }

        imagePreparingTimer.Stop();
        result.ElapsedTime = imagePreparingTimer.ElapsedMilliseconds;

        Logger.LogInfo("Завершена обработка изображений и операции по скрытию данных, длительность: " +
            Helper.GetFormattedElapsedTime(result.ElapsedTime));
        Logger.LogSeparator();

        return result;
    }

    private void PreparingAction(string imgPath, int index, int count, ConcurrentBag<OutputImage> outputImages)
    {
        var rnd = new Random();
        var img = new ImageHandler(imgPath);
        var allDiapasonesId = Enumerable.Range(1, 10).ToList();
        Logger.LogInfo($"Начата обработка изображения {img.ImgName} ({index} / {count})");

        if (StartParams.ManyHidings)
        {
            outputImages.Add(CreateOutputImageWithNoHiding(imgPath));

            int diapasonesNum = Math.Max(0, rnd.Next(-Constants.NoHidingChangeAdvantage, 11));
            Logger.LogInfo($"Для изображения {img.ImgName} выбрано выбрано количество диапазонов: {diapasonesNum}");

            if (diapasonesNum > 0)
            {
                var selectedDiapasones = allDiapasonesId.OrderBy(e => rnd.Next()).Take(diapasonesNum).ToList();
                Logger.LogInfo($"Выбранные диапазоны для изображения {img.ImgName}: " +
                    $"{string.Join(", ", selectedDiapasones.Select(id => Constants.Diapasones[id].ToString()))}");

                var diapasonesTasks = new List<Task>();
                foreach (var selectedDiapasoneId in selectedDiapasones)
                    diapasonesTasks.Add(CreateTask(() => HideInDiapasone(img, Constants.Diapasones[selectedDiapasoneId], outputImages), taskPool: null));

                foreach (var task in diapasonesTasks)
                {
                    task.Wait();
                    task.Dispose();
                }
            }
        }
        else
        {
            bool hide = rnd.Next(0, 2) == 1;
            if (hide)
            {
                int diapasoneId = rnd.Next(0, Constants.Diapasones.Count);
                Logger.LogInfo($"Выбранный диапазон для изображения {img.ImgName}: " +
                    $"{string.Join(", ", Constants.Diapasones[diapasoneId].ToString())}");

                var hidingTask = CreateTask(() => HideInDiapasone(img, Constants.Diapasones[diapasoneId], outputImages), taskPool: null);
                hidingTask.Wait();
            }
            else
                outputImages.Add(CreateOutputImageWithNoHiding(imgPath));
        }

        img.CloseHandler();
        Logger.LogInfo($"Завершена обработка изображения {img.ImgName}");
    }

    private OutputImage CreateOutputImageWithNoHiding(string imgPath)
    {
        string imgName = Path.GetFileName(imgPath);
        string originImageCopyPath = Path.Combine(Constants.OutputDirPath, imgName);
        File.Copy(imgPath, originImageCopyPath);
        Logger.LogInfo($"Изображение {imgName} с пустым контейнером добавлено в выходной каталог");
        return new OutputImage { Path = originImageCopyPath, Hided = false };
    }

    // //////
    // Анализ и сбор данных
    // //////
    private ImagesAnalysisResult AnalyseOperation(ConcurrentBag<OutputImage> outputImages)
    {
        var result = new ImagesAnalysisResult();

        if (StartParams.SkipAnalysis)
        {
            Logger.LogWarning("Шаг анализа изображений и сбора данных пропущен согласно входной настройке");
            return result;
        }


        var rnd = new Random();
        var imageAnalysisTimer = Stopwatch.StartNew();  // Таймер анализа и сбора данных

        if (outputImages.IsEmpty)
        {
            var outputFiles = Directory.GetFiles(Constants.OutputDirPath).ToList();
            var outputImageFiles = outputFiles.Where(path => Constants.ImagesExtensions.Contains(Path.GetExtension(path))).ToList();
            outputFiles = outputFiles.Select(path => Path.GetFileName(path)).ToList();  // После получения списка путей картинок нужны только имена файлов каталога
            foreach (var outputImageFile in outputImageFiles)
            {
                string imgName = Path.GetFileNameWithoutExtension(outputImageFile);
                outputImages.Add(new OutputImage
                {
                    Path = outputImageFile,

                    // Если записали инфо-файл, значит скрытие было, картинки без инфо-файлов считаются как картинки с пустым контейнером
                    Hided = outputFiles.Contains(imgName + Constants.InfoFilePostfix + Constants.InfoFileExt)
                });
            }
        }

        var shuffledOutputImages = outputImages.OrderBy(e => rnd.Next()).ToList();
        Logger.LogInfo("Все изображения, принятые к анализу:");
        Logger.LogRawEnumerable(shuffledOutputImages, asColumn: true, toString: oi => $"\t{Path.GetFileName(oi.Path)}");
        Logger.LogSeparator();

        var analysisData = new ConcurrentBag<ImageAnalysisData>();
        Logger.LogInfo("Начат процесс анализа и сбора данных об изображениях");

        // Временный .txt-файл с CSV-строками для уже обработанных изображений - на случай вылета во время анализа
        // (изображения со скрытием сохраняются сразу и так, а здесь при прерывании можно будет получить те данные анализа, которые успели собрать)
        StreamWriter? tempAnalysisDataFileWriter = null;
        try
        {
            tempAnalysisDataFileWriter = new StreamWriter(Constants.OutputTempAnalysisDataFilePath, append: false);
            WriteTempFileHeader(tempAnalysisDataFileWriter);
            Logger.LogInfo($"Создан файл для временной записи проанализированных данных по пути '{Constants.OutputTempAnalysisDataFilePath}'");
        }
        catch (Exception ex)
        {
            tempAnalysisDataFileWriter = null;
            Logger.LogError("Не удалось создать файл для временной записи проанализированных данных. Ошибка: \n" + ex.Message);
        }

        // Формирование и запуск задач анализа для каждого изображения
        var imageAnalysisTasks = new List<Task>();
        int k = 0;
        foreach (var imageInfo in shuffledOutputImages)
        {
            int index = k;
            imageAnalysisTasks.Add(CreateTask(() =>
            {
                string imgName = Path.GetFileName(imageInfo.Path);
                Logger.LogInfo($"Начат анализ изображения {imgName} ({index} / {outputImages.Count})");
                var analysisResult = AnalyseImage(imageInfo);

                if (analysisResult is not null)
                {
                    analysisData.Add(analysisResult);
                    Logger.LogInfo($"Успешно завершён анализ изображения {imgName}, собранные данные записаны в финальный датасет");

                    WriteAsCsvStringToTempFile(tempAnalysisDataFileWriter, analysisResult, imgName);
                }
                else
                {
                    Logger.LogError($"Ошибка при анализе изображения {imgName}, данные не включены в финальный датасет");
                }

                GC.Collect();
            }, taskPool: ImgProcessingPool));

            k++;
        }

        // Ожидание задач анализа
        foreach (var imageAnalysisTask in imageAnalysisTasks)
        {
            imageAnalysisTask.Wait();
            imageAnalysisTask.Dispose();
        }

        tempAnalysisDataFileWriter?.Close();
        imageAnalysisTimer.Stop();
        result.ElapsedTime = imageAnalysisTimer.ElapsedMilliseconds;

        Logger.LogInfo($"Завершён анализ изображений и сбор данных по ним для финального датасета, длительность: " +
            Helper.GetFormattedElapsedTime(result.ElapsedTime));
        Logger.LogSeparator();

        // Запись результатов анализа и сбора данных в CSV
        WriteAnalysisDataToCsv(analysisData);
        Logger.LogInfo("Собранные данные записаны в CSV-файл");

        return result;
    }

    private OutputImageProcessingInfo? HideAsLinearLsb(ImageHandler img, MinMaxData diapasone)
    {
        Logger.LogInfo($"Процесс линейного скрытия в НЗБ для {img.ImgName} в диапазоне {diapasone}");
        var timer = Stopwatch.StartNew();

        int lsbBitVolume = ContainerVolumeForLsb(img);
        int minBitLengthLsb = (int)((double)(lsbBitVolume * diapasone.Min) / 100);
        int maxBitLengthLsb = (int)((double)(lsbBitVolume * diapasone.Max) / 100);

        var dataToHide = TextDataHelper.GetRandomDataPartWithRandomBitLength(Constants.InputDataTextFilePath, minBitLengthLsb, maxBitLengthLsb);
        if (dataToHide is not null)
        {
            var rnd = new Random();
            var traverseType = rnd.Next(0, 2) == 1 ? TraverseType.Horizontal : TraverseType.Vertical;

            string newImgName = img.ImgName + $"_{diapasone.Min}_{diapasone.Max}_linearLsb";
            string newImagePath = Path.Combine(Constants.OutputDirPath, newImgName + Path.GetExtension(img.ImgPath));
            var hider = new LsbHider(img);
            hider.Params.TraverseType = traverseType;

            var hideTask = CreateTask(() => hider.Hide(dataToHide.Data, newImagePath), taskPool: CalculationsPool);
            hideTask.Wait();
            var hideResult = hideTask.Result;
            timer.Stop();
            hideTask.Dispose();

            var resultPath = hideResult.GetResultPath();
            if (string.IsNullOrEmpty(resultPath))
            {
                Logger.LogError($"При линейном НЗБ скрытии для {img.ImgName} в диапазоне {diapasone} не получен путь сохранённого изображения");
                return null;
            }

            Logger.LogInfo($"Процесс линейного скрытия в НЗБ для {img.ImgName} в диапазоне {diapasone} завершён, длительность: " +
                Helper.GetFormattedElapsedTime(timer.ElapsedMilliseconds));

            var outputImageInfo = new OutputImageProcessingInfo
            {
                ImagePath = resultPath,
                OriginImagePath = img.ImgPath,
                Diapasone = diapasone,
                StegoMethod = StegoMethod.LineraLsb,
                HidedVolumePercent = (int)((double)(dataToHide.RandomLength ?? 0) / lsbBitVolume * 100),
                HiddedBitVolume = dataToHide.RandomLength ?? 0,
                TextFileStartIndex = dataToHide.StartIndex,
                HidedData = dataToHide.Data,
                Seed = null
            };
            return outputImageInfo;
        }

        Logger.LogError($"Не удалось получить данные для линейного скрытия в НЗБ в изображении {img.ImgName}");
        return null;
    }

    private OutputImageProcessingInfo? HideAsRandomLsb(ImageHandler img, MinMaxData diapasone)
    {
        Logger.LogInfo($"Процесс псевдослучайного скрытия в НЗБ для {img.ImgName} в диапазоне {diapasone}");
        var timer = Stopwatch.StartNew();

        int lsbBitVolume = ContainerVolumeForLsb(img);
        int minBitLengthLsb = (int)((double)(lsbBitVolume * diapasone.Min) / 100);
        int maxBitLengthLsb = (int)((double)(lsbBitVolume * diapasone.Max) / 100);

        var dataToHide = TextDataHelper.GetRandomDataPartWithRandomBitLength(Constants.InputDataTextFilePath, minBitLengthLsb, maxBitLengthLsb);
        if (dataToHide is not null)
        {
            var rnd = new Random();
            int seed = rnd.Next();

            string newImgName = img.ImgName + $"_{diapasone.Min}_{diapasone.Max}_randomLsb";
            string newImagePath = Path.Combine(Constants.OutputDirPath, newImgName + Path.GetExtension(img.ImgPath));
            var hider = new LsbHider(img);
            hider.Params.Seed = seed;

            var hideTask = CreateTask(() => hider.Hide(dataToHide.Data, newImagePath), taskPool: CalculationsPool);
            hideTask.Wait();
            var hideResult = hideTask.Result;
            timer.Stop();
            hideTask.Dispose();

            var resultPath = hideResult.GetResultPath();
            if (string.IsNullOrEmpty(resultPath))
            {
                Logger.LogError($"При псевдослучайном НЗБ скрытии для {img.ImgName} в диапазоне {diapasone} не получен путь сохранённого изображения");
                return null;
            }

            Logger.LogInfo($"Процесс псевдослучайного скрытия в НЗБ для {img.ImgName} в диапазоне {diapasone} завершён, длительность: " +
                Helper.GetFormattedElapsedTime(timer.ElapsedMilliseconds));

            var outputImageInfo = new OutputImageProcessingInfo
            {
                ImagePath = resultPath,
                OriginImagePath = img.ImgPath,
                Diapasone = diapasone,
                StegoMethod = StegoMethod.RandomLsb,
                HidedVolumePercent = (int)((double)(dataToHide.RandomLength ?? 0) / lsbBitVolume * 100),
                HiddedBitVolume = dataToHide.RandomLength ?? 0,
                TextFileStartIndex = dataToHide.StartIndex,
                HidedData = dataToHide.Data,
                Seed = seed
            };
            return outputImageInfo;
        }

        Logger.LogError($"Не удалось получить данные для псевдослучайного скрытия в НЗБ в изображении {img.ImgName}");
        return null;
    }

    private OutputImageProcessingInfo? HideAsLinearKzh(ImageHandler img, MinMaxData diapasone)
    {
        Logger.LogInfo($"Процесс линейного скрытия по Коха-Жао для {img.ImgName} в диапазоне {diapasone}");
        var timer = Stopwatch.StartNew();

        int kzhBitVolume = ContainerVolumeForKzh(img);
        int minBitLengthKzh = (int)((double)(kzhBitVolume * diapasone.Min) / 100);
        int maxBitLengthKzh = (int)((double)(kzhBitVolume * diapasone.Max) / 100);

        var dataToHide = TextDataHelper.GetRandomDataPartWithRandomBitLength(Constants.InputDataTextFilePath, minBitLengthKzh, maxBitLengthKzh);
        if (dataToHide is not null)
        {
            var rnd = new Random();
            var traverseType = rnd.Next(0, 2) == 1 ? TraverseType.Horizontal : TraverseType.Vertical;

            string newImgName = img.ImgName + $"_{diapasone.Min}_{diapasone.Max}_linearKzh";
            string newImagePath = Path.Combine(Constants.OutputDirPath, newImgName + Path.GetExtension(img.ImgPath));
            var hider = new KochZhaoHider(img);
            hider.Params.TraverseType = traverseType;
            hider.Params.Threshold = rnd.Next(50, 120);

            var hideTask = CreateTask(() => hider.Hide(dataToHide.Data, newImagePath), taskPool: CalculationsPool);
            hideTask.Wait();
            var hideResult = hideTask.Result;
            timer.Stop();
            hideTask.Dispose();

            var resultPath = hideResult.GetResultPath();
            if (string.IsNullOrEmpty(resultPath))
            {
                Logger.LogError($"При линейном Коха-Жао скрытии для {img.ImgName} в диапазоне {diapasone} не получен путь сохранённого изображения");
                return null;
            }

            Logger.LogInfo($"Процесс линейного скрытия по Коха-Жао для {img.ImgName} в диапазоне {diapasone} завершён, длительность: " +
                Helper.GetFormattedElapsedTime(timer.ElapsedMilliseconds));

            var outputImageInfo = new OutputImageProcessingInfo
            {
                ImagePath = resultPath,
                OriginImagePath = img.ImgPath,
                Diapasone = diapasone,
                StegoMethod = StegoMethod.LinearKzh,
                HidedVolumePercent = (int)((double)(dataToHide.RandomLength ?? 0) / kzhBitVolume * 100),
                HiddedBitVolume = dataToHide.RandomLength ?? 0,
                TextFileStartIndex = dataToHide.StartIndex,
                HidedData = dataToHide.Data,
                Seed = null
            };
            return outputImageInfo;
        }

        Logger.LogError($"Не удалось получить данные для линейного скрытия по Коха-Жао в изображении {img.ImgName}");
        return null;
    }

    private void HideInDiapasone(ImageHandler img, MinMaxData diapasone, ConcurrentBag<OutputImage> outputImages)
    {
        var rnd = new Random();
        var hideDecisions = new List<bool>(3);

        if (StartParams.ManyHidings)
        {
            for (int i = 0; i < 3; i++)
                hideDecisions[i] = rnd.Next(0, 2) == 1;
            if (!hideDecisions.Any(dec => dec == true))
                hideDecisions[0] = true;
        }
        else
        {
            var decisionId = rnd.Next(0, 3);
            hideDecisions[decisionId] = true;
        }

        var methodsLog = new List<string>();
        if (hideDecisions[0]) methodsLog.Add("линейный НЗБ");
        if (hideDecisions[1]) methodsLog.Add("псевдослучайный НЗБ");
        if (hideDecisions[2]) methodsLog.Add("линейный Коха-Жао");
        Logger.LogInfo($"Для изображения {img.ImgName} в диапазоне {diapasone} " +
            $"{(StartParams.ManyHidings ? "определены следующие методы" : "определён следующий метод")} скрытия: " + string.Join(", ", methodsLog));

        var currentHandlers = new List<ImageHandler>();
        var hidingTasks = new List<Task>();
        if (hideDecisions[0])
        {
            var currentHandler = img.Clone();
            currentHandlers.Add(currentHandler);
            hidingTasks.Add(CreateTask(() => ExecuteHiding(() => HideAsLinearLsb(currentHandler, diapasone), outputImages), taskPool: null));
        }
        if (hideDecisions[1])
        {
            var currentHandler = img.Clone();
            currentHandlers.Add(currentHandler);
            hidingTasks.Add(CreateTask(() => ExecuteHiding(() => HideAsRandomLsb(currentHandler, diapasone), outputImages), taskPool: null));
        }
        if (hideDecisions[2])
        {
            var currentHandler = img.Clone();
            currentHandlers.Add(currentHandler);
            hidingTasks.Add(CreateTask(() => ExecuteHiding(() => HideAsLinearKzh(currentHandler, diapasone), outputImages), taskPool: null));
        }

        foreach (var task in hidingTasks)
        {
            task.Wait();
            task.Dispose();
        }

        foreach (var handler in currentHandlers)
            handler.CloseHandler();
        Logger.LogInfo($"Завершено скрытие по выбранным методам для изображения {img.ImgName} в диапазоне {diapasone}");
    }

    private void ExecuteHiding(Func<OutputImageProcessingInfo?> hidingMethod, ConcurrentBag<OutputImage> outputImages)
    {
        var processingResult = hidingMethod();
        if (processingResult is not null)
        {
            WriteOutputImageProcessingInfo(processingResult);
            outputImages.Add(new OutputImage { Path = processingResult.ImagePath, Hided = true });

            Logger.LogInfo($"Завершена подготовка изображения {Path.GetFileName(processingResult.ImagePath)} " +
                $"со скрытыми данными на основе изображения {Path.GetFileName(processingResult.OriginImagePath)}, " +
                $"подробная информация записана в соответствующий текстовый файл");
        }

        GC.Collect();
    }

    private ImageAnalysisData? AnalyseImage(OutputImage imageInfo)
    {
        string imgName = Path.GetFileName(imageInfo.Path);
        Logger.LogInfo($"Начат процесс анализа и сбора данных для {imgName}");
        var timer = Stopwatch.StartNew();

        var img = new ImageHandler(imageInfo.Path);

        var horizonalChiSqr = new ChiSquareAnalyser(img);
        horizonalChiSqr.Params.TraverseType = TraverseType.Horizontal;

        var verticalChiSqr = new ChiSquareAnalyser(img);
        verticalChiSqr.Params.TraverseType = TraverseType.Vertical;
        verticalChiSqr.Params.BlockWidth = 1;
        verticalChiSqr.Params.BlockHeight = img.Height;

        var horizonotalKzha = new KzhaAnalyser(img);
        horizonotalKzha.Params.TraverseType = TraverseType.Horizontal;

        var verticalKzha = new KzhaAnalyser(img);
        verticalKzha.Params.TraverseType = TraverseType.Vertical;

        var rs = new RsAnalyser(img);
        var statm = new StatmAnalyser(img);


        ChiSquareResult? horizonalChiSqrResult = null;
        ChiSquareResult? verticalChiSqrResult = null;
        KzhaResult? horizonotalKzhaResult = null;
        KzhaResult? verticalKzhaResult = null;
        RsResult? rsResult = null;
        StatmResult? statmResult = null;
        var analysisTasks = new List<Task>
        {
            CreateTask(() => horizonalChiSqrResult = horizonalChiSqr.Analyse(), taskPool: CalculationsPool),
            CreateTask(() => verticalChiSqrResult = verticalChiSqr.Analyse(), taskPool: CalculationsPool),
            CreateTask(() => horizonotalKzhaResult = horizonotalKzha.Analyse(), taskPool: CalculationsPool),
            CreateTask(() => verticalKzhaResult = verticalKzha.Analyse(), taskPool: CalculationsPool),
            CreateTask(() => rsResult = rs.Analyse(), taskPool: CalculationsPool),
            CreateTask(() => statmResult = statm.Analyse(), taskPool: CalculationsPool)
        };

        foreach (var task in analysisTasks)
        {
            task.Wait();
            task.Dispose();
        }

        timer.Stop();

        if (horizonalChiSqrResult is null || verticalChiSqrResult is null || horizonotalKzhaResult is null || verticalKzhaResult is null ||
            rsResult is null || statmResult is null)
        {
            var resultsLog = new List<string>();
            if (horizonalChiSqrResult is null) resultsLog.Add("метод Хи-квадрат с горизонтальным обходом");
            if (verticalChiSqrResult is null) resultsLog.Add("метод Хи-квадрат с вертикальным обходом");
            if (horizonotalKzhaResult is null) resultsLog.Add("анализ Коха-Жао с горизонтальным обходом");
            if (verticalKzhaResult is null) resultsLog.Add("анализ Коха-Жао с вертикальным обходом");
            if (rsResult is null) resultsLog.Add("метод Regular-Singular");
            if (statmResult is null) resultsLog.Add("сбор статистических характеристик");
            Logger.LogError($"Не удалось получить результаты анализа для изображения {imgName} следующими методами: " + string.Join(", ", resultsLog));

            return null;
        }

        Logger.LogInfo($"Завершён анализ и сбор информации для изображения {imgName}, длительность: " + Helper.GetFormattedElapsedTime(timer.ElapsedMilliseconds));
        return new ImageAnalysisData
        {
            ChiSquareVolume = horizonalChiSqrResult.MessageRelativeVolume,
            RsVolume = rsResult.MessageRelativeVolume,
            KzhaThreshold = horizonotalKzhaResult.Threshold,
            KzhaMessageBitVolume = horizonotalKzhaResult.MessageBitsVolume,
            ChiSquareVolume_Vertical = verticalChiSqrResult.MessageRelativeVolume,
            KzhaThreshold_Vertical = verticalKzhaResult.Threshold,
            KzhaMessageBitVolume_Vertical = verticalKzhaResult.MessageBitsVolume,
            NoiseValue = statmResult.NoiseValueMethod2,
            SharpnessValue = statmResult.SharpnessValue,
            BlurValue = statmResult.BlurValue,
            ContrastValue = statmResult.ContrastValue,
            EntropyShennonValue = statmResult.EntropyValues.Shennon,
            EntropyVaidaValue = statmResult.EntropyValues.Vaida,
            EntropyTsallisValue = statmResult.EntropyValues.Tsallis,
            EntropyRenyiValue = statmResult.EntropyValues.Renyi,
            EntropyHavardValue = statmResult.EntropyValues.Havard,
            PixelsNum = img.Height * img.Width,
            DataWasHided = imageInfo.Hided ? 1 : 0
        };
    }

    private void WriteAnalysisDataToCsv(IEnumerable<ImageAnalysisData> data)
    {
        using (var fileWriter = new StreamWriter(Constants.OutputAnalysisDataFilePath))
        using (var csvWriter = new CsvWriter(fileWriter, CsvConfig))
        {
            csvWriter.WriteRecords(data);
            csvWriter.Flush();
        }
    }

    private static void WriteTempFileHeader(StreamWriter? file)
    {
        if (file is null)
            return;

        var properties = typeof(ImageAnalysisData).GetProperties();
        var values = new string[properties.Length + 1];

        for (int i = 0; i < properties.Length; i++)
            values[i + 1] = properties[i].Name;
        values[0] = "ImgName";

        string output = string.Join(",", values);
        file.WriteLine(output);
        file.Flush();
    }

    private static void WriteAsCsvStringToTempFile(StreamWriter? file, ImageAnalysisData data, string imgName)
    {
        if (file is null)
            return;

        var properties = data.GetType().GetProperties();
        var values = new string[properties.Length + 1];

        for (int i = 0; i < properties.Length; i++)
            values[i + 1] = properties[i].GetValue(data, null)?.ToString() ?? "";
        values[0] = imgName;

        values = values.Select(val => val.Replace(',', '.')).ToArray();

        string output = string.Join(",", values);
        file.WriteLine(output);
        file.Flush();
    }

    private static void WriteOutputImageProcessingInfo(OutputImageProcessingInfo info)
    {
        string imageName = Path.GetFileNameWithoutExtension(info.ImagePath);
        string infoFilePath = Path.Combine(Constants.OutputDirPath, imageName + Constants.InfoFilePostfix + Constants.InfoFileExt);

        string output = $"Оригинальное изображение: {info.OriginImagePath}\n" +
            $"Путь к полученному изображению: {info.ImagePath}\n\n" +
            $"Метод скрытия: {(info.StegoMethod is not StegoMethod.LinearKzh ? "НЗБ" : "Коха-Жао")}\n" +
            $"Вид скрытия: {(info.StegoMethod is not StegoMethod.RandomLsb ? "последовательный" : "псевдослучайный")}\n" +
            (info.Seed is not null ? $"Ключ ГПСЧ: {info.Seed}\n" : "") +
            (info.Seed is null ? $"Метод обхода при скрытии: {info.TraverseType}\n\n" : "\n") +
            $"Диапазон: [{info.Diapasone.Min}; {info.Diapasone.Max}]\n" +
            $"Реальный процент объёма, занятый скрытой информацией: {info.HidedVolumePercent}%\n" +
            $"Битовый объём скрытых данных: {info.HiddedBitVolume} бит\n\n" +
            $"Стартовый индекс выбранного отрывка: {info.TextFileStartIndex}\n\n" +
            $"Скрытый текстовый отрывок:\n" + info.HidedData;
        File.WriteAllText(infoFilePath, output);
    }

    private static void ClearOutputDirectory(bool onlyAnalysisFilesClear)
    {
        try
        {
            if (onlyAnalysisFilesClear)
            {
                if (File.Exists(Constants.OutputLogFilePath))
                    File.Delete(Constants.OutputLogFilePath);
                if (File.Exists(Constants.OutputAnalysisDataFilePath))
                    File.Delete(Constants.OutputAnalysisDataFilePath);
                if (File.Exists(Constants.OutputTempAnalysisDataFilePath))
                    File.Delete(Constants.OutputTempAnalysisDataFilePath);
            }
            else
                Directory.Delete(Constants.OutputDirPath, true);
        }
        catch { }

        if (!Directory.Exists(Constants.InputDataImagesDirPath))
            Directory.CreateDirectory(Constants.InputDataImagesDirPath);

        if (!Directory.Exists(Constants.OutputDirPath))
            Directory.CreateDirectory(Constants.OutputDirPath);
    }


    private static int ContainerVolumeForLsb(ImageHandler img) => img.Height * img.Width * 3;
    private static int ContainerVolumeForKzh(ImageHandler img) => (img.Height / 8) * (img.Width / 8);

    private Task<T> CreateTask<T>(Func<T> task, TaskPool? taskPool) => taskPool is not null ? taskPool.AddAsync(() => Task.Run(task)) : Task.Run(task);
    private Task CreateTask(Action task, TaskPool? taskPool) => taskPool is not null ? taskPool.AddAsync(() => Task.Run(task)) : Task.Run(task);
}
