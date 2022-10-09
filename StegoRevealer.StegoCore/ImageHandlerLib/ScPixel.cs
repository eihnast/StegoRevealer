using SkiaSharp;

namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    public struct ScPixel
    {
        private byte[] _pixelArray = new byte[] { 0, 0, 0, 0 };
        public int Length { get { return _pixelArray.Length; } }


        public byte Red 
        { 
            get { return _pixelArray[0]; } 
            set { _pixelArray[0] = value; }
        }
        public byte Green
        { 
            get { return _pixelArray[1]; } 
            set { _pixelArray[1] = value; }
        }
        public byte Blue
        { 
            get { return _pixelArray[2]; } 
            set { _pixelArray[2] = value; }
        }
        public byte Alpha
        { 
            get { return _pixelArray[3]; } 
            set { _pixelArray[3] = value; }
        }

        public ScPixel(byte red, byte green, byte blue, byte alpha = 255)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public ScPixel(SKColor color)
        {
            Red = color.Red;
            Green = color.Green;
            Blue = color.Blue;
            Alpha = color.Alpha;
        }

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

        public static ScPixel FromSkColor(SKColor color)
        {
            return new ScPixel(color);
        }

        public SKColor ToSkColor()
        {
            return new SKColor(Red, Green, Blue, Alpha);
        }
    }
}
