using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SkiaSharp;

namespace StegoRevealer.UI.Components;

public class RsCsaGraph : Control
{
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var width = Bounds.Width;
        var height = Bounds.Height;

        using var skSurface = SKSurface.Create(new SKImageInfo((int)width, (int)height));
        var canvas = skSurface.Canvas;
        canvas.Clear(SKColors.White);

        float margin = 60;
        float plotWidth = (float)width - 2 * margin;
        float plotHeight = (float)height - 2 * margin;

        float MapX(double x) => (float)(x / 100.0 * plotWidth) + margin;
        float MapY(double y) => (float)(y / 100.0 * plotHeight) + margin;

        void DrawZone(SKPoint[] points, SKColor color, string label, float textMarginY = 0)
        {
            using var path = new SKPath();
            path.MoveTo(points[0]);
            for (int i = 1; i < points.Length; i++)
                path.LineTo(points[i]);
            path.Close();

            // Заливка
            canvas.DrawPath(path, new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            });

            // Контур
            canvas.DrawPath(path, new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
                IsAntialias = true
            });

            // Подпись — по центру тяжести (центроид)
            float cx = 0, cy = 0;
            foreach (var p in points)
            {
                cx += p.X;
                cy += p.Y;
            }
            cx /= points.Length;
            cy /= points.Length + textMarginY;

            canvas.DrawText(label, cx, cy, new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 16,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            });
        }

        // Пунктирные линии
        void DashedLine(float x1, float y1, float x2, float y2)
        {
            var paint = new SKPaint
            {
                Color = SKColors.Black,
                PathEffect = SKPathEffect.CreateDash(new float[] { 8, 8 }, 0),
                StrokeWidth = 2,
                Style = SKPaintStyle.Stroke
            };
            canvas.DrawLine(x1, y1, x2, y2, paint);
        }

        // Заливка схемы
        var o0 = new SKPoint[] { new SKPoint(MapX(0), MapY(0)), new SKPoint(MapX(100), MapY(0)), new SKPoint(MapX(100), MapY(100)), new SKPoint(MapX(0), MapY(100)) };
        DrawZone(o0, SKColors.LightGray, "");

        // Пунктирные линии
        DashedLine(MapX(30), MapY(4), MapX(100), MapY(4));
        DashedLine(MapX(0), MapY(30), MapX(100), MapY(30));
        DashedLine(MapX(0), MapY(80), MapX(100), MapY(80));
        DashedLine(MapX(95), MapY(0), MapX(95), MapY(100));

        // Зоны. X - CSA, Y - RS.
        var o1Real = new SKPoint[] { new SKPoint(MapX(0), MapY(0)), new SKPoint(MapX(0.1), MapY(0)), new SKPoint(MapX(0.1), MapY(4)), new SKPoint(MapX(0), MapY(4)) };
        var o1Virtual = new SKPoint[] { new SKPoint(MapX(0), MapY(0)), new SKPoint(MapX(8), MapY(0)), new SKPoint(MapX(8), MapY(13)), new SKPoint(MapX(0), MapY(13)) };
        var o2 = new SKPoint[] { new SKPoint(MapX(0), MapY(0)), new SKPoint(MapX(0.1), MapY(0)), new SKPoint(MapX(10), MapY(30)), new SKPoint(MapX(0), MapY(30)) };
        var o3 = new SKPoint[] { new SKPoint(MapX(0.1), MapY(0)), new SKPoint(MapX(30), MapY(0)), new SKPoint(MapX(30), MapY(30)) };
        var o4 = new SKPoint[] { new SKPoint(MapX(0), MapY(30)), new SKPoint(MapX(30), MapY(30)), new SKPoint(MapX(30), MapY(80)), new SKPoint(MapX(0), MapY(80)) };
        var o5 = new SKPoint[] { new SKPoint(MapX(0), MapY(80)), new SKPoint(MapX(30), MapY(80)), new SKPoint(MapX(30), MapY(100)), new SKPoint(MapX(0), MapY(100)) };
        var o6 = new SKPoint[] { new SKPoint(MapX(95), MapY(0)), new SKPoint(MapX(100), MapY(0)), new SKPoint(MapX(100), MapY(30)), new SKPoint(MapX(95), MapY(30)) };
        var o7 = new SKPoint[] { new SKPoint(MapX(95), MapY(80)), new SKPoint(MapX(100), MapY(80)), new SKPoint(MapX(100), MapY(100)), new SKPoint(MapX(95), MapY(100)) };

        DrawZone(o2, SKColors.LightBlue, "O.2");
        DrawZone(o3, SKColors.LightGreen, "O.3");
        DrawZone(o4, SKColors.LightBlue, "O.4");
        DrawZone(o5, SKColors.LightBlue, "O.5");
        DrawZone(o6, SKColors.LightGreen, "O.6");
        DrawZone(o7, SKColors.LightBlue, "O.7");
        DrawZone(o1Real, SKColors.White, "O.1", MapY(1));

        // Подписи осей
        var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 20, IsAntialias = true };
        canvas.DrawText("CSA (%)", margin + plotWidth / 2, (float)height - 10, textPaint);
        canvas.Save();
        canvas.RotateDegrees(-90, 15, margin + plotHeight / 2);
        canvas.DrawText("RS (%)", 15, margin + plotHeight / 2, textPaint);
        canvas.Restore();

        // Вставка на Avalonia DrawingContext
        using var snapshot = skSurface.Snapshot();
        using var skiaImage = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = skiaImage.AsStream();
        var bitmap = new Avalonia.Media.Imaging.Bitmap(stream);
        var destRect = new Rect(0, 0, Bounds.Width, Bounds.Height);
        var sourceRect = new Rect(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height);
        context.DrawImage(bitmap, sourceRect, destRect);
    }
}
