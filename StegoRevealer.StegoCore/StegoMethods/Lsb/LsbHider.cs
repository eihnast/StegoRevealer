using System.Collections;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.CommonLib;
using System.Text.RegularExpressions;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using Accord;

namespace StegoRevealer.StegoCore.StegoMethods.Lsb;

/// <summary>
/// Метод скрытия информации в НЗБ
/// </summary>
public class LsbHider : IHider
{
    /// <summary>
    /// Параметры метода НЗБ
    /// </summary>
    public LsbParameters Params { get; set; }


    public LsbHider(ImageHandler img)
    {
        Params = new LsbParameters(img);
    }


    /// <summary>
    /// Допустимый объём контейнера
    /// </summary>
    private int GetContainerVolume()
    {
        var (w, h, d) = Params.Image.GetImgSizes();
        int pixelsToHideNum = w * h * Params.Channels.Count;
        return pixelsToHideNum * Params.LsbNum;
    }

    /// <summary>
    /// Определение скрываемого объёма информации (учитывает допустимый объём контейнера)
    /// </summary>
    private static int GetHidingVolume(int ContainerVolume, int DataVolume)
    {
        if (ContainerVolume > DataVolume)
            return DataVolume;
        return ContainerVolume;
    }

    /// <inheritdoc/>
    public IHideResult Hide(string? data, string? newImagePath = null) => HideAlgorithm(data, newImagePath);

    /// <inheritdoc/>
    public IHideResult Hide(IParams parameters, string? data, string? newImagePath = null)
    {
        LsbHideResult result = new();

        LsbParameters? lsbParams = parameters as LsbParameters;
        if (lsbParams is null)  // Не удалось привести к LsbParameters
        {
            result.Error("lsbParams является null");
            return result;
        }

        // Замена параметров на переданные
        var oldLsbParams = Params;
        Params = lsbParams;

        result = HideAlgorithm(data, newImagePath);
        Params = oldLsbParams;  // Возврат параметров
        return result;
    }

    // Логика метода с текущими параметрами
    private LsbHideResult HideAlgorithm(string? data, string? newImagePath)
    { 
        LsbHideResult result = new();  // Результаты скрытия
        result.Log($"Запущен процесс скрытия для {Params.Image.ImgName}");

        // Начальные проверки
        if (data is not null)
            Params.Data = data;

        if (Params.Data.Length == 0)
        {
            result.Error("Отсутствуют данные для скрытия");
            return result;
        }
        result.Log($"Объём скрываемых данных: {Params.Data.Length} символов");

        // Доопределение параметров скрытия
        bool isRandomHiding = Params.Seed is not null;  // Вид скрытия: последовательный или псевдослучайный
        int containerVolume = GetContainerVolume();  // Объём контейнера с учётом числа НЗБ
        int hidingVolume = GetHidingVolume(containerVolume, Params.DataBitLength);  // Реальный объём скрытия
        double relativeHidingVolume = hidingVolume / containerVolume;  // Доля заполнения объёма контейнера
        int usingColorBytesNum = Params.GetNeededColorBytesNum();  // Количество цветовых байт, нужных для скрытия
        result.Log($"Установлены параметры:\n\t" +
            $"isRandomHiding = {isRandomHiding}\n\tcontainerVolume = {containerVolume}\n\t" +
            $"hidingVolume = {hidingVolume}\n\trelativeHidingVolume = {relativeHidingVolume}\n\t" +
            $"usingColorBytesNum = {usingColorBytesNum}");

        // Выбор типа итерации в зависимости от метода скрытия (последовательное / псевдослучайное)
        Func<int, LsbParameters, IEnumerable<ScPointCoords>> iterator 
            = isRandomHiding ? LsbCommon.GetForRandomAccessIndex : LsbCommon.GetForLinearAccessIndex;

        // Осуществление скрытия
        result.Log("Запущена процедура скрытия");

        var colorBytesIndexes = new List<ScPointCoords>();
        if (isRandomHiding)
            foreach (var colorByteIndexes in iterator(usingColorBytesNum, Params))
                colorBytesIndexes.Add(colorByteIndexes);
        else
            colorBytesIndexes = iterator(usingColorBytesNum, Params).Take(usingColorBytesNum).ToList();

        int basketSize = 512;  // Цветовых байт в пачке
        int basketsCount = colorBytesIndexes.Count() / basketSize;  // Оставшиеся будут добавлены в последнюю пачку
        var basketsTasks = new Task[basketsCount];
        for (int i = 0; i < basketsCount; i++)
        {
            int basketIndex = i;
            basketsTasks[i] = new Task(() =>
            {
                int lastIndexInBasket = basketIndex < basketsCount - 1 ? basketSize * (basketIndex + 1) : colorBytesIndexes.Count();
                var colorBytesIndexesForBasket = colorBytesIndexes.Take((basketSize * basketIndex)..lastIndexInBasket);
                int k = (basketSize * basketIndex) * Params.LsbNum;

                foreach (var colorByteIndexes in colorBytesIndexesForBasket)
                {
                    // Если у нас больше 1 LSB используется, но осталось для сокрытия меньше бит (конец записи) - в конце будут дописаны нули
                    var bitsToHide = new BitArray(Params.LsbNum);  // Массив скрываемых в НЗБ бит, длина фиксирована как LsbNum 
                    for (int i = 0; i < Params.LsbNum && (k + i) < Params.DataBitArray.Length; i++)
                        bitsToHide[i] = Params.DataBitArray[k + i];
                    HideDataBitToColorByte(colorByteIndexes, bitsToHide);  // Скрытие в байте цвета

                    k += Params.LsbNum;
                }
            });
        }

        foreach (var basketTask in basketsTasks)
            basketTask.Start();
        foreach (var basketTask in basketsTasks)
            basketTask.Wait();

        result.Log("Завершён цикл скрытия");

        // Сохранение изображения со внедрённой информацией
        if (string.IsNullOrEmpty(newImagePath))
        {
            string newImageName = Params.Image.ImgName + "_lsb" + (isRandomHiding ? "_rnd" : "_lin");
            result.Path = Params.Image.SaveNear(newImageName);
        }
        else
        {
            result.Path = Params.Image.Save(newImagePath);
        }

        result.Log($"Изображение сохранено как {result.Path}");

        result.Log($"Процесс скрытия завершён");
        return result;
    }

    /// <summary>
    /// Скрытие бита информации в пиксель по переданным координатам
    /// </summary>
    /// <param name="inds">Индексы пикселя (и канала)</param>
    /// <param name="bits">Скрываемый бит</param>
    private void HideDataBitToColorByte(ScPointCoords inds, BitArray bits)
    {
        lock (hidingLock)
        {
            var imgArray = Params.Image.ImgArray;  // Рабочий массив пикселей изображения
            var colorByte = imgArray[inds.Y, inds.X, inds.ChannelId];
            colorByte = PixelsTools.SetLsbValues(colorByte, bits);  // Установка значений
            imgArray[inds.Y, inds.X, inds.ChannelId] = colorByte;
        }
    }

    private object hidingLock = new object();
}
