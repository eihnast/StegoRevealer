using SkiaSharp;

namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    /// <summary>
    /// Класс, представляющий пиксель изображения
    /// </summary>
    public struct ScPixel
    {
        // Значения цветов по каналам
        private byte[] _pixelArray = new byte[] { 0, 0, 0, 0 };

        /// <summary>
        /// Размер массива значений пикселя (число каналов)
        /// </summary>
        public int Length { get { return _pixelArray.Length; } }

        /// <summary>
        /// Значение красного канала
        /// </summary>
        public byte Red 
        { 
            get { return _pixelArray[0]; } 
            set { _pixelArray[0] = value; }
        }

        /// <summary>
        /// Значение зелёного канала
        /// </summary>
        public byte Green
        { 
            get { return _pixelArray[1]; } 
            set { _pixelArray[1] = value; }
        }

        /// <summary>
        /// Значение синего канала
        /// </summary>
        public byte Blue
        { 
            get { return _pixelArray[2]; } 
            set { _pixelArray[2] = value; }
        }

        /// <summary>
        /// Значение альфа-канала
        /// </summary>
        public byte Alpha
        { 
            get { return _pixelArray[3]; } 
            set { _pixelArray[3] = value; }
        }


        public ScPixel() => _pixelArray = new byte[] { 0, 0, 0, 0 };

        public ScPixel(byte red, byte green, byte blue, byte alpha = 255) : this()
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public ScPixel(SKColor color) : this()
        {
            Red = color.Red;
            Green = color.Green;
            Blue = color.Blue;
            Alpha = color.Alpha;
        }


        // Доступ по индексаторам

        public byte this[int i]
        {
            get
            {
                return _pixelArray[i];
            }
            set
            {
                _pixelArray[i] = value;
            }
        }


        /// <summary>
        /// Преобразование SKColor в ScPixel
        /// </summary>
        public static ScPixel FromSkColor(SKColor color)
        {
            return new ScPixel(color);
        }

        /// <summary>
        /// Преобразование ScPixel в SKColor
        /// </summary>
        public SKColor ToSkColor()
        {
            return new SKColor(Red, Green, Blue, Alpha);
        }
    }
}
