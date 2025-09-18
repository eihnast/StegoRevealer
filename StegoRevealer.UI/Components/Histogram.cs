using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace StegoRevealer.UI.Components;

public class Histogram : Control
{
    public static readonly StyledProperty<IList<double>?> ValuesProperty =
        AvaloniaProperty.Register<Histogram, IList<double>?>(nameof(Values));

    public static readonly StyledProperty<int> BinCountProperty =
        AvaloniaProperty.Register<Histogram, int>(nameof(BinCount), 0); // оставлен на будущее

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

    // ---------- Зум: текущее окно индексов и история ----------
    private int _viewStart = 0;     // включительно
    private int _viewEnd = -1;      // включительно; -1 = весь массив
    private readonly Stack<(int start, int end)> _zoomHistory = new();

    // ---------- Прямоугольник области построения ----------
    private Rect _lastPlotRect;

    // ---------- Выделение мышью ----------
    private bool _isDragging;
    private Point _dragStart;
    private Point _dragCurrent;

    // ---------- Наведение мышью (направляющие) ----------
    private Point? _hoverPoint; // пиксельные координаты курсора внутри контрола; null если вне области графика

    // ---------- КЭШИ кистей/перьев/шрифтов ----------
    private readonly Typeface _typeface = new Typeface("Segoe UI");
    private readonly IBrush _bg = Brushes.White;
    private readonly Pen _axisPen = new Pen(Brushes.Black, 1);
    private readonly Pen _zeroPen = new Pen(Brushes.Gray, 1) { DashStyle = new DashStyle(new double[] { 3, 3 }, 0) };
    private readonly IBrush _barFill = new SolidColorBrush(Color.FromArgb(180, 70, 130, 180));
    private readonly Pen _barStroke = new Pen(Brushes.Black, 1);
    private readonly IBrush _selFill = new SolidColorBrush(Color.FromArgb(60, 30, 144, 255));
    private readonly Pen _selPen = new Pen(Brushes.DodgerBlue, 1);
    private readonly Pen _guidePen = new Pen(Brushes.DimGray, 1) { DashStyle = new DashStyle(new double[] { 4, 4 }, 0) };

    static Histogram()
    {
        AffectsRender<Histogram>(ValuesProperty, BinCountProperty, PlotPaddingProperty);
        FocusableProperty.OverrideDefaultValue<Histogram>(true);
    }

    public Histogram()
    {
        ClipToBounds = true;
    }

    // Публичный метод для кнопки «Сброс»
    public void ResetZoom()
    {
        _viewStart = 0;
        _viewEnd = -1;
        _zoomHistory.Clear();
        InvalidateVisual();
    }

    // Необязательное «шаг назад»
    public bool ZoomBack()
    {
        if (_zoomHistory.Count == 0) return false;
        var prev = _zoomHistory.Pop();
        _viewStart = prev.start;
        _viewEnd = prev.end;
        InvalidateVisual();
        return true;
    }

    // ----------------------- Отрисовка -----------------------
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var rect = new Rect(Bounds.Size);
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        context.FillRectangle(_bg, rect);

        var vals = Values?.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToArray();
        if (vals == null || vals.Length == 0)
            return;

        // Текущее окно по X (индексы)
        int n = vals.Length;
        int start = _viewStart;
        int end = _viewEnd < 0 ? n - 1 : Math.Clamp(_viewEnd, 0, n - 1);
        start = Math.Clamp(start, 0, end);
        int count = Math.Max(1, end - start + 1);

        // Диапазон Y с учётом 0
        double minVal = double.PositiveInfinity;
        double maxVal = double.NegativeInfinity;
        for (int i = start; i <= end; i++)
        {
            double v = vals[i];
            if (v < minVal) minVal = v;
            if (v > maxVal) maxVal = v;
        }
        double yMin = Math.Min(0, minVal);
        double yMax = Math.Max(0, maxVal);
        if (Math.Abs(yMax - yMin) < 1e-12) yMax = yMin + 1;

        // Область построения
        var plot = new Rect(
            rect.X + PlotPadding.Left,
            rect.Y + PlotPadding.Top,
            rect.Width - PlotPadding.Left - PlotPadding.Right,
            rect.Height - PlotPadding.Top - PlotPadding.Bottom);

        _lastPlotRect = plot; // для событий мыши

        // Оси
        context.DrawLine(_axisPen, new Point(plot.Left, plot.Bottom), new Point(plot.Right, plot.Bottom)); // X
        context.DrawLine(_axisPen, new Point(plot.Left, plot.Bottom), new Point(plot.Left, plot.Top));     // Y

