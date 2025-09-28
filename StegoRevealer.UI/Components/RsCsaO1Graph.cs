using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SkiaSharp;
using StegoRevealer.UI.Tools;

namespace StegoRevealer.UI.Components;

public class RsCsaO1Graph : Control
{
    private static SolidColorBrush BackgroundBrush = CommonTools.GetBrush("SrDark");

    private double? _rsValue = null;
    private double? _csaValue = null;

    public void SetPoint(double rs, double csa)
    {
        _rsValue = rs;
        _csaValue = csa;
        InvalidateVisual();  // Перерисовать компонент
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var width = Bounds.Width;
        var height = Bounds.Height;

        using var skSurface = SKSurface.Create(new SKImageInfo((int)width, (int)height));
        if (skSurface is null)
            return;

        var canvas = skSurface.Canvas;
        canvas.Clear(CommonTools.MapToSkiaColor(BackgroundBrush.Color));

        float margin = 30;
        float plotWidth = (float)width - 2 * margin;
        float plotHeight = (float)height - 2 * margin;

        float MapX(double x) => (float)(x / 1 * plotWidth) + margin;
        float MapY(double y) => (float)(y / 5 * plotHeight) + margin;

        void DrawZone(SKPoint[] points, SKColor color, string label, float textMarginY = 0, SKColor? textColor = null, bool drawCircuit = true)
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
            if (drawCircuit)
            {
                canvas.DrawPath(path, new SKPaint
                {
                    Color = SKColors.Black,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1.5f,
                    IsAntialias = true
                });
            }

            // Подпись — по центру тяжести (центроид)
            float cx = 0, cy = 0;
            foreach (var p in points)
            {
                cx += p.X;
                cy += p.Y;
            }
            cx /= points.Length;
            cy /= points.Length;

            // Подпись
            canvas.DrawText(label, cx, cy + textMarginY, new SKPaint
            {
                Color = textColor ?? SKColors.Black,
                TextSize = 16,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            });
        }

        // Заливка схемы
        var o0 = new SKPoint[] { new SKPoint(MapX(0), MapY(0)), new SKPoint(MapX(1), MapY(0)), new SKPoint(MapX(1), MapY(5)), new SKPoint(MapX(0), MapY(5)) };
        DrawZone(o0, SKColors.LightGray, "");

        // Зоны. X - CSA, Y - RS.
        var o1 = new SKPoint[] { new SKPoint(MapX(0), MapY(0)), new SKPoint(MapX(0.1), MapY(0)), new SKPoint(MapX(0.1), MapY(4)), new SKPoint(MapX(0), MapY(4)) };
        //var o2 = new SKPoint[] { new SKPoint(MapX(0), MapY(0)), new SKPoint(MapX(0.1), MapY(0)), new SKPoint(MapX(10), MapY(30)), new SKPoint(MapX(0), MapY(30)) };
        //var o3 = new SKPoint[] { new SKPoint(MapX(0.1), MapY(0)), new SKPoint(MapX(30), MapY(0)), new SKPoint(MapX(30), MapY(30)) };

        //DrawZone(o2, SKColors.LightBlue, "O.2");
        //DrawZone(o3, SKColors.LightGreen, "O.3");
        DrawZone(o1, SKColors.White, "O.1");

        // Подписи осей
        var largeTextPaint = new SKPaint { Color = SKColor.Parse("#FFE0E0E0"), TextSize = 16,
            TextAlign = SKTextAlign.Center, TextScaleX = 1.5f, FakeBoldText = true, IsAntialias = true };
        var smallTextPaint = new SKPaint { Color = SKColor.Parse("#FFE0E0E0"), TextSize = 14,
            TextAlign = SKTextAlign.Center, TextScaleX = 1.0f, FakeBoldText = false, IsAntialias = true };

        canvas.DrawText("CSA (%)", margin + plotWidth / 2, margin - 10, largeTextPaint);
        canvas.Save();
        canvas.RotateDegrees(-90, margin - 10, margin + plotHeight / 2);
        canvas.DrawText("RS (%)", margin - 10, margin + plotHeight / 2, largeTextPaint);
        canvas.Restore();

        // Подписи значений на осях
        canvas.DrawText("4", margin - 10, MapY(4) + 2, smallTextPaint);
        canvas.DrawText("0.1", MapX(0.1), margin - 10, smallTextPaint);

        // Отрисовка заданной точки, если она установлена
        if (_rsValue.HasValue && _csaValue.HasValue)
        {
            float x = MapX(_csaValue.Value);
            float y = MapY(_rsValue.Value);

            // Нарисовать саму точку (красный круг)
            canvas.DrawCircle(x, y, 4, new SKPaint
            {
                Color = SKColors.Red,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            });
        }

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
