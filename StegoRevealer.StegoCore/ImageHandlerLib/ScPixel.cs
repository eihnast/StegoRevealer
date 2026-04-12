using SkiaSharp;

namespace StegoRevealer.StegoCore.ImageHandlerLib;

/// <summary>
/// Класс, представляющий пиксель изображения
/// </summary>
public struct ScPixel
{
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
    public byte Alpha { get; set; }

    public int Length => 4;

    public ScPixel(byte red, byte green, byte blue, byte alpha = 255)
    {
        Red = red; Green = green; Blue = blue; Alpha = alpha;
    }

    public ScPixel(SKColor c)
    {
        Red = c.Red; Green = c.Green; Blue = c.Blue; Alpha = c.Alpha;
    }

    public byte this[int i]
    {
        get => i switch { 0 => Red, 1 => Green, 2 => Blue, 3 => Alpha, _ => throw new IndexOutOfRangeException() };
        set
        {
            switch (i)
            {
                case 0: Red = value; break;
                case 1: Green = value; break;
                case 2: Blue = value; break;
                case 3: Alpha = value; break;
                default: throw new IndexOutOfRangeException();
            }
        }
    }

    public static ScPixel FromSkColor(SKColor color) => new ScPixel(color);
    public SKColor ToSkColor() => new SKColor(Red, Green, Blue, Alpha);
}