        // Преобразования
        double ValueToY(double v) => plot.Bottom - (v - yMin) / (yMax - yMin) * plot.Height;
        double IndexToX(int idx) => plot.Left + (idx - start) * (plot.Width / count);

        // Нулевая линия
        var y0 = ValueToY(0);
        if (yMin < 0 && 0 < yMax)
            context.DrawLine(_zeroPen, new Point(plot.Left, y0), new Point(plot.Right, y0));

        // -------- LOD: если столбцов больше, чем пикселей по ширине, рисуем не больше 1 столбика на пиксель --------
        bool useLod = count > plot.Width;
        if (useLod)
        {
            int pixelCols = (int)Math.Ceiling(plot.Width);
            if (pixelCols > 0)
            {
                for (int col = 0; col < pixelCols; col++)
                {
                    int idx = start + (int)((col / plot.Width) * count);
                    if (idx > end) idx = end;

                    double v = vals[idx];
                    double y = ValueToY(v);
                    double top = Math.Min(y, y0);
                    double height = Math.Abs(y0 - y);

                    var bar = new Rect(
                        plot.Left + col + 0.5,
                        top,
                        1.0, // ширина 1 пиксель
                        Math.Max(1.0, height)
                    );

                    context.FillRectangle(_barFill, bar);
                }
            }

            // Подписи, направляющие и прямоугольник выделения
            DrawLabelsGuidesSelection(context, plot, start, end, yMin, yMax, y0, ValueToY);
            return; // LOD-ветка завершена
        }

        // -------- Обычная «толстая» отрисовка столбиков --------
        double pxBarW = plot.Width / count;
        double gap = Math.Min(2.0, pxBarW * 0.1);
        double barW = Math.Max(1.0, pxBarW - gap);

        for (int i = start; i <= end; i++)
        {
            double v = vals[i];
            double y = ValueToY(v);
            double top = Math.Min(y, y0);
            double height = Math.Abs(y0 - y);

            var bar = new Rect(
                IndexToX(i) + gap / 2.0 + 0.5,
                top,
                barW,
                Math.Max(1.0, height)
            );

            context.FillRectangle(_barFill, bar);
            if (barW >= 3.0)
                context.DrawRectangle(_barStroke, bar);
        }

