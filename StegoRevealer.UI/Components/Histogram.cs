using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StegoRevealer.UI.Components;

public class Histogram : Control
{
    public static readonly StyledProperty<IList<double>?> ValuesProperty =
        AvaloniaProperty.Register<Histogram, IList<double>?>(nameof(Values));

    public static readonly StyledProperty<int> BinCountProperty =
        AvaloniaProperty.Register<Histogram, int>(nameof(BinCount), 0); // 0 = авто

    public static readonly StyledProperty<Thickness> PlotPaddingProperty =
        AvaloniaProperty.Register<Histogram, Thickness>(nameof(PlotPadding), new Thickness(40, 20, 20, 40));

    public IList<double>? Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public int BinCount
    {
        get => GetValue(BinCountProperty);
        set => SetValue(BinCountProperty, value);
    }

    public Thickness PlotPadding
    {
        get => GetValue(PlotPaddingProperty);
        set => SetValue(PlotPaddingProperty, value);
    }

    static Histogram()
    {
        AffectsRender<Histogram>(ValuesProperty, BinCountProperty, PlotPaddingProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var rect = new Rect(Bounds.Size);
        if (rect.Width <= 0 || rect.Height <= 0) return;

        context.FillRectangle(Brushes.White, rect);

        var vals = Values?.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToArray();
        if (vals == null || vals.Length == 0) return;

        double minVal = vals.Min();
        double maxVal = vals.Max();
        double yMin = Math.Min(0, minVal);
        double yMax = Math.Max(0, maxVal);
        if (Math.Abs(yMax - yMin) < 1e-12) yMax = yMin + 1;

        var plot = new Rect(
            rect.X + PlotPadding.Left,
            rect.Y + PlotPadding.Top,
            rect.Width - PlotPadding.Left - PlotPadding.Right,
            rect.Height - PlotPadding.Top - PlotPadding.Bottom);

        var penAxis = new Pen(Brushes.Black, 1);
        context.DrawLine(penAxis, new Point(plot.Left, plot.Bottom), new Point(plot.Right, plot.Bottom)); // X
        context.DrawLine(penAxis, new Point(plot.Left, plot.Bottom), new Point(plot.Left, plot.Top));     // Y

        double ValueToY(double v) => plot.Bottom - (v - yMin) / (yMax - yMin) * plot.Height;

        if (yMin < 0 && 0 < yMax)
        {
            var y0 = ValueToY(0);
            context.DrawLine(new Pen(Brushes.Gray, 1, dashStyle: new DashStyle(new double[] { 3, 3 }, 0)),
                new Point(plot.Left, y0), new Point(plot.Right, y0));
        }

        int n = vals.Length;
        double pxBarW = plot.Width / Math.Max(1, n);
        double gap = Math.Min(2.0, pxBarW * 0.1);
        double barW = Math.Max(1.0, pxBarW - gap);

        var barFill = new SolidColorBrush(Color.FromArgb(180, 70, 130, 180));
        var barStroke = new Pen(Brushes.Black, 1);

        double yBase = ValueToY(0);

        for (int i = 0; i < n; i++)
        {
            double v = vals[i];
            double y = ValueToY(v);
            double top = Math.Min(y, yBase);
            double height = Math.Abs(yBase - y);

            var bar = new Rect(
                plot.Left + i * pxBarW + gap / 2.0 + 0.5,
                top,
                barW,
                Math.Max(1.0, height)
            );

            context.FillRectangle(barFill, bar);
            context.DrawRectangle(barStroke, bar);
        }

        double fontSize = 12;
        double indent = fontSize / 2.0;
        var typeface = new Typeface("Segoe UI");

        var ftXLeft = new FormattedText("0", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black);
        context.DrawText(ftXLeft, new Point(plot.Left, plot.Bottom + 2));

        string ftXString = (n - 1).ToString();
        var ftXRight = new FormattedText(ftXString, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black);
        context.DrawText(ftXRight, new Point(plot.Right - ftXString.Length * indent, plot.Bottom + 2));

        string ftYMinString = yMin.ToString("G4");
        var ftYMin = new FormattedText(ftYMinString, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black);
        context.DrawText(ftYMin, new Point(plot.Left - 4 - ftYMinString.Length * indent, plot.Bottom - fontSize));

        if (yMin < 0 && 0 < yMax)
        {
            string ftY0String = "0";
            var ftY0 = new FormattedText(ftY0String, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black);
            context.DrawText(ftY0, new Point(plot.Left - 4 - ftY0String.Length * indent, ValueToY(0) - indent));
        }

        string ftYMaxString = yMax.ToString("G4");
        var ftYMax = new FormattedText(ftYMaxString, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black);
        context.DrawText(ftYMax, new Point(plot.Left - 4 - ftYMaxString.Length * indent, plot.Top - indent));
    }
}
