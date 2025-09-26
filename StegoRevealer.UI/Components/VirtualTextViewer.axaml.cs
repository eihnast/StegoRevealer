using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StegoRevealer.UI.Components;

public partial class VirtualTextViewer : UserControl
{
    public static readonly StyledProperty<IBrush?> ContentBackgroundProperty =
        AvaloniaProperty.Register<VirtualTextViewer, IBrush?>(nameof(ContentBackground), Brushes.Transparent);

    public static readonly StyledProperty<Thickness> ContentPaddingProperty =
        AvaloniaProperty.Register<VirtualTextViewer, Thickness>(nameof(ContentPadding), new Thickness(6, 6, 6, 6));

    public static readonly StyledProperty<double> RightPaddingForScrollbarProperty =
        AvaloniaProperty.Register<VirtualTextViewer, double>(nameof(RightPaddingForScrollbar), 30d);

    public static readonly StyledProperty<bool> EnableSanitizationProperty =
        AvaloniaProperty.Register<VirtualTextViewer, bool>(nameof(EnableSanitization), false);

    public IBrush? ContentBackground
    {
        get => GetValue(ContentBackgroundProperty);
        set => SetValue(ContentBackgroundProperty, value);
    }
    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }
    public double RightPaddingForScrollbar
    {
        get => GetValue(RightPaddingForScrollbarProperty);
        set => SetValue(RightPaddingForScrollbarProperty, value);
    }
    public bool EnableSanitization
    {
        get => GetValue(EnableSanitizationProperty);
        set => SetValue(EnableSanitizationProperty, value);
    }

    private readonly ScrollViewer _scrollViewer;
    private readonly Border _host;
    private readonly TextCanvas _canvas;

    private string _rawText = string.Empty;
    private string _fullText = string.Empty;
    private TextLayout? _layout;

    private double _wrapWidth;
    private double _layoutWrapWidth;
    private double _contentHeight;

    private readonly List<(int start, int length)> _visualLines = new();
    private readonly List<double> _lineOffsets = new();
    private readonly List<double> _lineHeights = new();

    private int _selectionStart = -1;
    private int _selectionEnd = -1;
    private bool _isSelecting;
    private bool _selectAllMode;

    public VirtualTextViewer()
    {
        Focusable = true;

        _canvas = new TextCanvas(this);

        _host = new Border
        {
            Background = Brushes.Transparent,
            Child = _canvas,
            Padding = new Thickness(ContentPadding.Left, ContentPadding.Top,
                                    ContentPadding.Right + RightPaddingForScrollbar, ContentPadding.Bottom)
        };

        _scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            Content = _host
        };

        Content = _scrollViewer;

        global::System.ObservableExtensions.Subscribe(
            this.GetObservable(ContentBackgroundProperty),
            b => _host.Background = b);

        global::System.ObservableExtensions.Subscribe(
            this.GetObservable(ContentPaddingProperty),
            pad =>
            {
                _host.Padding = new Thickness(pad.Left, pad.Top, pad.Right + RightPaddingForScrollbar, pad.Bottom);
                RecalculateLayoutAndUpdate();
            });

        global::System.ObservableExtensions.Subscribe(
            this.GetObservable(RightPaddingForScrollbarProperty),
            r =>
            {
                var p = ContentPadding;
                _host.Padding = new Thickness(p.Left, p.Top, Math.Max(0, p.Right + r), p.Bottom);
                RecalculateLayoutAndUpdate();
            });

        global::System.ObservableExtensions.Subscribe(
            this.GetObservable(EnableSanitizationProperty),
            _ => ReflowTextOnly());

        global::System.ObservableExtensions.Subscribe(
            this.GetObservable(FontFamilyProperty),
            _ => RecalculateLayoutAndUpdate());
        global::System.ObservableExtensions.Subscribe(
            this.GetObservable(FontSizeProperty),
            _ => RecalculateLayoutAndUpdate());
        global::System.ObservableExtensions.Subscribe(
            this.GetObservable(ForegroundProperty),
            _ => RecalculateLayoutAndUpdate());

        global::System.ObservableExtensions.Subscribe(
            _scrollViewer.GetObservable(ScrollViewer.ViewportProperty),
            _ => RecalculateLayoutAndUpdate());
        global::System.ObservableExtensions.Subscribe(
            this.GetObservable(BoundsProperty),
            _ => RecalculateLayoutAndUpdate());

        _scrollViewer.ScrollChanged += (_, __) => _canvas.InvalidateVisual();

        _canvas.PointerPressed += OnPointerPressed;
        _canvas.PointerMoved += OnPointerMoved;
        _canvas.PointerReleased += OnPointerReleased;
        _canvas.PointerCaptureLost += (_, __) => _isSelecting = false;

        // Курсор над текстом
        _canvas.PointerEntered += (_, __) => _canvas.Cursor = new Cursor(StandardCursorType.Ibeam);
        _canvas.PointerExited += (_, __) => _canvas.Cursor = new Cursor(StandardCursorType.Arrow);

        // Хоткеи
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    public void SetText(string text)
    {
        _rawText = text ?? string.Empty;
        ReflowTextOnly();
    }
    public string Text => _fullText;

    public void ScrollToTop(bool clearSelection = true)
    {
        if (clearSelection)
            ClearSelection();

        try
        {
            _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, 0);
        }
        catch { }

        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, 0);
            }
            catch { }
        }, DispatcherPriority.Background);
    }

    public void SelectAll()
    {
        _selectAllMode = true;
        _selectionStart = 0;
        _selectionEnd = _fullText.Length;
        _canvas.SetSelectionRects(BuildSelectionBands());
    }
    public void ClearSelection()
    {
        _selectAllMode = false;
        _selectionStart = _selectionEnd = -1;
        _canvas.SetSelectionRects(Array.Empty<Rect>());
    }

    private static readonly HashSet<int> ZeroWidthCp = new()
    {
        0x200B,0x200C,0x200D,0x200E,0x200F, // ZWSP/ZWNJ/ZWJ/LRM/RLM
        0x2029, 0x2060, 0xFEFF,            // PS, WJ, ZWNBSP/BOM
        0xFE00,0xFE01,0xFE02,0xFE03,0xFE04,0xFE05,0xFE06,0xFE07,0xFE08,0xFE09,0xFE0A,0xFE0B,0xFE0C,0xFE0D,0xFE0E,0xFE0F,
        0x061C,                             // ARABIC LETTER MARK
        0x202A,0x202B,0x202C,0x202D,0x202E, // LRE/RLE/PDF/LRO/RLO
        0x2066,0x2067,0x2068,0x2069         // LRI/RLI/FSI/PDI
    };
    private static bool IsAllowedCtrl(char c) => c is '\r' or '\n' or '\t';

    private static string Sanitize(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var list = new List<char>(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            int cp = char.ConvertToUtf32(s, i);
            if (cp > 0xFFFF) i++;

            char c = (cp <= 0xFFFF) ? (char)cp : '\uFFFD';
            if (c == '\uFFFD') continue;

            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (char.IsControl(c) && !IsAllowedCtrl(c)) continue;
            if (ZeroWidthCp.Contains(cp)) continue;
            if ((cp & 0xFFFE) == 0xFFFE || (cp >= 0xFDD0 && cp <= 0xFDEF)) continue; // noncharacters

            list.Add(c);
        }
        return new string(list.ToArray());
    }

    private void ReflowTextOnly()
    {
        _fullText = EnableSanitization ? Sanitize(_rawText) : _rawText;

        _selectionStart = _selectionEnd = -1;
        _selectAllMode = false;

        RecalculateLayoutAndUpdate();
    }

    private Typeface CurrentTypeface => new Typeface(FontFamily, FontStyle.Normal, FontWeight.Normal);

    private double GetWrappingWidth()
    {
        double w = _host.Bounds.Width;
        if (double.IsNaN(w) || w <= 0) w = _scrollViewer.Viewport.Width;
        if (double.IsNaN(w) || w <= 0) w = Bounds.Width;
        if (double.IsNaN(w) || w <= 0) w = Width;
        if (double.IsNaN(w) || w <= 0) w = 800;

        var pad = _host.Padding;
        var bt = _host.BorderThickness;
        w -= (pad.Left + pad.Right + bt.Left + bt.Right);
        return Math.Max(1, w);
    }

    private void RecalculateLayoutAndUpdate()
    {
        _wrapWidth = GetWrappingWidth();
        _layoutWrapWidth = _wrapWidth + 10;

        if (string.IsNullOrEmpty(_fullText))
        {
            _layout = null;
            _visualLines.Clear();
            _lineOffsets.Clear();
            _lineHeights.Clear();
            _contentHeight = 0;

            _canvas.SetContext(_layout, _host.Padding, _contentHeight);
            _canvas.SetSelectionRects(Array.Empty<Rect>());
            _canvas.InvalidateMeasure();
            _canvas.InvalidateVisual();
            return;
        }

        _layout = new TextLayout(
            _fullText,
            CurrentTypeface,
            FontSize > 0 ? FontSize : 16,
            Foreground ?? Brushes.White,
            TextAlignment.Left,
            TextWrapping.Wrap,
            null, null,
            FlowDirection.LeftToRight,
            _layoutWrapWidth,
            double.PositiveInfinity);

        _visualLines.Clear();
        _lineOffsets.Clear();
        _lineHeights.Clear();

        double y = 0;
        foreach (var tl in _layout.TextLines)
        {
            if (tl.Length == 0) continue;

            _visualLines.Add((tl.FirstTextSourceIndex, tl.Length));
            _lineOffsets.Add(y);
            _lineHeights.Add(tl.Height);
            y += tl.Height;
        }

        _contentHeight = (_lineOffsets.Count > 0) ? _lineOffsets[^1] + _lineHeights[^1] : 0;

        _canvas.SetContext(_layout, _host.Padding, _contentHeight);
        _canvas.SetSelectionRects(BuildSelectionBands());

        _canvas.InvalidateMeasure();
        _canvas.InvalidateVisual();
    }

    private int GetScrollLineFromOffset(double y)
    {
        if (_lineOffsets.Count == 0) return 0;
        int lo = 0, hi = _lineOffsets.Count - 1, ans = 0;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (_lineOffsets[mid] <= y) { ans = mid; lo = mid + 1; }
            else hi = mid - 1;
        }
        return ans;
    }

    private (int a, int b) GetSelectionRange()
    {
        if (_selectAllMode) return (0, _fullText.Length);
        if (_selectionStart < 0 || _selectionEnd < 0 || _selectionStart == _selectionEnd)
            return (-1, -1);

        int a = Math.Max(0, Math.Min(_selectionStart, _selectionEnd));
        int b = Math.Min(_fullText.Length, Math.Max(_selectionStart, _selectionEnd));
        if (b <= a) return (-1, -1);
        return (a, b);
    }

    private static bool IsZeroWidthOrFormat(char c)
    {
        var cat = CharUnicodeInfo.GetUnicodeCategory(c);
        if (cat == UnicodeCategory.NonSpacingMark || cat == UnicodeCategory.Format)
            return true;

        int cp = char.ConvertToUtf32(c.ToString(), 0);
        if (cp > 0xFFFF) return true;
        if (char.IsControl(c) && c != '\r' && c != '\n' && c != '\t') return true;
        return ZeroWidthCp.Contains(cp);
    }

    private static (bool ok, double xMin, double xMax) RectUnion(IReadOnlyList<Rect> rects, double clampMin, double clampMax, double inflate)
    {
        double minX = double.PositiveInfinity;
        double maxX = double.NegativeInfinity;
        for (int i = 0; i < rects.Count; i++)
        {
            var r = rects[i];
            if (r.Width <= 0) continue;
            double x1 = Math.Max(clampMin, r.X - inflate);
            double x2 = Math.Min(clampMax, r.X + r.Width + inflate);
            if (x2 <= x1) continue;
            if (x1 < minX) minX = x1;
            if (x2 > maxX) maxX = x2;
        }
        if (!double.IsFinite(minX) || !double.IsFinite(maxX) || maxX <= minX)
            return (false, 0, 0);
        return (true, minX, maxX);
    }

    private (bool ok, double xMin, double xMax) TryGetEdgeBandForLine(int ia, int ib)
    {
        for (int i = ia; i < ib; i++)
        {
            char c = _fullText[i];
            if (IsZeroWidthOrFormat(c)) continue;

            var r1 = _layout!.HitTestTextRange(i, 1);
            var u1 = RectUnion(r1.ToList(), 0, _layoutWrapWidth, 0.25);
            if (!u1.ok) continue;

            for (int j = ib - 1; j >= ia; j--)
            {
                char c2 = _fullText[j];
                if (IsZeroWidthOrFormat(c2)) continue;

                var r2 = _layout!.HitTestTextRange(j, 1);
                var u2 = RectUnion(r2.ToList(), 0, _layoutWrapWidth, 0.25);
                if (!u2.ok) continue;

                double x1 = Math.Min(u1.xMin, u2.xMin);
                double x2 = Math.Max(u1.xMax, u2.xMax);
                if (x2 - x1 < 0.5) x2 = x1 + 0.5;
                return (true, x1, x2);
            }
        }
        return (false, 0, 0);
    }

    private IReadOnlyList<Rect> BuildSelectionBands()
    {
        if (_layout is null || string.IsNullOrEmpty(_fullText) || _visualLines.Count == 0)
            return Array.Empty<Rect>();

        var (selA, selB) = GetSelectionRange();
        if (selA < 0) return Array.Empty<Rect>();

        var bands = new List<Rect>();

        for (int li = 0; li < _visualLines.Count; li++)
        {
            var (ls, len) = _visualLines[li];
            int le = ls + len;

            int ia = Math.Max(ls, selA);
            int ib = Math.Min(le, selB);
            if (ib <= ia) continue;

            var rectsLine = _layout.HitTestTextRange(ls, len);
            var uLine = RectUnion(rectsLine.ToList(), 0, _layoutWrapWidth, 0);
            double lineMin = uLine.ok ? uLine.xMin : 0;
            double lineMax = uLine.ok ? uLine.xMax : _layoutWrapWidth;

            var edge = TryGetEdgeBandForLine(ia, ib);
            double x1, x2;
            if (edge.ok)
            {
                x1 = edge.xMin;
                x2 = edge.xMax;
            }
            else
            {
                var rectsSel = _layout.HitTestTextRange(ia, ib - ia);
                var uSel = RectUnion(rectsSel.ToList(), 0, _layoutWrapWidth, 0.25);
                if (uSel.ok)
                {
                    x1 = uSel.xMin;
                    x2 = uSel.xMax;
                }
                else
                {
                    x1 = Math.Max(0, lineMin - 0.25);
                    x2 = Math.Min(_layoutWrapWidth, lineMax + 0.25);
                }
            }

            if (ia == ls && uLine.ok)
                x1 = Math.Min(x1, Math.Max(0, lineMin - 0.25));
            if (ib == le && uLine.ok)
                x2 = Math.Max(x2, Math.Min(_layoutWrapWidth, lineMax + 0.25));

            bands.Add(new Rect(x1, _lineOffsets[li], Math.Max(0.5, x2 - x1), _lineHeights[li]));
        }

        return bands;
    }

    private int GetCharIndexFromPoint(Point pInCanvas)
    {
        if (_layout is null || string.IsNullOrEmpty(_fullText))
            return 0;

        var pad = _host.Padding;

        double yInFull = pInCanvas.Y - pad.Top;
        double xInFull = pInCanvas.X - pad.Left;

        if (xInFull < 0) xInFull = 0;
        if (xInFull > _layoutWrapWidth) xInFull = _layoutWrapWidth;

        double totalHeight = _contentHeight;
        if (yInFull < 0) yInFull = 0;
        if (yInFull > totalHeight - 0.001) yInFull = Math.Max(0, totalHeight - 0.001);

        int lineIdx = GetScrollLineFromOffset(yInFull);
        lineIdx = Math.Clamp(lineIdx, 0, Math.Max(0, _visualLines.Count - 1));

        var (lineStart, lineLen) = _visualLines[lineIdx];
        int lineEndExclusive = lineStart + lineLen;

        var hit = _layout.HitTestPoint(new Point(xInFull, yInFull));
        int index = hit.TextPosition + (hit.IsTrailing ? 1 : 0);

        if (index > lineEndExclusive) index = lineEndExclusive;
        if (index < lineStart) index = lineStart;

        return Math.Clamp(index, 0, _fullText.Length);
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Ctrl+C
        if (e.Key == Key.C && (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
        {
            string? selected = GetSelectedText();
            if (!string.IsNullOrEmpty(selected))
            {
                var tl = TopLevel.GetTopLevel(this);
                if (tl?.Clipboard != null)
                {
                    try { await tl.Clipboard.SetTextAsync(selected); } catch { /* ignore */ }
                }
                e.Handled = true;
            }
        }

        // Ctrl+A
        else if (e.Key == Key.A && (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
        {
            if (!string.IsNullOrEmpty(_fullText))
            {
                SelectAll();
                e.Handled = true;
            }
        }
    }

    private string? GetSelectedText()
    {
        if (_fullText.Length == 0) return null;
        if (_selectAllMode) return _fullText;
        if (_selectionStart < 0 || _selectionEnd < 0 || _selectionStart == _selectionEnd) return null;

        int a = Math.Max(0, Math.Min(_selectionStart, _selectionEnd));
        int b = Math.Min(_fullText.Length, Math.Max(_selectionStart, _selectionEnd));
        return (b > a) ? _fullText.Substring(a, b - a) : null;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (string.IsNullOrEmpty(_fullText)) return;

        Focus();
        _canvas.Focus();

        _isSelecting = true;
        _canvas.CapturePointer(e.Pointer);

        var p = e.GetPosition(_canvas);
        int idx = GetCharIndexFromPoint(p);

        _selectionStart = idx;
        _selectionEnd = idx;
        _selectAllMode = false;

        _canvas.SetSelectionRects(BuildSelectionBands());
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isSelecting || string.IsNullOrEmpty(_fullText)) return;

        var p = e.GetPosition(_canvas);
        int idx = GetCharIndexFromPoint(p);
        if (idx != _selectionEnd)
        {
            _selectionEnd = idx;
            _canvas.SetSelectionRects(BuildSelectionBands());
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isSelecting = false;
        _canvas.ReleasePointerCapture(e.Pointer);
    }

    private sealed class TextCanvas : Control
    {
        private readonly VirtualTextViewer _owner;
        private TextLayout? _layout;
        private Thickness _padding;
        private double _contentHeight;
        private IReadOnlyList<Rect> _selRects = Array.Empty<Rect>();
        private readonly IBrush _selectionFill = new SolidColorBrush(Color.FromArgb(0x66, 0xC0, 0xE0, 0xFF));

        public TextCanvas(VirtualTextViewer owner) => _owner = owner;

        public void SetContext(TextLayout? layout, Thickness padding, double contentHeight)
        {
            _layout = layout;
            _padding = padding;
            _contentHeight = Math.Max(0, contentHeight);
            InvalidateMeasure();
            InvalidateVisual();
        }

        public void SetSelectionRects(IReadOnlyList<Rect> rects)
        {
            _selRects = rects ?? Array.Empty<Rect>();
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double w = double.IsFinite(availableSize.Width) ? availableSize.Width : 0;
            double h = _contentHeight;
            if (h < 0) h = 0;
            return new Size(w, h);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (_layout is null) return;

            var pad = _padding;

            if (_selRects.Count > 0)
            {
                foreach (var r in _selRects)
                {
                    var rr = new Rect(r.X + pad.Left, r.Y + pad.Top, r.Width, r.Height);
                    if (rr.Width > 0.1 && rr.Height > 0.1)
                        context.FillRectangle(_selectionFill, rr);
                }
            }

            _layout.Draw(context, new Point(pad.Left, pad.Top));
        }
    }
}
