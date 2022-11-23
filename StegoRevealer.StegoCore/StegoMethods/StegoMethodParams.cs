using System.Collections;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.StegoMethods
{
    public abstract class StegoMethodParams
    {
        public ImageHandler Image { get; }  // Изображение

        public abstract bool HidingOperation { get; set; }

        public abstract int? Seed { get; set; }  // Сид для ГПСЧ при псевдослучайном скрытии (определяет тип скрытия)

        public abstract string Data { get; set; }  // Скрываемая информация

        public abstract BitArray DataBitArray { get; }
        public abstract int DataBitLength { get; }


        // Количество извлекаемых бит информации и цветовых байт изображения
        public abstract int ToExtractBitLength { get; set; }  // Количество извлекаемых бит
        public abstract int ToExtractColorBytesNum { get; }


        public abstract UniqueList<ImgChannel> Channels { get; }  // Каналы, используемые для скрытия

        public abstract bool InterlaceChannels { get; set; }  // Чередовать ли каналы при скрытии (иначе - поканально)

        public abstract bool VerticalHiding { get; set; }  // Вести скрытие по вертикалям (столбцам пикселей, а не линиям)

        public abstract StartValues StartPoints { get; set; }  // Начальные индексы скрытия (пиксели/блоки) в разных каналах

        // public abstract int LsbNum { get; set; }  // Количество НЗБ-пикселей для скрытия


        public StegoMethodParams(ImageHandler imgHandler)
        {
            Image = imgHandler;
        }

        // Сбрасывает параметры к стандартным
        public abstract void Reset();

        // Количество цветовых байт, которое необходимо для сокрытия (извлечения) всей информации
        public abstract int GetNeededToHideColorBytesNum(int? dataBitLength = null);
    }
}
