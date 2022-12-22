using System.Collections;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.StegoMethods
{
    /// <summary>
    /// Базовое представление параметров стеганографического метода
    /// </summary>
    public abstract class StegoMethodParams
    {
        /// <summary>
        /// Изображение
        /// </summary>
        public ImageHandler Image { get; }

        /// <summary>
        /// Является ли текущая операция скрытием (да - скрытие, нет - извлечение)
        /// </summary>
        public abstract StegoOperationType StegoOperation { get; set; }

        /// <summary>
        /// Ключ для ГПСЧ при псевдослучайном скрытии (определяет тип скрытия)<br/>
        /// Если отлично от null - скрытие будет производиться псевдослучайно
        /// </summary>
        public abstract int? Seed { get; set; }


        /// <summary>
        /// Скрываемая информация
        /// </summary>
        public abstract string Data { get; set; }

        /// <summary>
        /// Скрываемая информация в виде массива бит
        /// </summary>
        public abstract BitArray DataBitArray { get; }

        /// <summary>
        /// Битовая длина скрываемого сообщения
        /// </summary>
        public abstract int DataBitLength { get; }


        /// <summary>
        /// Количество извлекаемых бит информации
        /// </summary>
        public abstract int ToExtractBitLength { get; set; }

        /// <summary>
        /// Количество извлекаемых цветовых байт изображения
        /// </summary>
        public abstract int ToExtractColorBytesNum { get; }


        /// <summary>
        /// Каналы, используемые для скрытия
        /// </summary>
        public abstract UniqueList<ImgChannel> Channels { get; }

        /// <summary>
        /// Чередовать ли каналы при скрытии (иначе - поканально)
        /// </summary>
        public abstract bool InterlaceChannels { get; set; }

        /// <summary>
        /// Вести скрытие по вертикалям (столбцам пикселей, а не линиям)
        /// </summary>
        public abstract TraverseType TraverseType { get; set; }

        /// <summary>
        /// Начальные индексы скрытия (пиксели/блоки) в разных каналах
        /// </summary>
        public abstract StartValues StartPoints { get; set; }


        public StegoMethodParams(ImageHandler imgHandler)
        {
            Image = imgHandler;
        }


        /// <summary>
        /// Сбрасывает параметры к стандартным
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Количество цветовых байт, которое необходимо для сокрытия/извлечения всей информации
        /// </summary>
        public abstract int GetNeededColorBytesNum(int? dataBitLength = null);
    }
}
