using System.Collections;
using Accord.Math.Geometry;
using System.Data.Common;
using System.Threading.Channels;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.StegoCore.StegoMethods.Lsb
{
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
        public IHideResult Hide(string? data)
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
            result.Log("Запущен цикл скрытия");
            int k = 0;  // Индекс бита данных
            foreach (var blockCoords in iterator(usingColorBytesNum, Params))
            {
                (int line, int column, int channel) = blockCoords.AsTuple();
                if (k >= Params.DataBitLength)
                    break;

                BitArray bitsToHide = new BitArray(Params.LsbNum);  // Массив скрываемых в НЗБ бит
                for (int i = 0; i < Params.LsbNum; i++)
                    bitsToHide[i] = Params.DataBitArray[k + i];
                HideDataBitToColorByte(new ScPointCoords(line, column, channel), bitsToHide);  // Скрытие в байте цвета
                k += Params.LsbNum;
            }
            result.Log("Завершён цикл скрытия");

            // Сохранение изображения со внедрённой информацией
            string newImgName = Params.Image.ImgName + "_lsb"
                + (isRandomHiding ? "_rnd" : "_lin");
            result.Path = Params.Image.Save(newImgName);
            result.Log($"Изображение сохранено как {newImgName}");

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
            var imgArray = Params.Image.ImgArray;  // Рабочий массив пикселей изображения
            var colorByte = imgArray[inds.Y, inds.X, inds.ChannelId];
            colorByte = PixelsTools.SetLsbValues(colorByte, bits);  // Установка значений
            imgArray[inds.Y, inds.X, inds.ChannelId] = colorByte;
        }
    }
}