        // Подписи, направляющие и прямоугольник выделения
        DrawLabelsGuidesSelection(context, plot, start, end, yMin, yMax, y0, ValueToY);
    }

    // Подписи, направляющие (hover) и выделение (drag)
    private void DrawLabelsGuidesSelection(
        DrawingContext context,
        Rect plot,
        int start,
        int end,
        double yMin,
        double yMax,
        double y0,
        Func<double, double> valueToY)
    {
        // --- Подписи (без измерений — «карманы») ---
        double fontSize = 12;
        double indent = fontSize / 2.0;

        // X: левый индекс
        var ftXLeft = new FormattedText(start.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, fontSize, Brushes.Black);
        context.DrawText(ftXLeft, new Point(plot.Left, plot.Bottom + 2));

        // X: правый индекс
        string ftXRightStr = end.ToString();
        var ftXRight = new FormattedText(ftXRightStr, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, fontSize, Brushes.Black);
        context.DrawText(ftXRight, new Point(plot.Right - ftXRightStr.Length * indent, plot.Bottom + 2));

        // Y: min
        string ftYMinStr = yMin.ToString("G4");
        var ftYMin = new FormattedText(ftYMinStr, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, fontSize, Brushes.Black);
        context.DrawText(ftYMin, new Point(plot.Left - 4 - ftYMinStr.Length * indent, plot.Bottom - fontSize));

        // Y: 0 (если попадает)
        if (yMin < 0 && 0 < yMax)
        {
            const string ftY0Str = "0";
            var ftY0 = new FormattedText(ftY0Str, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, fontSize, Brushes.Black);
            context.DrawText(ftY0, new Point(plot.Left - 4 - 2 * indent, y0 - indent));
        }

        // Y: max
        string ftYMaxStr = yMax.ToString("G4");
        var ftYMax = new FormattedText(ftYMaxStr, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, fontSize, Brushes.Black);
        context.DrawText(ftYMax, new Point(plot.Left - 4 - ftYMaxStr.Length * indent, plot.Top - indent));

        // --- «Прицельные» направляющие и подписи у осей при наведении ---
        if (_hoverPoint is Point hp && plot.Contains(hp))
        {
            // Горизонтальная (параллельно X) — от оси Y до курсора
            var yH = Math.Clamp(hp.Y, plot.Top, plot.Bottom);
            context.DrawLine(_guidePen, new Point(plot.Left, yH), new Point(Math.Clamp(hp.X, plot.Left, plot.Right), yH));

            // Вертикальная (параллельно Y) — от оси X (нулевая линия, при необходимости клэмпим в рамки) до курсора
            double yBaseClamped = Math.Clamp(y0, plot.Top, plot.Bottom);
            var xV = Math.Clamp(hp.X, plot.Left, plot.Right);
            context.DrawLine(_guidePen, new Point(xV, yBaseClamped), new Point(xV, yH));

            // Значения у осей напротив пересечений
            // 1) Подпись индекса на оси X
            // Текущее окно: start..end, count = end-start+1
            double rel = (xV - plot.Left) / Math.Max(1.0, plot.Width);
            double idxD = start + rel * Math.Max(1, end - start + 1);
            int idxNearest = Math.Clamp((int)Math.Round(idxD), start, end);

            // Рисуем прямо под осью X около xV (немного смещение, чтобы не пересекаться с осью)
            var idxStr = idxNearest.ToString();
            var ftIdx = new FormattedText(idxStr, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, fontSize, Brushes.Black);

            context.DrawText(ftIdx, new Point(
                Math.Clamp(xV - idxStr.Length * indent * 0.5, plot.Left, plot.Right - 30),
                plot.Bottom + 2));

            // 2) Подпись значения на оси Y
            double val = yMin + (plot.Bottom - yH) / Math.Max(1.0, plot.Height) * (yMax - yMin);
            string valStr = val.ToString("G5");
            var ftVal = new FormattedText(valStr, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, fontSize, Brushes.Black);
            // Рисуем слева от оси Y на уровне yH
            context.DrawText(ftVal, new Point(plot.Left - 4 - valStr.Length * indent, yH - indent));
        }

        // --- Прямоугольник выделения при drag ---
        if (_isDragging)
        {
            var sel = GetSelectionRectClamped();
            if (sel.Width > 0)
            {
                context.FillRectangle(_selFill, sel);
                context.DrawRectangle(_selPen, sel);
            }
        }
    }

    // ----------------------- Обработка мыши -----------------------
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (Values == null || Values.Count == 0) return;

        var p = e.GetPosition(this);
        // Начало выделения — только если внутри графика
        if (!_lastPlotRect.Contains(p)) return;

        _isDragging = true;
        _dragStart = _dragCurrent = p;
        e.Pointer.Capture(this);
        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var p = e.GetPosition(this);

        // Обновляем «ховер» всегда (даже без драга), но инвалидируем только при реальном изменении
        Point? newHover = _lastPlotRect.Contains(p) ? p : (Point?)null;
        if (_hoverPoint != newHover)
        {
            _hoverPoint = newHover;
            InvalidateVisual();
        }

        if (_isDragging)
        {
            _dragCurrent = p;
            InvalidateVisual();
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (_hoverPoint != null)
        {
            _hoverPoint = null;
            InvalidateVisual();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_isDragging) return;

        _isDragging = false;
        e.Pointer.Capture(null);

        var sel = GetSelectionRectClamped();
        // Порог — хотя бы 5 px по ширине
        if (sel.Width >= 5 && Values != null && Values.Count > 1)
        {
            int n = Values.Count;
            int curStart = _viewStart;
            int curEnd = _viewEnd < 0 ? n - 1 : _viewEnd;
            int curCount = Math.Max(1, curEnd - curStart + 1);

            // x->index
            double xToIndex(double x) =>
                curStart + (x - _lastPlotRect.Left) / Math.Max(1.0, _lastPlotRect.Width) * curCount;

            int newStart = (int)Math.Floor(xToIndex(sel.Left));
            int newEnd = (int)Math.Ceiling(xToIndex(sel.Right)) - 1;

            newStart = Math.Clamp(newStart, curStart, curEnd);
            newEnd = Math.Clamp(newEnd, newStart, curEnd);

            // Сохраняем текущее окно в историю и применяем новое
            _zoomHistory.Push((curStart, curEnd));
            _viewStart = newStart;
            _viewEnd = newEnd;
            InvalidateVisual();
        }
        else
        {
            InvalidateVisual(); // просто скрыть прямоугольник
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (_isDragging)
        {
            _isDragging = false;
            InvalidateVisual();
        }
    }

    private Rect GetSelectionRectClamped()
    {
        // Ограничиваем выделение рамками области построения по X
        double x1 = Math.Clamp(_dragStart.X, _lastPlotRect.Left, _lastPlotRect.Right);
        double x2 = Math.Clamp(_dragCurrent.X, _lastPlotRect.Left, _lastPlotRect.Right);
        double left = Math.Min(x1, x2);
        double right = Math.Max(x1, x2);
        return new Rect(left, _lastPlotRect.Top, Math.Max(0, right - left), _lastPlotRect.Height);
    }
}
