using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using StegoRevealer.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoRevealer.UI.Components;

public enum HugeTextSanitizationMode
{
    None,
    Safe,
    Aggressive
}

public static class HugeTextBoxDiagnosticsSettings
{
    public static bool Enabled { get; set; } = true;
}

public partial class HugeTextBox : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<HugeTextBox, string?>(
            nameof(Text),
            string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<HugeTextBox, string?>(nameof(PlaceholderText), string.Empty);

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<HugeTextBox, bool>(nameof(IsReadOnly), true);

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<HugeTextBox, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);

    public static readonly StyledProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<HugeTextBox, ScrollBarVisibility>(
            nameof(HorizontalScrollBarVisibility),
            ScrollBarVisibility.Auto);

    public static readonly StyledProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<HugeTextBox, ScrollBarVisibility>(
            nameof(VerticalScrollBarVisibility),
            ScrollBarVisibility.Auto);

    public static readonly StyledProperty<HugeTextSanitizationMode> SanitizationModeProperty =
        AvaloniaProperty.Register<HugeTextBox, HugeTextSanitizationMode>(
            nameof(SanitizationMode),
            HugeTextSanitizationMode.Safe);

    public static readonly StyledProperty<char> ReplacementCharacterProperty =
        AvaloniaProperty.Register<HugeTextBox, char>(nameof(ReplacementCharacter), '\uFFFD');

    public static readonly StyledProperty<bool> ShowLineNumbersProperty =
        AvaloniaProperty.Register<HugeTextBox, bool>(nameof(ShowLineNumbers), false);

    public static readonly StyledProperty<bool> ShowFocusAdornerProperty =
        AvaloniaProperty.Register<HugeTextBox, bool>(nameof(ShowFocusAdorner), false);

    public static readonly StyledProperty<IBrush?> FocusAdornerBrushProperty =
        AvaloniaProperty.Register<HugeTextBox, IBrush?>(nameof(FocusAdornerBrush));

    public static readonly StyledProperty<IBrush?> ContentBackgroundProperty =
        AvaloniaProperty.Register<HugeTextBox, IBrush?>(nameof(ContentBackground));

    public static readonly StyledProperty<bool> ChunkLongLinesForRenderingProperty =
        AvaloniaProperty.Register<HugeTextBox, bool>(nameof(ChunkLongLinesForRendering), true);

    public static readonly StyledProperty<int> MaxDisplayLogicalLineLengthProperty =
        AvaloniaProperty.Register<HugeTextBox, int>(nameof(MaxDisplayLogicalLineLength), 2048);

    public static readonly StyledProperty<bool> ReflowSyntheticWrapOnResizeProperty =
        AvaloniaProperty.Register<HugeTextBox, bool>(nameof(ReflowSyntheticWrapOnResize), true);

    private const double HorizontalPadding = 6.0;
    private const double VerticalPadding = 2.0;
    private const double RightContentPadding = 18.0;
    private const double WrapPixelSafetyPadding = 6.0;
    private const double AutoScrollOutsideThreshold = 0.0;
    private const int LayoutCacheLimit = 256;
    private const int NumberCacheLimit = 128;
    private static readonly IBrush DefaultSelectionBrush = new SolidColorBrush(Color.FromArgb(96, 0, 120, 215));
    private static readonly IBrush InactiveSelectionBrush = new SolidColorBrush(Color.FromArgb(72, 128, 128, 128));
    private static readonly IBrush LineNumberBrush = new SolidColorBrush(Color.FromArgb(192, 160, 160, 160));
    private static readonly IBrush CaretBrush = Brushes.White;
    private static readonly IBrush DefaultFocusAdornerBrush = new SolidColorBrush(Color.FromArgb(96, 0, 120, 215));

    private enum BoundaryCaretAffinity
    {
        None,
        PreferPreviousSegment,
        PreferCurrentSegment
    }

    private readonly Border _outerBorder;
    private readonly Border _contentBorder;
    private readonly HugeTextSurface _surface;
    private readonly TextBlock _placeholder;
    private readonly ScrollBar _verticalScrollBar;
    private readonly ScrollBar _horizontalScrollBar;
    private readonly Border _scrollCorner;
    private readonly List<IDisposable> _subscriptions = new();
    private readonly Dictionary<int, TextLayout> _layoutCache = new();
    private readonly Dictionary<int, RenderSliceInfo> _renderSliceCache = new();
    private readonly Dictionary<string, TextLayout> _lineNumberLayoutCache = new(StringComparer.Ordinal);
    private readonly DispatcherTimer _caretBlinkTimer;
    private readonly DispatcherTimer _autoScrollTimer;

    private string _sourceText = string.Empty;
    private int[] _sourceLineStarts = new[] { 0 };
    private int[] _sourceLineLengths = new[] { 0 };
    private int[] _sourceLineBreakLengths = new[] { 0 };
    private int[] _sourceLineFirstDisplayIndex = new[] { 0 };
    private int[] _sourceLineDisplayCounts = new[] { 1 };
    private DisplayLineInfo[] _displayLines = new[] { DisplayLineInfo.Empty };

    private bool _isApplyingOuterText;
    private bool _isUpdatingOuterText;
    private bool _isUpdatingScrollBars;
    private bool _suspendCaretBlink;
    private bool _showCaret = true;
    private bool _isPointerSelecting;
    private Point _lastPointerPoint;
    private double _verticalOffset;
    private double _horizontalOffset;
    private double _viewportWidth;
    private double _viewportHeight;
    private double _lineHeight = 16.0;
    private double _averageGlyphWidth = 7.2;
    private double _lineNumberMarginWidth;
    private int _wrapColumn = 120;
    private bool _wrapColumnMeasured;
    private double _lastMeasuredWrapWidth = -1;
    private int _caretIndex;
    private int _selectionAnchorIndex;
    private BoundaryCaretAffinity _caretBoundaryAffinity;
    private double? _preferredCaretX;
    private int _logSequence;

    public HugeTextBox()
    {
        InitializeComponent();

        _outerBorder = this.FindControl<Border>("PART_OuterBorder")
            ?? throw new InvalidOperationException("PART_OuterBorder was not found.");
        _contentBorder = this.FindControl<Border>("PART_ContentBorder")
            ?? throw new InvalidOperationException("PART_ContentBorder was not found.");
        _surface = this.FindControl<HugeTextSurface>("PART_Surface")
            ?? throw new InvalidOperationException("PART_Surface was not found.");
        _placeholder = this.FindControl<TextBlock>("PART_Placeholder")
            ?? throw new InvalidOperationException("PART_Placeholder was not found.");
        _verticalScrollBar = this.FindControl<ScrollBar>("PART_VerticalScrollBar")
            ?? throw new InvalidOperationException("PART_VerticalScrollBar was not found.");
        _horizontalScrollBar = this.FindControl<ScrollBar>("PART_HorizontalScrollBar")
            ?? throw new InvalidOperationException("PART_HorizontalScrollBar was not found.");
        _scrollCorner = this.FindControl<Border>("PART_ScrollCorner")
            ?? throw new InvalidOperationException("PART_ScrollCorner was not found.");

        _surface.Owner = this;
        _surface.Cursor = new Cursor(StandardCursorType.Ibeam);
        _contentBorder.Cursor = new Cursor(StandardCursorType.Ibeam);

        _caretBlinkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(530) };
        _caretBlinkTimer.Tick += (_, _) =>
        {
            if (_suspendCaretBlink || !_surface.IsFocused)
                return;

            _showCaret = !_showCaret;
            _surface.InvalidateVisual();
        };

        _autoScrollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
        _autoScrollTimer.Tick += (_, _) => AutoScrollTick();

        _contentBorder.AddHandler(InputElement.PointerPressedEvent, SurfaceOnPointerPressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        _contentBorder.AddHandler(InputElement.PointerMovedEvent, SurfaceOnPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        _contentBorder.AddHandler(InputElement.PointerReleasedEvent, SurfaceOnPointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        _contentBorder.AddHandler(InputElement.PointerWheelChangedEvent, SurfaceOnPointerWheelChanged, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        _surface.KeyDown += SurfaceOnKeyDown;
        _surface.TextInput += SurfaceOnTextInput;
        _surface.GotFocus += SurfaceOnGotFocus;
        _surface.LostFocus += SurfaceOnLostFocus;

        Observe(this.GetObservable(TextProperty), nameof(TextProperty), OnOuterTextChanged);
        Observe(this.GetObservable(PlaceholderTextProperty), nameof(PlaceholderTextProperty), _ => UpdatePlaceholderVisibility());
        Observe(this.GetObservable(IsReadOnlyProperty), nameof(IsReadOnlyProperty), _ => ApplyConfiguration(rebuildDocument: false, rebuildDisplayLines: false, reason: "IsReadOnly changed"));
        Observe(this.GetObservable(TextWrappingProperty), nameof(TextWrappingProperty), _ => ApplyConfiguration(rebuildDocument: false, rebuildDisplayLines: true, reason: "TextWrapping changed"));
        Observe(this.GetObservable(HorizontalScrollBarVisibilityProperty), nameof(HorizontalScrollBarVisibilityProperty), _ => UpdateScrollBars("HorizontalScrollBarVisibility changed"));
        Observe(this.GetObservable(VerticalScrollBarVisibilityProperty), nameof(VerticalScrollBarVisibilityProperty), _ => UpdateScrollBars("VerticalScrollBarVisibility changed"));
        Observe(this.GetObservable(SanitizationModeProperty), nameof(SanitizationModeProperty), _ => ReapplySanitization("SanitizationMode changed"));
        Observe(this.GetObservable(ReplacementCharacterProperty), nameof(ReplacementCharacterProperty), _ => ReapplySanitization("ReplacementCharacter changed"));
        Observe(this.GetObservable(ShowLineNumbersProperty), nameof(ShowLineNumbersProperty), _ => ApplyConfiguration(rebuildDocument: false, rebuildDisplayLines: true, reason: "ShowLineNumbers changed"));
        Observe(this.GetObservable(ShowFocusAdornerProperty), nameof(ShowFocusAdornerProperty), _ => _surface.InvalidateVisual());
        Observe(this.GetObservable(FocusAdornerBrushProperty), nameof(FocusAdornerBrushProperty), _ => _surface.InvalidateVisual());
        Observe(this.GetObservable(ContentBackgroundProperty), nameof(ContentBackgroundProperty), _ => ApplyVisualOptions("ContentBackground changed"));
        Observe(this.GetObservable(BackgroundProperty), nameof(BackgroundProperty), _ => ApplyVisualOptions("Background changed"));
        Observe(this.GetObservable(ForegroundProperty), nameof(ForegroundProperty), _ => ApplyVisualOptions("Foreground changed"));
        Observe(this.GetObservable(FontFamilyProperty), nameof(FontFamilyProperty), _ => ApplyConfiguration(rebuildDocument: false, rebuildDisplayLines: true, reason: "FontFamily changed"));
        Observe(this.GetObservable(FontSizeProperty), nameof(FontSizeProperty), _ => ApplyConfiguration(rebuildDocument: false, rebuildDisplayLines: true, reason: "FontSize changed"));
        Observe(this.GetObservable(FontStyleProperty), nameof(FontStyleProperty), _ => ApplyConfiguration(rebuildDocument: false, rebuildDisplayLines: true, reason: "FontStyle changed"));
        Observe(this.GetObservable(FontWeightProperty), nameof(FontWeightProperty), _ => ApplyConfiguration(rebuildDocument: false, rebuildDisplayLines: true, reason: "FontWeight changed"));
        Observe(this.GetObservable(FontStretchProperty), nameof(FontStretchProperty), _ => ApplyConfiguration(rebuildDocument: false, rebuildDisplayLines: true, reason: "FontStretch changed"));
        Observe(this.GetObservable(ChunkLongLinesForRenderingProperty), nameof(ChunkLongLinesForRenderingProperty), _ => ApplyConfiguration(rebuildDocument: false, rebuildDisplayLines: true, reason: "ChunkLongLinesForRendering changed"));
        Observe(this.GetObservable(MaxDisplayLogicalLineLengthProperty), nameof(MaxDisplayLogicalLineLengthProperty), _ => ApplyConfiguration(rebuildDocument: false, rebuildDisplayLines: true, reason: "MaxDisplayLogicalLineLength changed"));
        Observe(this.GetObservable(ReflowSyntheticWrapOnResizeProperty), nameof(ReflowSyntheticWrapOnResizeProperty), _ => ApplyConfiguration(rebuildDocument: false, rebuildDisplayLines: false, reason: "ReflowSyntheticWrapOnResize changed"));
        Observe(_surface.GetObservable(BoundsProperty), "Surface.Bounds", _ => OnViewportBoundsChanged());
        Observe(_verticalScrollBar.GetObservable(RangeBase.ValueProperty), "VScroll.Value", _ => OnVerticalScrollBarValueChanged());
        Observe(_horizontalScrollBar.GetObservable(RangeBase.ValueProperty), "HScroll.Value", _ => OnHorizontalScrollBarValueChanged());

        ApplyVisualOptions("CTOR");
        SetSourceText(Text ?? string.Empty, resetSelection: true, reason: "CTOR");
        UpdatePlaceholderVisibility();
        LogInfo(() => "HugeTextBox initialized.");
    }

    public event EventHandler? TextChanged;
    public event EventHandler? CaretIndexChanged;

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    public HugeTextSanitizationMode SanitizationMode
    {
        get => GetValue(SanitizationModeProperty);
        set => SetValue(SanitizationModeProperty, value);
    }

    public char ReplacementCharacter
    {
        get => GetValue(ReplacementCharacterProperty);
        set => SetValue(ReplacementCharacterProperty, value);
    }

    public bool ShowLineNumbers
    {
        get => GetValue(ShowLineNumbersProperty);
        set => SetValue(ShowLineNumbersProperty, value);
    }

    public bool ShowFocusAdorner
    {
        get => GetValue(ShowFocusAdornerProperty);
        set => SetValue(ShowFocusAdornerProperty, value);
    }

    public IBrush? FocusAdornerBrush
    {
        get => GetValue(FocusAdornerBrushProperty);
        set => SetValue(FocusAdornerBrushProperty, value);
    }

    public IBrush? ContentBackground
    {
        get => GetValue(ContentBackgroundProperty);
        set => SetValue(ContentBackgroundProperty, value);
    }

    public bool ChunkLongLinesForRendering
    {
        get => GetValue(ChunkLongLinesForRenderingProperty);
        set => SetValue(ChunkLongLinesForRenderingProperty, value);
    }

    public int MaxDisplayLogicalLineLength
    {
        get => GetValue(MaxDisplayLogicalLineLengthProperty);
        set => SetValue(MaxDisplayLogicalLineLengthProperty, value);
    }

    public bool ReflowSyntheticWrapOnResize
    {
        get => GetValue(ReflowSyntheticWrapOnResizeProperty);
        set => SetValue(ReflowSyntheticWrapOnResizeProperty, value);
    }

    public int CaretIndex
    {
        get => _caretIndex;
        set => SetCaretAndSelection(value, HasSelection ? _selectionAnchorIndex : value, ensureVisible: true, raiseCaretEvent: true, reason: "CaretIndex set");
    }

    public int SelectionStart
    {
        get => Math.Min(_caretIndex, _selectionAnchorIndex);
        set => Select(value, SelectionLength);
    }

    public int SelectionLength
    {
        get => Math.Abs(_caretIndex - _selectionAnchorIndex);
        set => Select(SelectionStart, value);
    }

    public string SelectedText
    {
        get
        {
            if (!HasSelection)
                return string.Empty;

            var start = SelectionStart;
            return _sourceText.Substring(start, SelectionLength);
        }
        set
        {
            if (IsReadOnly)
                return;

            ReplaceSelection(value ?? string.Empty, "SelectedText set");
        }
    }

    public int LineCount => _displayLines.Length;

    public int TextLength => _sourceText.Length;

    public Control InnerEditor => _surface;

    public void Select(int start, int length)
    {
        start = Math.Clamp(start, 0, TextLength);
        length = Math.Max(0, length);
        var end = Math.Clamp(start + length, 0, TextLength);
        SetCaretAndSelection(end, start, ensureVisible: true, raiseCaretEvent: true, reason: $"Select({start},{length})");
    }

    public void SelectAll()
    {
        SetCaretAndSelection(TextLength, 0, ensureVisible: false, raiseCaretEvent: true, reason: "SelectAll");
    }

    public void Clear()
    {
        if (IsReadOnly)
        {
            Text = string.Empty;
            return;
        }

        ReplaceRange(0, TextLength, string.Empty, true, "Clear");
    }

    public void AppendText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (IsReadOnly)
        {
            Text = (_sourceText ?? string.Empty) + text;
            return;
        }

        ReplaceRange(TextLength, 0, text, moveCaretToInsertedEnd: true, reason: "AppendText");
    }

    public void Copy()
    {
        if (!HasSelection)
            return;

        var selected = SelectedText;
        if (string.IsNullOrEmpty(selected))
            return;

        _ = CopyTextToClipboardAsync(selected);
    }

    public void Cut()
    {
        if (IsReadOnly || !HasSelection)
            return;

        var selected = SelectedText;
        if (!string.IsNullOrEmpty(selected))
            _ = CopyTextToClipboardAsync(selected);

        ReplaceRange(SelectionStart, SelectionLength, string.Empty, moveCaretToInsertedEnd: false, reason: "Cut");
    }

    public async void Paste()
    {
        if (IsReadOnly)
            return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        var text = clipboard is null ? null : await clipboard.TryGetTextAsync();
        if (!string.IsNullOrEmpty(text))
            ReplaceSelection(text, "Paste");
    }

    public void ScrollToLine(int line)
    {
        if (_displayLines.Length == 0)
            return;

        var targetLine = Math.Clamp(line - 1, 0, _displayLines.Length - 1);
        SetVerticalOffset(targetLine * _lineHeight, "ScrollToLine");
    }

    public void ScrollTo(int line, int column)
    {
        if (_displayLines.Length == 0)
            return;

        var targetLine = Math.Clamp(line - 1, 0, _displayLines.Length - 1);
        var lineInfo = _displayLines[targetLine];
        var targetOffset = Math.Clamp(lineInfo.SourceStart + Math.Max(0, column), lineInfo.SourceStart, lineInfo.SourceStart + lineInfo.SourceLength);
        SetCaretAndSelection(targetOffset, targetOffset, ensureVisible: true, raiseCaretEvent: true, reason: "ScrollTo");
    }

    public void ScrollToEnd()
    {
        SetCaretAndSelection(TextLength, TextLength, ensureVisible: true, raiseCaretEvent: true, reason: "ScrollToEnd");
    }

    public void FocusEditor()
    {
        _surface.Focus();
    }

    public void RefreshWrapFromCurrentWidth()
    {
        _wrapColumnMeasured = false;
        _lastMeasuredWrapWidth = -1;
        RebuildDisplayLines("Manual RefreshWrapFromCurrentWidth", preserveTopDisplayLine: true);
    }

    internal void RenderSurface(DrawingContext context, HugeTextSurface surface)
    {
        var bounds = surface.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        using var clip = context.PushClip(new Rect(bounds.Size));

        var visibleSelectionBrush = _surface.IsFocused ? DefaultSelectionBrush : InactiveSelectionBrush;
        var firstVisibleLine = GetFirstVisibleDisplayLineIndex();
        var visibleLineCount = GetVisibleDisplayLineCount();
        var lastVisibleLine = Math.Min(_displayLines.Length - 1, firstVisibleLine + visibleLineCount);
        var textOriginX = GetTextAreaLeft();

        if (_surface.IsFocused && ShowFocusAdorner)
        {
            var focusBrush = FocusAdornerBrush ?? DefaultFocusAdornerBrush;
            if (focusBrush is not null)
                context.DrawRectangle(null, new Pen(focusBrush, 1), new Rect(0.5, 0.5, Math.Max(0, bounds.Width - 1), Math.Max(0, bounds.Height - 1)));
        }

        for (var lineIndex = firstVisibleLine; lineIndex <= lastVisibleLine; lineIndex++)
        {
            var lineInfo = _displayLines[lineIndex];
            var lineTop = GetDisplayLineTop(lineIndex) - _verticalOffset;
            if (lineTop > bounds.Height)
                break;

            if (lineTop + _lineHeight < 0)
                continue;

            var renderSlice = GetRenderSliceInfo(lineIndex);
            var layout = GetTextLayoutForLine(lineIndex, renderSlice);
            var drawX = textOriginX - _horizontalOffset + renderSlice.BaseX;
            var drawOrigin = new Point(drawX, lineTop + VerticalPadding);

            if (HasSelection)
            {
                DrawSelectionForLine(context, lineInfo, renderSlice, layout, drawOrigin, lineTop, visibleSelectionBrush);
            }

            layout.Draw(context, drawOrigin);

            if (ShowLineNumbers)
            {
                DrawLineNumber(context, lineInfo, lineTop);
            }
        }

        if (_surface.IsFocused && _showCaret)
        {
            DrawCaret(context, textOriginX);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Observe<T>(IObservable<T> observable, string name, Action<T> onNext)
    {
        _subscriptions.Add(observable.Subscribe(value =>
        {
            try
            {
                onNext(value);
            }
            catch (Exception ex)
            {
                LogError(() => $"Observable handler failed: {name}. {ex}");
                throw;
            }
        }));
    }

    private void OnOuterTextChanged(string? newValue)
    {
        if (_isUpdatingOuterText)
            return;

        SetSourceText(newValue ?? string.Empty, resetSelection: true, reason: "Outer Text changed");
    }

    private void ReapplySanitization(string reason)
    {
        if (_isApplyingOuterText)
            return;

        SetSourceText(_sourceText, resetSelection: false, reason: reason);
    }

    private void ApplyConfiguration(bool rebuildDocument, bool rebuildDisplayLines, string reason)
    {
        ApplyVisualOptions(reason);
        MeasureTypography(reason);

        if (rebuildDocument)
        {
            SetSourceText(_sourceText, resetSelection: false, reason: reason);
            return;
        }

        if (rebuildDisplayLines)
        {
            _wrapColumnMeasured = false;
            _lastMeasuredWrapWidth = -1;
            RebuildDisplayLines(reason, preserveTopDisplayLine: true);
            return;
        }

        UpdateScrollBars(reason);
        _surface.InvalidateVisual();
    }

    private void ApplyVisualOptions(string reason)
    {
        _outerBorder.Background = Background ?? Brushes.Transparent;
        _contentBorder.Background = ContentBackground ?? Background ?? Brushes.Transparent;
        _placeholder.Foreground = Foreground ?? Brushes.Gray;
        _scrollCorner.Background = Background;
        LogInfo(() => $"ApplyVisualOptions: reason={reason}, background={DescribeBrush(Background)}, contentBackground={DescribeBrush(ContentBackground ?? Background)}");
    }

    private void OnViewportBoundsChanged()
    {
        var width = Math.Max(0, _surface.Bounds.Width);
        var height = Math.Max(0, _surface.Bounds.Height);
        if (Math.Abs(width - _viewportWidth) < 0.5 && Math.Abs(height - _viewportHeight) < 0.5)
            return;

        var widthChanged = Math.Abs(width - _viewportWidth) >= 0.5;
        _viewportWidth = width;
        _viewportHeight = height;
        LogInfo(() => $"Viewport changed: width={_viewportWidth:0.##}, height={_viewportHeight:0.##}, widthChanged={widthChanged}");

        if (widthChanged && (ReflowSyntheticWrapOnResize || !_wrapColumnMeasured))
        {
            RebuildDisplayLines("Viewport width changed", preserveTopDisplayLine: true);
        }
        else
        {
            UpdateScrollBars("Viewport size changed");
            _surface.InvalidateVisual();
        }
    }

    private void MeasureTypography(string reason)
    {
        try
        {
            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var foreground = Foreground;
            using var sampleLayout = new TextLayout(
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
                typeface,
                FontSize,
                foreground,
                TextAlignment.Left,
                TextWrapping.NoWrap,
                maxWidth: double.PositiveInfinity,
                maxHeight: double.PositiveInfinity);

            using var lineLayout = new TextLayout(
                "Mg",
                typeface,
                FontSize,
                foreground,
                TextAlignment.Left,
                TextWrapping.NoWrap,
                maxWidth: double.PositiveInfinity,
                maxHeight: double.PositiveInfinity);

            _averageGlyphWidth = Math.Max(1.0, sampleLayout.WidthIncludingTrailingWhitespace / 62.0);
            _lineHeight = Math.Max(1.0, Math.Ceiling(lineLayout.Height + VerticalPadding * 2.0));

            if (ShowLineNumbers)
            {
                var digits = Math.Max(1, _sourceLineStarts.Length.ToString(CultureInfo.InvariantCulture).Length);
                using var lineNumberMeasure = new TextLayout(new string('8', digits), typeface, FontSize, foreground);
                var lineNumberWidth = lineNumberMeasure.WidthIncludingTrailingWhitespace;
                _lineNumberMarginWidth = Math.Ceiling(lineNumberWidth + 10.0);
            }
            else
            {
                _lineNumberMarginWidth = 0.0;
            }

            ClearLayoutCaches();
            LogInfo(() => $"MeasureTypography: reason={reason}, lineHeight={_lineHeight:0.##}, avgGlyphWidth={_averageGlyphWidth:0.###}, lineNumberMargin={_lineNumberMarginWidth:0.##}");
        }
        catch (Exception ex)
        {
            _averageGlyphWidth = Math.Max(1.0, FontSize * 0.60);
            _lineHeight = Math.Max(1.0, FontSize * 1.5);
            _lineNumberMarginWidth = ShowLineNumbers ? Math.Ceiling(_averageGlyphWidth * 5 + 10.0) : 0.0;
            ClearLayoutCaches();
            LogWarning(() => $"MeasureTypography fallback used. reason={reason}. {ex.Message}");
        }
    }

    private void SetSourceText(string rawText, bool resetSelection, string reason)
    {
        var sanitized = Sanitize(rawText);
        if (_sourceText == sanitized && !resetSelection)
        {
            UpdatePlaceholderVisibility();
            _surface.InvalidateVisual();
            return;
        }

        LogInfo(() => $"SetSourceText: reason={reason}, rawLen={rawText.Length}, sanitizedLen={sanitized.Length}, resetSelection={resetSelection}");

        _sourceText = sanitized;
        BuildSourceLineIndex();
        MeasureTypography(reason);
        RebuildDisplayLines(reason, preserveTopDisplayLine: false);

        if (resetSelection)
        {
            _caretIndex = 0;
            _selectionAnchorIndex = 0;
            _preferredCaretX = null;
            SetVerticalOffset(0, reason + "/reset-scroll-y");
            SetHorizontalOffset(0, reason + "/reset-scroll-x");
            RaiseCaretIndexChanged();
        }
        else
        {
            _caretIndex = Math.Clamp(_caretIndex, 0, TextLength);
            _selectionAnchorIndex = Math.Clamp(_selectionAnchorIndex, 0, TextLength);
            EnsureCaretVisible("SetSourceText");
        }

        UpdatePlaceholderVisibility();
        PushInternalTextToStyledPropertyIfNeeded();
        RaiseTextChanged();
        _surface.InvalidateVisual();
    }

    private void BuildSourceLineIndex()
    {
        var starts = new List<int>(Math.Max(4, _sourceText.Length / 64));
        var lengths = new List<int>(Math.Max(4, _sourceText.Length / 64));
        var breaks = new List<int>(Math.Max(4, _sourceText.Length / 64));

        var index = 0;
        while (index < _sourceText.Length)
        {
            var lineStart = index;
            while (index < _sourceText.Length && _sourceText[index] != '\r' && _sourceText[index] != '\n')
                index++;

            var lineLength = index - lineStart;
            var breakLength = 0;
            if (index < _sourceText.Length)
            {
                breakLength = 1;
                if (_sourceText[index] == '\r' && index + 1 < _sourceText.Length && _sourceText[index + 1] == '\n')
                    breakLength = 2;
                index += breakLength;
            }

            starts.Add(lineStart);
            lengths.Add(lineLength);
            breaks.Add(breakLength);
        }

        if (_sourceText.Length == 0 || (_sourceText.Length > 0 && (_sourceText[^1] == '\n' || _sourceText[^1] == '\r')))
        {
            starts.Add(_sourceText.Length);
            lengths.Add(0);
            breaks.Add(0);
        }

        if (starts.Count == 0)
        {
            starts.Add(0);
            lengths.Add(0);
            breaks.Add(0);
        }

        _sourceLineStarts = starts.ToArray();
        _sourceLineLengths = lengths.ToArray();
        _sourceLineBreakLengths = breaks.ToArray();
        LogInfo(() => $"BuildSourceLineIndex: sourceLineCount={_sourceLineStarts.Length}");
    }

    private void RebuildDisplayLines(string reason, bool preserveTopDisplayLine)
    {
        var previousTopLine = preserveTopDisplayLine ? GetFirstVisibleDisplayLineIndex() : 0;
        var previousTopFraction = preserveTopDisplayLine && _lineHeight > 0 ? (_verticalOffset / _lineHeight) - Math.Floor(_verticalOffset / _lineHeight) : 0.0;

        MeasureTypography(reason);
        DetermineWrapColumn(reason);
        BuildDisplayLines(reason);
        ClearLayoutCaches();
        UpdateScrollBars(reason);

        if (preserveTopDisplayLine)
        {
            var targetTop = Math.Clamp(previousTopLine, 0, Math.Max(0, _displayLines.Length - 1));
            var newOffset = targetTop * _lineHeight + previousTopFraction * _lineHeight;
            SetVerticalOffset(newOffset, $"{reason}/restore-top");
        }
        else
        {
            SetVerticalOffset(_verticalOffset, $"{reason}/clamp-y");
        }

        SetHorizontalOffset(_horizontalOffset, $"{reason}/clamp-x");
        EnsureCaretVisible(reason);
        _surface.InvalidateVisual();
        LogInfo(() => $"RebuildDisplayLines: reason={reason}, displayLineCount={_displayLines.Length}, wrapColumn={_wrapColumn}");
    }

    private void DetermineWrapColumn(string reason)
    {
        if (TextWrapping == TextWrapping.NoWrap)
        {
            _wrapColumnMeasured = true;
            _wrapColumn = Math.Max(16, MaxDisplayLogicalLineLength);
            return;
        }

        var availableWidth = GetAvailableWrapPixelWidth();
        if (availableWidth <= 0)
        {
            _wrapColumn = Math.Max(16, MaxDisplayLogicalLineLength > 0 ? Math.Min(MaxDisplayLogicalLineLength, 120) : 120);
            _wrapColumnMeasured = false;
            LogInfo(() => $"DetermineWrapColumn fallback: reason={reason}, wrapColumn={_wrapColumn}");
            return;
        }

        var hardLimit = Math.Max(16, MaxDisplayLogicalLineLength);
        var measuredColumns = Math.Max(16, (int)Math.Floor(availableWidth / _averageGlyphWidth));
        _wrapColumn = Math.Min(hardLimit, measuredColumns);
        _wrapColumnMeasured = true;
        _lastMeasuredWrapWidth = availableWidth;
        LogInfo(() => $"DetermineWrapColumn: reason={reason}, availableWidth={availableWidth:0.##}, wrapColumn={_wrapColumn}, hardLimit={hardLimit}");
    }

    private void BuildDisplayLines(string reason)
    {
        var lines = new List<DisplayLineInfo>(Math.Max(1, _sourceLineStarts.Length));
        _sourceLineFirstDisplayIndex = new int[_sourceLineStarts.Length];
        _sourceLineDisplayCounts = new int[_sourceLineStarts.Length];

        for (var sourceLineIndex = 0; sourceLineIndex < _sourceLineStarts.Length; sourceLineIndex++)
        {
            _sourceLineFirstDisplayIndex[sourceLineIndex] = lines.Count;
            var lineStart = _sourceLineStarts[sourceLineIndex];
            var lineLength = _sourceLineLengths[sourceLineIndex];

            if (TextWrapping == TextWrapping.NoWrap)
            {
                lines.Add(new DisplayLineInfo(lineStart, lineLength, sourceLineIndex, true));
                _sourceLineDisplayCounts[sourceLineIndex] = 1;
                continue;
            }

            if (lineLength == 0)
            {
                lines.Add(new DisplayLineInfo(lineStart, 0, sourceLineIndex, true));
                _sourceLineDisplayCounts[sourceLineIndex] = 1;
                continue;
            }

            var localOffset = 0;
            var wrappedCount = 0;
            while (localOffset < lineLength)
            {
                var segmentLength = ComputeWrappedSegmentLength(lineStart + localOffset, lineLength - localOffset);
                if (segmentLength <= 0)
                    segmentLength = Math.Min(lineLength - localOffset, Math.Max(1, _wrapColumn));

                if (TextWrapping != TextWrapping.NoWrap)
                    segmentLength = AdjustWrappedSegmentLengthToPixelWidth(lineStart + localOffset, segmentLength, lineLength - localOffset);

                lines.Add(new DisplayLineInfo(lineStart + localOffset, segmentLength, sourceLineIndex, wrappedCount == 0));
                wrappedCount++;
                localOffset += segmentLength;
            }

            _sourceLineDisplayCounts[sourceLineIndex] = Math.Max(1, wrappedCount);
        }

        if (lines.Count == 0)
            lines.Add(DisplayLineInfo.Empty);

        _displayLines = lines.ToArray();
        LogInfo(() => $"BuildDisplayLines: reason={reason}, sourceLineCount={_sourceLineStarts.Length}, displayLineCount={_displayLines.Length}");
    }

    private int ComputeWrappedSegmentLength(int absoluteStart, int remainingLength)
    {
        if (remainingLength <= 0)
            return 0;

        var hardLimit = Math.Max(1, _wrapColumn);
        if (remainingLength <= hardLimit)
            return remainingLength;

        if (TextWrapping == TextWrapping.WrapWithOverflow)
        {
            var wordBreak = FindLastBreakOpportunity(absoluteStart, hardLimit, includeHardLimitWhitespace: true);
            if (wordBreak > 0)
                return wordBreak;

            var overflowBreak = FindNextBreakOpportunity(absoluteStart + hardLimit, remainingLength - hardLimit);
            if (overflowBreak > 0)
                return hardLimit + overflowBreak;

            return Math.Min(remainingLength, Math.Max(hardLimit, MaxDisplayLogicalLineLength));
        }

        var wrapBreak = FindLastBreakOpportunity(absoluteStart, hardLimit, includeHardLimitWhitespace: true);
        if (wrapBreak > 0)
            return wrapBreak;

        return hardLimit;
    }

    private int AdjustWrappedSegmentLengthToPixelWidth(int absoluteStart, int proposedLength, int remainingLength)
    {
        if (TextWrapping == TextWrapping.NoWrap || proposedLength <= 1)
            return proposedLength;

        var availableWidth = GetAvailableWrapPixelWidth();
        if (availableWidth <= 1.0)
            return proposedLength;

        if (MeasureSourceSliceWidth(absoluteStart, proposedLength) <= availableWidth)
            return proposedLength;

        var low = 1;
        var high = Math.Min(proposedLength, remainingLength);
        var best = 1;
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1);
            var width = MeasureSourceSliceWidth(absoluteStart, mid);
            if (width <= availableWidth)
            {
                best = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        var preferred = FindLastBreakOpportunity(absoluteStart, best, includeHardLimitWhitespace: true);
        if (preferred > 0 && preferred <= best)
            best = preferred;

        return Math.Max(1, best);
    }

    private double GetAvailableWrapPixelWidth()
    {
        return Math.Max(0.0, _viewportWidth - GetTextAreaLeft() - HorizontalPadding - RightContentPadding - WrapPixelSafetyPadding);
    }

    private double MeasureSourceSliceWidth(int absoluteStart, int length)
    {
        length = Math.Clamp(length, 0, Math.Max(0, _sourceText.Length - absoluteStart));
        if (length <= 0)
            return 0.0;

        var slice = _sourceText.Substring(absoluteStart, length);
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        using var layout = new TextLayout(
            slice,
            typeface,
            FontSize,
            Foreground,
            TextAlignment.Left,
            TextWrapping.NoWrap,
            maxWidth: double.PositiveInfinity,
            maxHeight: double.PositiveInfinity);
        return layout.WidthIncludingTrailingWhitespace;
    }

    private int FindLastBreakOpportunity(int absoluteStart, int maxLength, bool includeHardLimitWhitespace)
    {
        var localEnd = Math.Min(_sourceText.Length, absoluteStart + maxLength);
        for (var i = localEnd - 1; i >= absoluteStart; i--)
        {
            var c = _sourceText[i];
            if (!char.IsWhiteSpace(c))
                continue;

            if (i == absoluteStart)
                continue;

            return (i - absoluteStart) + 1;
        }

        if (includeHardLimitWhitespace && localEnd < _sourceText.Length && char.IsWhiteSpace(_sourceText[localEnd]))
            return maxLength + 1;

        return 0;
    }

    private int FindNextBreakOpportunity(int absoluteStart, int maxScanLength)
    {
        var localEnd = Math.Min(_sourceText.Length, absoluteStart + Math.Max(0, maxScanLength));
        for (var i = absoluteStart; i < localEnd; i++)
        {
            if (char.IsWhiteSpace(_sourceText[i]))
                return (i - absoluteStart) + 1;
        }

        return 0;
    }

    private void UpdateScrollBars(string reason)
    {
        _isUpdatingScrollBars = true;
        try
        {
            var verticalMaximumOffset = Math.Max(0.0, GetExtentHeight() - _viewportHeight);
            var horizontalMaximumOffset = Math.Max(0.0, GetExtentWidth() - _viewportWidth);

            _verticalOffset = Math.Clamp(_verticalOffset, 0.0, verticalMaximumOffset);
            _horizontalOffset = Math.Clamp(_horizontalOffset, 0.0, horizontalMaximumOffset);

            _verticalScrollBar.Maximum = Math.Max(_viewportHeight, GetExtentHeight());
            _verticalScrollBar.ViewportSize = Math.Max(0.0, _viewportHeight);
            _verticalScrollBar.SmallChange = Math.Max(_lineHeight, 16.0);
            _verticalScrollBar.LargeChange = Math.Max(_viewportHeight * 0.9, _lineHeight);
            _verticalScrollBar.Value = _verticalOffset;

            _horizontalScrollBar.Maximum = Math.Max(_viewportWidth, GetExtentWidth());
            _horizontalScrollBar.ViewportSize = Math.Max(0.0, _viewportWidth);
            _horizontalScrollBar.SmallChange = Math.Max(_averageGlyphWidth * 4.0, 16.0);
            _horizontalScrollBar.LargeChange = Math.Max(_viewportWidth * 0.9, _averageGlyphWidth * 16.0);
            _horizontalScrollBar.Value = _horizontalOffset;

            var showVertical = ShouldShowVerticalScrollBar(verticalMaximumOffset);
            var showHorizontal = ShouldShowHorizontalScrollBar(horizontalMaximumOffset);
            _verticalScrollBar.IsVisible = showVertical;
            _horizontalScrollBar.IsVisible = showHorizontal;
            _scrollCorner.IsVisible = showVertical && showHorizontal;
        }
        finally
        {
            _isUpdatingScrollBars = false;
        }

        LogInfo(() => $"UpdateScrollBars: reason={reason}, verticalOffset={_verticalOffset:0.###}, horizontalOffset={_horizontalOffset:0.###}, extentHeight={GetExtentHeight():0.###}, extentWidth={GetExtentWidth():0.###}, viewport={_viewportWidth:0.###}x{_viewportHeight:0.###}");
    }

    private bool ShouldShowVerticalScrollBar(double maximumOffset)
    {
        return VerticalScrollBarVisibility switch
        {
            ScrollBarVisibility.Disabled => false,
            ScrollBarVisibility.Hidden => false,
            ScrollBarVisibility.Visible => true,
            _ => maximumOffset > 0.5,
        };
    }

    private bool ShouldShowHorizontalScrollBar(double maximumOffset)
    {
        return HorizontalScrollBarVisibility switch
        {
            ScrollBarVisibility.Disabled => false,
            ScrollBarVisibility.Hidden => false,
            ScrollBarVisibility.Visible => true,
            _ => maximumOffset > 0.5,
        };
    }

    private void OnVerticalScrollBarValueChanged()
    {
        if (_isUpdatingScrollBars)
            return;

        SetVerticalOffset(_verticalScrollBar.Value, "Vertical ScrollBar");
    }

    private void OnHorizontalScrollBarValueChanged()
    {
        if (_isUpdatingScrollBars)
            return;

        SetHorizontalOffset(_horizontalScrollBar.Value, "Horizontal ScrollBar");
    }

    private void SetVerticalOffset(double value, string reason)
    {
        var max = Math.Max(0.0, GetExtentHeight() - _viewportHeight);
        var clamped = Math.Clamp(value, 0.0, max);
        if (Math.Abs(clamped - _verticalOffset) < 0.25)
            return;

        _verticalOffset = clamped;
        UpdateScrollBars(reason);
        _surface.InvalidateVisual();
        LogInfo(() => $"SetVerticalOffset: reason={reason}, value={_verticalOffset:0.###}");
    }

    private void SetHorizontalOffset(double value, string reason)
    {
        var max = Math.Max(0.0, GetExtentWidth() - _viewportWidth);
        var clamped = Math.Clamp(value, 0.0, max);
        if (Math.Abs(clamped - _horizontalOffset) < 0.25)
            return;

        _horizontalOffset = clamped;
        _renderSliceCache.Clear();
        UpdateScrollBars(reason);
        _surface.InvalidateVisual();
        LogInfo(() => $"SetHorizontalOffset: reason={reason}, value={_horizontalOffset:0.###}");
    }

    private double GetExtentHeight() => _displayLines.Length * _lineHeight;

    private double GetExtentWidth()
    {
        if (_displayLines.Length == 0)
            return _viewportWidth;

        if (TextWrapping == TextWrapping.Wrap || TextWrapping == TextWrapping.WrapWithOverflow)
            return Math.Max(_viewportWidth, GetTextAreaLeft() + HorizontalPadding + RightContentPadding + Math.Max(0.0, GetAvailableWrapPixelWidth()));

        var maxChars = 0;
        for (var i = 0; i < _sourceLineLengths.Length; i++)
            maxChars = Math.Max(maxChars, _sourceLineLengths[i]);

        return Math.Max(_viewportWidth, GetTextAreaLeft() + HorizontalPadding + RightContentPadding + maxChars * _averageGlyphWidth);
    }

    private double GetTextAreaLeft() => _lineNumberMarginWidth + HorizontalPadding;

    private int GetFirstVisibleDisplayLineIndex()
    {
        if (_lineHeight <= 0)
            return 0;

        return Math.Clamp((int)Math.Floor(_verticalOffset / _lineHeight), 0, Math.Max(0, _displayLines.Length - 1));
    }

    private int GetVisibleDisplayLineCount()
    {
        if (_lineHeight <= 0)
            return 1;

        return Math.Max(1, (int)Math.Ceiling(_viewportHeight / _lineHeight) + 1);
    }

    private double GetDisplayLineTop(int displayLineIndex) => displayLineIndex * _lineHeight;

    private void DrawSelectionForLine(
        DrawingContext context,
        DisplayLineInfo lineInfo,
        RenderSliceInfo renderSlice,
        TextLayout layout,
        Point drawOrigin,
        double lineTop,
        IBrush brush)
    {
        var selectionStart = SelectionStart;
        var selectionEnd = selectionStart + SelectionLength;
        var lineStart = lineInfo.SourceStart;
        var lineEnd = lineInfo.SourceStart + lineInfo.SourceLength;

        var overlapStart = Math.Max(selectionStart, lineStart);
        var overlapEnd = Math.Min(selectionEnd, lineEnd);
        if (overlapStart >= overlapEnd)
            return;

        var sliceVisibleStart = lineStart + renderSlice.SliceStart;
        var sliceVisibleEnd = sliceVisibleStart + renderSlice.SliceText.Length;
        overlapStart = Math.Max(overlapStart, sliceVisibleStart);
        overlapEnd = Math.Min(overlapEnd, sliceVisibleEnd);
        if (overlapStart >= overlapEnd)
            return;

        var localStart = overlapStart - sliceVisibleStart;
        var localLength = overlapEnd - overlapStart;

        foreach (var rect in layout.HitTestTextRange(localStart, localLength))
        {
            var selectionRect = new Rect(
                drawOrigin.X + rect.X,
                lineTop + VerticalPadding + rect.Y,
                rect.Width,
                Math.Max(1.0, rect.Height));
            context.DrawRectangle(brush, null, selectionRect);
        }
    }

    private void DrawLineNumber(DrawingContext context, DisplayLineInfo lineInfo, double lineTop)
    {
        if (!lineInfo.IsFirstVisualSegmentOfSourceLine)
            return;

        var lineNumberText = (lineInfo.SourceLineIndex + 1).ToString(CultureInfo.InvariantCulture);
        var layout = GetLineNumberLayout(lineNumberText);
        var x = Math.Max(0, _lineNumberMarginWidth - layout.WidthIncludingTrailingWhitespace - 4.0);
        var y = lineTop + VerticalPadding;
        layout.Draw(context, new Point(x, y));
    }

    private void DrawCaret(DrawingContext context, double textOriginX)
    {
        if (_caretIndex < 0 || _displayLines.Length == 0)
            return;

        var displayLineIndex = GetDisplayLineIndexFromSourceOffset(_caretIndex, _caretBoundaryAffinity, out _);
        if (displayLineIndex < 0 || displayLineIndex >= _displayLines.Length)
            return;

        var lineTop = GetDisplayLineTop(displayLineIndex) - _verticalOffset;
        if (lineTop + _lineHeight < 0 || lineTop > _viewportHeight)
            return;

        var renderSlice = GetRenderSliceInfo(displayLineIndex);
        var lineInfo = _displayLines[displayLineIndex];
        var localInLine = Math.Clamp(_caretIndex - lineInfo.SourceStart, 0, lineInfo.SourceLength);
        if (localInLine < renderSlice.SliceStart || localInLine > renderSlice.SliceStart + renderSlice.SliceText.Length)
            return;

        var layout = GetTextLayoutForLine(displayLineIndex, renderSlice);
        var localInSlice = localInLine - renderSlice.SliceStart;
        var hitRect = layout.HitTestTextPosition(localInSlice);
        var x = textOriginX - _horizontalOffset + renderSlice.BaseX + hitRect.X;
        var caretRect = new Rect(x, lineTop + 1, 1.0, Math.Max(1.0, _lineHeight - 2));
        context.DrawRectangle(CaretBrush, null, caretRect);
    }

    private void SurfaceOnGotFocus(object? sender, GotFocusEventArgs e)
    {
        _showCaret = true;
        _suspendCaretBlink = false;
        _caretBlinkTimer.Start();
        _surface.InvalidateVisual();
    }

    private void SurfaceOnLostFocus(object? sender, RoutedEventArgs e)
    {
        _caretBlinkTimer.Stop();
        _showCaret = false;
        _autoScrollTimer.Stop();
        _isPointerSelecting = false;
        _surface.InvalidateVisual();
    }

    private void SurfaceOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        FocusEditor();
        _showCaret = true;
        _suspendCaretBlink = false;
        RestartCaretBlink();

        var point = e.GetPosition(_surface);
        LogInfo(() => $"PointerPressed at {point}");
        _lastPointerPoint = point;
        var hit = GetCaretHitFromPoint(point);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var anchor = shift ? _selectionAnchorIndex : hit.SourceOffset;
        SetCaretAndSelection(hit.SourceOffset, anchor, ensureVisible: false, raiseCaretEvent: true, reason: "PointerPressed", caretAffinity: hit.Affinity);
        _isPointerSelecting = true;
        e.Pointer.Capture(_contentBorder);
        _autoScrollTimer.Start();
        e.Handled = true;
    }

    private void SurfaceOnPointerMoved(object? sender, PointerEventArgs e)
    {
        _lastPointerPoint = e.GetPosition(_surface);
        LogInfo(() => $"PointerMoved at {_lastPointerPoint}, selecting={_isPointerSelecting}");
        if (!_isPointerSelecting)
            return;

        UpdateSelectionFromPointer(_lastPointerPoint, ensureVisible: false, reason: "PointerMoved");
        e.Handled = true;
    }

    private void SurfaceOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _lastPointerPoint = e.GetPosition(_surface);
        LogInfo(() => $"PointerReleased at {_lastPointerPoint}, selecting={_isPointerSelecting}");
        if (_isPointerSelecting)
        {
            UpdateSelectionFromPointer(_lastPointerPoint, ensureVisible: true, reason: "PointerReleased");
        }

        _isPointerSelecting = false;
        _autoScrollTimer.Stop();
        e.Pointer.Capture(null);
        RestartCaretBlink();
        e.Handled = true;
    }

    private void SurfaceOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var verticalDelta = -e.Delta.Y * Math.Max(_lineHeight * 3.0, 48.0);
        var horizontalDelta = -e.Delta.X * Math.Max(_averageGlyphWidth * 12.0, 48.0);

        if (Math.Abs(verticalDelta) > 0.01)
            SetVerticalOffset(_verticalOffset + verticalDelta, "MouseWheel");

        if (Math.Abs(horizontalDelta) > 0.01)
            SetHorizontalOffset(_horizontalOffset + horizontalDelta, "MouseWheel");

        e.Handled = true;
    }

    private void SurfaceOnKeyDown(object? sender, KeyEventArgs e)
    {
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var ctrlLike = e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta);

        switch (e.Key)
        {
            case Key.Left:
                MoveCaretHorizontally(backward: true, extendSelection: shift, byWord: ctrlLike, reason: "Key.Left");
                e.Handled = true;
                break;
            case Key.Right:
                MoveCaretHorizontally(backward: false, extendSelection: shift, byWord: ctrlLike, reason: "Key.Right");
                e.Handled = true;
                break;
            case Key.Up:
                MoveCaretVertically(-1, shift, ctrlLike ? "Key.Ctrl+Up" : "Key.Up");
                e.Handled = true;
                break;
            case Key.Down:
                MoveCaretVertically(1, shift, ctrlLike ? "Key.Ctrl+Down" : "Key.Down");
                e.Handled = true;
                break;
            case Key.PageUp:
                MoveCaretVertically(-Math.Max(1, GetVisibleDisplayLineCount() - 1), shift, "Key.PageUp");
                e.Handled = true;
                break;
            case Key.PageDown:
                MoveCaretVertically(Math.Max(1, GetVisibleDisplayLineCount() - 1), shift, "Key.PageDown");
                e.Handled = true;
                break;
            case Key.Home:
                MoveCaretToLineBoundary(start: true, extendSelection: shift, documentBoundary: ctrlLike, reason: ctrlLike ? "Ctrl+Home" : "Home");
                e.Handled = true;
                break;
            case Key.End:
                MoveCaretToLineBoundary(start: false, extendSelection: shift, documentBoundary: ctrlLike, reason: ctrlLike ? "Ctrl+End" : "End");
                e.Handled = true;
                break;
            case Key.Back:
                if (!IsReadOnly)
                {
                    DeleteBackward();
                    e.Handled = true;
                }
                break;
            case Key.Delete:
                if (!IsReadOnly)
                {
                    DeleteForward();
                    e.Handled = true;
                }
                break;
            case Key.Enter:
                if (!IsReadOnly)
                {
                    ReplaceSelection(Environment.NewLine, "Enter");
                    e.Handled = true;
                }
                break;
            case Key.Tab:
                if (!IsReadOnly)
                {
                    ReplaceSelection("\t", "Tab");
                    e.Handled = true;
                }
                break;
            case Key.A when ctrlLike:
                SelectAll();
                e.Handled = true;
                break;
            case Key.C when ctrlLike:
                Copy();
                e.Handled = true;
                break;
            case Key.X when ctrlLike:
                Cut();
                e.Handled = true;
                break;
            case Key.V when ctrlLike:
                Paste();
                e.Handled = true;
                break;
        }
    }

    private void SurfaceOnTextInput(object? sender, TextInputEventArgs e)
    {
        if (IsReadOnly || string.IsNullOrEmpty(e.Text))
            return;

        if (e.Text.All(char.IsControl))
            return;

        ReplaceSelection(e.Text, "TextInput");
        e.Handled = true;
    }

    private void AutoScrollTick()
    {
        if (!_isPointerSelecting)
            return;

        var adjusted = false;
        if (_lastPointerPoint.Y < -AutoScrollOutsideThreshold)
        {
            var distance = Math.Abs(_lastPointerPoint.Y);
            SetVerticalOffset(_verticalOffset - Math.Max(_lineHeight, 12.0) - distance, "AutoScrollUp");
            adjusted = true;
        }
        else if (_lastPointerPoint.Y > _viewportHeight + AutoScrollOutsideThreshold)
        {
            var distance = _lastPointerPoint.Y - _viewportHeight;
            SetVerticalOffset(_verticalOffset + Math.Max(_lineHeight, 12.0) + distance, "AutoScrollDown");
            adjusted = true;
        }

        if (_lastPointerPoint.X < -AutoScrollOutsideThreshold)
        {
            var distance = Math.Abs(_lastPointerPoint.X);
            SetHorizontalOffset(_horizontalOffset - Math.Max(_averageGlyphWidth * 4.0, 12.0) - distance, "AutoScrollLeft");
            adjusted = true;
        }
        else if (_lastPointerPoint.X > _viewportWidth + AutoScrollOutsideThreshold)
        {
            var distance = _lastPointerPoint.X - _viewportWidth;
            SetHorizontalOffset(_horizontalOffset + Math.Max(_averageGlyphWidth * 4.0, 12.0) + distance, "AutoScrollRight");
            adjusted = true;
        }

        if (adjusted)
            UpdateSelectionFromPointer(_lastPointerPoint, ensureVisible: false, reason: "AutoScrollTick");
    }

    private void UpdateSelectionFromPointer(Point point, bool ensureVisible, string reason)
    {
        var clipped = new Point(
            Math.Clamp(point.X, 0, Math.Max(0.0, _viewportWidth - 1)),
            Math.Clamp(point.Y, 0, Math.Max(0.0, _viewportHeight - 1)));

        var hit = GetCaretHitFromPoint(clipped);
        SetCaretAndSelection(hit.SourceOffset, _selectionAnchorIndex, ensureVisible, raiseCaretEvent: true, reason: reason, caretAffinity: hit.Affinity);
    }

    private void MoveCaretHorizontally(bool backward, bool extendSelection, bool byWord, string reason)
    {
        var anchor = extendSelection ? _selectionAnchorIndex : _caretIndex;
        var target = byWord
            ? (backward ? FindPreviousWordBoundary(_caretIndex) : FindNextWordBoundary(_caretIndex))
            : (backward ? FindPreviousCaretIndex(_caretIndex) : FindNextCaretIndex(_caretIndex));

        SetCaretAndSelection(target, anchor, ensureVisible: true, raiseCaretEvent: true, reason: reason);
        _preferredCaretX = null;
    }

    private void MoveCaretVertically(int displayLineDelta, bool extendSelection, string reason)
    {
        if (_displayLines.Length == 0)
            return;

        var currentLineIndex = GetDisplayLineIndexFromSourceOffset(_caretIndex, _caretBoundaryAffinity, out _);
        var targetLineIndex = Math.Clamp(currentLineIndex + displayLineDelta, 0, _displayLines.Length - 1);
        var desiredX = _preferredCaretX ?? GetCaretDocumentX(_caretIndex);
        var targetHit = GetCaretHitFromDisplayLineAndDocumentX(targetLineIndex, desiredX);
        var anchor = extendSelection ? _selectionAnchorIndex : targetHit.SourceOffset;

        _preferredCaretX = desiredX;
        SetCaretAndSelection(targetHit.SourceOffset, anchor, ensureVisible: true, raiseCaretEvent: true, reason: reason, caretAffinity: targetHit.Affinity);
    }

    private void MoveCaretToLineBoundary(bool start, bool extendSelection, bool documentBoundary, string reason)
    {
        int target;
        if (documentBoundary)
        {
            target = start ? 0 : TextLength;
        }
        else
        {
            var displayLineIndex = GetDisplayLineIndexFromSourceOffset(_caretIndex, _caretBoundaryAffinity, out _);
            var displayLine = _displayLines[displayLineIndex];
            target = start ? displayLine.SourceStart : displayLine.SourceStart + displayLine.SourceLength;
        }

        var anchor = extendSelection ? _selectionAnchorIndex : target;
        SetCaretAndSelection(target, anchor, ensureVisible: true, raiseCaretEvent: true, reason: reason, caretAffinity: start ? BoundaryCaretAffinity.PreferCurrentSegment : BoundaryCaretAffinity.PreferPreviousSegment);
        _preferredCaretX = null;
    }

    private void DeleteBackward()
    {
        if (HasSelection)
        {
            ReplaceRange(SelectionStart, SelectionLength, string.Empty, false, "Backspace selection");
            return;
        }

        if (_caretIndex <= 0)
            return;

        var previous = FindPreviousCaretIndex(_caretIndex);
        ReplaceRange(previous, _caretIndex - previous, string.Empty, false, "Backspace");
    }

    private void DeleteForward()
    {
        if (HasSelection)
        {
            ReplaceRange(SelectionStart, SelectionLength, string.Empty, false, "Delete selection");
            return;
        }

        if (_caretIndex >= TextLength)
            return;

        var next = FindNextCaretIndex(_caretIndex);
        ReplaceRange(_caretIndex, next - _caretIndex, string.Empty, false, "Delete");
    }

    private void ReplaceSelection(string insertedText, string reason)
    {
        ReplaceRange(SelectionStart, SelectionLength, insertedText, true, reason);
    }

    private void ReplaceRange(int start, int length, string insertedText, bool moveCaretToInsertedEnd, string reason)
    {
        start = Math.Clamp(start, 0, TextLength);
        length = Math.Clamp(length, 0, TextLength - start);
        insertedText = Sanitize(insertedText);

        var builder = new StringBuilder(_sourceText.Length - length + insertedText.Length);
        builder.Append(_sourceText, 0, start);
        builder.Append(insertedText);
        var tailStart = start + length;
        if (tailStart < _sourceText.Length)
            builder.Append(_sourceText, tailStart, _sourceText.Length - tailStart);

        var newText = builder.ToString();
        var newCaret = moveCaretToInsertedEnd ? start + insertedText.Length : start;
        var previousTopLine = GetFirstVisibleDisplayLineIndex();
        var previousTopFraction = _lineHeight > 0 ? (_verticalOffset / _lineHeight) - Math.Floor(_verticalOffset / _lineHeight) : 0.0;

        _sourceText = newText;
        BuildSourceLineIndex();
        RebuildDisplayLines(reason, preserveTopDisplayLine: false);
        if (_displayLines.Length > 0)
            SetVerticalOffset(previousTopLine * _lineHeight + previousTopFraction * _lineHeight, reason + "/restore-view");

        SetCaretAndSelection(newCaret, newCaret, ensureVisible: true, raiseCaretEvent: true, reason: reason);
        UpdatePlaceholderVisibility();
        PushInternalTextToStyledPropertyIfNeeded();
        RaiseTextChanged();
        _surface.InvalidateVisual();
        LogInfo(() => $"ReplaceRange: reason={reason}, start={start}, length={length}, insertedLen={insertedText.Length}, newTextLen={_sourceText.Length}");
    }

    private void SetCaretAndSelection(int caretIndex, int anchorIndex, bool ensureVisible, bool raiseCaretEvent, string reason, BoundaryCaretAffinity caretAffinity = BoundaryCaretAffinity.None)
    {
        caretIndex = Math.Clamp(caretIndex, 0, TextLength);
        anchorIndex = Math.Clamp(anchorIndex, 0, TextLength);

        var changed = caretIndex != _caretIndex || anchorIndex != _selectionAnchorIndex || caretAffinity != _caretBoundaryAffinity;
        _caretIndex = caretIndex;
        _selectionAnchorIndex = anchorIndex;
        _caretBoundaryAffinity = caretAffinity;

        RestartCaretBlink();
        if (ensureVisible)
            EnsureCaretVisible(reason);
        else
            _surface.InvalidateVisual();

        if (raiseCaretEvent && changed)
            RaiseCaretIndexChanged();
    }

    private void EnsureCaretVisible(string reason)
    {
        if (_displayLines.Length == 0 || _viewportWidth <= 0.5 || _viewportHeight <= 0.5)
            return;

        var lineIndex = GetDisplayLineIndexFromSourceOffset(_caretIndex, _caretBoundaryAffinity, out _);
        var lineTop = GetDisplayLineTop(lineIndex);
        if (lineTop < _verticalOffset)
            _verticalOffset = lineTop;
        else if (lineTop + _lineHeight > _verticalOffset + _viewportHeight)
            _verticalOffset = Math.Max(0.0, lineTop + _lineHeight - _viewportHeight);

        var caretX = GetCaretDocumentX(_caretIndex);
        if (caretX < _horizontalOffset)
            _horizontalOffset = Math.Max(0.0, caretX - _averageGlyphWidth * 2.0);
        else if (caretX > _horizontalOffset + Math.Max(0.0, _viewportWidth - GetTextAreaLeft() - HorizontalPadding - RightContentPadding))
            _horizontalOffset = Math.Max(0.0, caretX - Math.Max(0.0, _viewportWidth - GetTextAreaLeft() - HorizontalPadding - RightContentPadding) + _averageGlyphWidth * 2.0);

        UpdateScrollBars(reason + "/EnsureCaretVisible");
        _surface.InvalidateVisual();
    }

    private int GetDisplayLineIndexFromSourceOffset(int sourceOffset, BoundaryCaretAffinity affinity, out int localOffsetInDisplayLine)
    {
        sourceOffset = Math.Clamp(sourceOffset, 0, TextLength);
        var sourceLineIndex = GetSourceLineIndexFromOffset(sourceOffset);
        var lineStart = _sourceLineStarts[sourceLineIndex];
        var contentLength = _sourceLineLengths[sourceLineIndex];
        var relativeOffset = Math.Clamp(sourceOffset - lineStart, 0, contentLength);
        var firstDisplay = _sourceLineFirstDisplayIndex[sourceLineIndex];
        var displayCount = _sourceLineDisplayCounts[sourceLineIndex];

        for (var i = 0; i < displayCount; i++)
        {
            var displayIndex = firstDisplay + i;
            var displayLine = _displayLines[displayIndex];
            var segmentStart = displayLine.SourceStart - lineStart;
            var segmentEnd = segmentStart + displayLine.SourceLength;
            var isLastSegment = i == displayCount - 1;

            if (relativeOffset < segmentEnd)
            {
                localOffsetInDisplayLine = Math.Clamp(relativeOffset - segmentStart, 0, displayLine.SourceLength);
                return displayIndex;
            }

            if (relativeOffset == segmentEnd)
            {
                if (isLastSegment || affinity != BoundaryCaretAffinity.PreferCurrentSegment)
                {
                    localOffsetInDisplayLine = Math.Clamp(relativeOffset - segmentStart, 0, displayLine.SourceLength);
                    return displayIndex;
                }

                continue;
            }

            if (isLastSegment)
            {
                localOffsetInDisplayLine = Math.Clamp(relativeOffset - segmentStart, 0, displayLine.SourceLength);
                return displayIndex;
            }
        }

        localOffsetInDisplayLine = 0;
        return firstDisplay;
    }

    private int GetSourceLineIndexFromOffset(int sourceOffset)
    {
        sourceOffset = Math.Clamp(sourceOffset, 0, TextLength);

        // Important: line starts are boundary markers for *physical* source lines.
        // When the caret/source offset is exactly equal to the start of the next line,
        // we must resolve it to that next line, not to the previous line whose inclusive
        // [content + line-break] range also ends at the same offset.
        // So this search is intentionally based on line starts, not on inclusive end ranges.
        var low = 0;
        var high = _sourceLineStarts.Length - 1;
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1);
            var start = _sourceLineStarts[mid];

            if (sourceOffset < start)
            {
                high = mid - 1;
                continue;
            }

            if (mid + 1 < _sourceLineStarts.Length && sourceOffset >= _sourceLineStarts[mid + 1])
            {
                low = mid + 1;
                continue;
            }

            return mid;
        }

        return Math.Clamp(high >= 0 ? high : 0, 0, _sourceLineStarts.Length - 1);
    }

    private CaretHitInfo GetCaretHitFromPoint(Point point)
    {
        if (_displayLines.Length == 0)
            return new CaretHitInfo(0, BoundaryCaretAffinity.None);

        var lineIndex = Math.Clamp((int)Math.Floor((point.Y + _verticalOffset) / _lineHeight), 0, _displayLines.Length - 1);
        return GetCaretHitFromDisplayLineAndViewportPoint(lineIndex, point);
    }

    private CaretHitInfo GetCaretHitFromDisplayLineAndViewportPoint(int lineIndex, Point point)
    {
        var lineInfo = _displayLines[lineIndex];
        var renderSlice = GetRenderSliceInfo(lineIndex);
        var layout = GetTextLayoutForLine(lineIndex, renderSlice);

        var drawX = GetTextAreaLeft() - _horizontalOffset + renderSlice.BaseX;
        var localX = point.X - drawX;
        var localY = Math.Clamp(point.Y - (GetDisplayLineTop(lineIndex) - _verticalOffset + VerticalPadding), 0.0, _lineHeight);

        if (localX <= 0)
            return new CaretHitInfo(lineInfo.SourceStart, BoundaryCaretAffinity.PreferCurrentSegment);

        var renderedWidth = layout.WidthIncludingTrailingWhitespace;
        if (localX >= renderedWidth)
            return new CaretHitInfo(lineInfo.SourceStart + lineInfo.SourceLength, BoundaryCaretAffinity.PreferPreviousSegment);

        var hit = layout.HitTestPoint(new Point(localX, localY));
        var localOffset = hit.TextPosition + (hit.IsTrailing ? 1 : 0) + renderSlice.SliceStart;
        var sourceOffset = Math.Clamp(lineInfo.SourceStart + localOffset, lineInfo.SourceStart, lineInfo.SourceStart + lineInfo.SourceLength);
        var affinity = BoundaryCaretAffinity.None;
        if (sourceOffset == lineInfo.SourceStart)
            affinity = BoundaryCaretAffinity.PreferCurrentSegment;
        else if (sourceOffset == lineInfo.SourceStart + lineInfo.SourceLength)
            affinity = BoundaryCaretAffinity.PreferPreviousSegment;

        return new CaretHitInfo(sourceOffset, affinity);
    }

    private double GetCaretDocumentX(int sourceOffset)
    {
        if (_displayLines.Length == 0)
            return 0.0;

        var lineIndex = GetDisplayLineIndexFromSourceOffset(sourceOffset, _caretBoundaryAffinity, out var localOffset);
        var renderSlice = GetRenderSliceInfo(lineIndex, forceSliceAroundDocumentColumn: localOffset);
        var layout = GetTextLayoutForLine(lineIndex, renderSlice);
        var localInSlice = Math.Clamp(localOffset - renderSlice.SliceStart, 0, renderSlice.SliceText.Length);
        var rect = layout.HitTestTextPosition(localInSlice);
        return renderSlice.BaseX + rect.X;
    }

    private CaretHitInfo GetCaretHitFromDisplayLineAndDocumentX(int displayLineIndex, double documentX)
    {
        displayLineIndex = Math.Clamp(displayLineIndex, 0, _displayLines.Length - 1);
        var renderSlice = GetRenderSliceInfo(displayLineIndex, forceSliceAroundDocumentX: documentX);
        var layout = GetTextLayoutForLine(displayLineIndex, renderSlice);
        var localX = documentX - renderSlice.BaseX;
        var line = _displayLines[displayLineIndex];

        if (localX <= 0)
            return new CaretHitInfo(line.SourceStart, BoundaryCaretAffinity.PreferCurrentSegment);

        var renderedWidth = layout.WidthIncludingTrailingWhitespace;
        if (localX >= renderedWidth)
            return new CaretHitInfo(line.SourceStart + line.SourceLength, BoundaryCaretAffinity.PreferPreviousSegment);

        var hit = layout.HitTestPoint(new Point(localX, _lineHeight / 2.0));
        var local = hit.TextPosition + (hit.IsTrailing ? 1 : 0) + renderSlice.SliceStart;
        var sourceOffset = Math.Clamp(line.SourceStart + local, line.SourceStart, line.SourceStart + line.SourceLength);
        var affinity = BoundaryCaretAffinity.None;
        if (sourceOffset == line.SourceStart)
            affinity = BoundaryCaretAffinity.PreferCurrentSegment;
        else if (sourceOffset == line.SourceStart + line.SourceLength)
            affinity = BoundaryCaretAffinity.PreferPreviousSegment;

        return new CaretHitInfo(sourceOffset, affinity);
    }

    private RenderSliceInfo GetRenderSliceInfo(int displayLineIndex, int? forceSliceAroundDocumentColumn = null, double? forceSliceAroundDocumentX = null)
    {
        var useCache = forceSliceAroundDocumentColumn is null && forceSliceAroundDocumentX is null;
        if (useCache && _renderSliceCache.TryGetValue(displayLineIndex, out var cached))
            return cached;

        var line = _displayLines[displayLineIndex];
        var text = line.SourceLength == 0 ? string.Empty : _sourceText.Substring(line.SourceStart, line.SourceLength);
        var useApproximateNoWrapSlice = TextWrapping == TextWrapping.NoWrap && line.SourceLength > Math.Max(64, MaxDisplayLogicalLineLength);

        RenderSliceInfo result;
        if (!useApproximateNoWrapSlice)
        {
            result = new RenderSliceInfo(0, 0.0, text);
        }
        else
        {
            var viewportChars = Math.Max(64, (int)Math.Ceiling(Math.Max(64.0, _viewportWidth) / _averageGlyphWidth) + 96);
            var maxSliceChars = Math.Max(viewportChars, MaxDisplayLogicalLineLength);
            var documentColumn = forceSliceAroundDocumentColumn
                ?? (forceSliceAroundDocumentX is double docX
                    ? (int)Math.Floor(Math.Max(0.0, docX) / _averageGlyphWidth)
                    : (int)Math.Floor(_horizontalOffset / _averageGlyphWidth));

            var sliceStart = Math.Clamp(documentColumn - 64, 0, Math.Max(0, line.SourceLength - 1));
            if (sliceStart + maxSliceChars > line.SourceLength)
                sliceStart = Math.Max(0, line.SourceLength - maxSliceChars);

            var sliceLength = Math.Clamp(maxSliceChars, 0, line.SourceLength - sliceStart);
            var sliceText = text.Substring(sliceStart, sliceLength);
            result = new RenderSliceInfo(sliceStart, sliceStart * _averageGlyphWidth, sliceText);
        }

        if (useCache)
        {
            if (_renderSliceCache.Count >= LayoutCacheLimit)
                _renderSliceCache.Clear();
            _renderSliceCache[displayLineIndex] = result;
        }

        return result;
    }

    private TextLayout GetTextLayoutForLine(int displayLineIndex, RenderSliceInfo renderSlice)
    {
        var hash = new HashCode();
        hash.Add(displayLineIndex);
        hash.Add(renderSlice.SliceStart);
        hash.Add(FontSize);
        hash.Add(FontFamily);
        hash.Add(FontStyle);
        hash.Add(FontWeight);
        hash.Add(FontStretch);
        hash.Add(Foreground);
        hash.Add(TextWrapping);
        var cacheKey = hash.ToHashCode();
        if (_layoutCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var wrap = TextWrapping == TextWrapping.NoWrap ? TextWrapping.NoWrap : TextWrapping.NoWrap;
        var layout = new TextLayout(
            renderSlice.SliceText,
            typeface,
            FontSize,
            Foreground,
            TextAlignment.Left,
            wrap,
            maxWidth: double.PositiveInfinity,
            maxHeight: _lineHeight);

        if (_layoutCache.Count >= LayoutCacheLimit)
        {
            foreach (var old in _layoutCache.Values)
                old.Dispose();
            _layoutCache.Clear();
        }

        _layoutCache[cacheKey] = layout;
        return layout;
    }

    private TextLayout GetLineNumberLayout(string text)
    {
        if (_lineNumberLayoutCache.TryGetValue(text, out var cached))
            return cached;

        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var layout = new TextLayout(text, typeface, FontSize, LineNumberBrush, TextAlignment.Right, TextWrapping.NoWrap);
        if (_lineNumberLayoutCache.Count >= NumberCacheLimit)
        {
            foreach (var old in _lineNumberLayoutCache.Values)
                old.Dispose();
            _lineNumberLayoutCache.Clear();
        }

        _lineNumberLayoutCache[text] = layout;
        return layout;
    }

    private void ClearLayoutCaches()
    {
        foreach (var layout in _layoutCache.Values)
            layout.Dispose();
        _layoutCache.Clear();

        foreach (var layout in _lineNumberLayoutCache.Values)
            layout.Dispose();
        _lineNumberLayoutCache.Clear();
        _renderSliceCache.Clear();
    }

    private void RestartCaretBlink()
    {
        _showCaret = true;
        _suspendCaretBlink = false;
        if (_surface.IsFocused)
        {
            _caretBlinkTimer.Stop();
            _caretBlinkTimer.Start();
        }
        _surface.InvalidateVisual();
    }

    private bool HasSelection => _caretIndex != _selectionAnchorIndex;

    private int FindPreviousCaretIndex(int index)
    {
        if (index <= 0)
            return 0;

        index--;
        if (index > 0 && char.IsLowSurrogate(_sourceText[index]) && char.IsHighSurrogate(_sourceText[index - 1]))
            index--;
        return index;
    }

    private int FindNextCaretIndex(int index)
    {
        if (index >= TextLength)
            return TextLength;

        index++;
        if (index < TextLength && char.IsLowSurrogate(_sourceText[index]) && char.IsHighSurrogate(_sourceText[index - 1]))
            index++;
        return Math.Clamp(index, 0, TextLength);
    }

    private int FindPreviousWordBoundary(int index)
    {
        index = Math.Clamp(index, 0, TextLength);
        if (index == 0)
            return 0;

        index = FindPreviousCaretIndex(index);
        while (index > 0 && char.IsWhiteSpace(_sourceText[index]))
            index = FindPreviousCaretIndex(index);
        while (index > 0 && !char.IsWhiteSpace(_sourceText[index - 1]))
            index = FindPreviousCaretIndex(index);
        return index;
    }

    private int FindNextWordBoundary(int index)
    {
        index = Math.Clamp(index, 0, TextLength);
        while (index < TextLength && !char.IsWhiteSpace(_sourceText[index]))
            index = FindNextCaretIndex(index);
        while (index < TextLength && char.IsWhiteSpace(_sourceText[index]))
            index = FindNextCaretIndex(index);
        return index;
    }

    private void PushInternalTextToStyledPropertyIfNeeded()
    {
        if (string.Equals(Text, _sourceText, StringComparison.Ordinal))
            return;

        _isUpdatingOuterText = true;
        try
        {
            SetCurrentValue(TextProperty, _sourceText);
        }
        finally
        {
            _isUpdatingOuterText = false;
        }
    }

    private void RaiseTextChanged() => TextChanged?.Invoke(this, EventArgs.Empty);

    private void RaiseCaretIndexChanged() => CaretIndexChanged?.Invoke(this, EventArgs.Empty);

    private void UpdatePlaceholderVisibility()
    {
        _placeholder.Text = PlaceholderText ?? string.Empty;
        _placeholder.IsVisible = string.IsNullOrEmpty(_sourceText) && !string.IsNullOrEmpty(_placeholder.Text);
    }

    private async Task CopyTextToClipboardAsync(string text)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
            await clipboard.SetTextAsync(text);
    }

    private string Sanitize(string? input)
    {
        input ??= string.Empty;
        if (input.Length == 0 || SanitizationMode == HugeTextSanitizationMode.None)
            return input;

        var sb = new StringBuilder(input.Length);
        var changed = false;

        for (var i = 0; i < input.Length;)
        {
            if (!Rune.TryGetRuneAt(input, i, out var rune))
            {
                sb.Append(ReplacementCharacter);
                i++;
                changed = true;
                continue;
            }

            var advance = rune.Utf16SequenceLength;
            if (ShouldReplaceRune(input, i, rune))
            {
                changed = true;
                for (var j = 0; j < advance; j++)
                    sb.Append(ReplacementCharacter);
            }
            else
            {
                sb.Append(input, i, advance);
            }

            i += advance;
        }

        return changed ? sb.ToString() : input;
    }

    private bool ShouldReplaceRune(string source, int index, Rune rune)
    {
        var scalar = rune.Value;

        if (scalar == '\r' || scalar == '\n' || scalar == '\t')
            return false;

        if (char.IsControl((char)Math.Min(char.MaxValue, scalar)))
            return true;

        if (Rune.IsControl(rune))
            return true;

        if (IsUnicodeNonCharacter(scalar) || IsBidirectionalControl(scalar))
            return true;

        if (SanitizationMode == HugeTextSanitizationMode.Aggressive && Rune.GetUnicodeCategory(rune) == System.Globalization.UnicodeCategory.Format)
            return true;

        if (rune.Utf16SequenceLength == 2)
        {
            if (index + 1 >= source.Length)
                return true;

            if (!char.IsSurrogatePair(source[index], source[index + 1]))
                return true;
        }

        return false;
    }

    private static bool IsBidirectionalControl(int scalar)
    {
        return scalar is 0x061C or 0x200E or 0x200F or >= 0x202A and <= 0x202E or >= 0x2066 and <= 0x2069;
    }

    private static bool IsUnicodeNonCharacter(int scalar)
    {
        if (scalar is >= 0xFDD0 and <= 0xFDEF)
            return true;

        return (scalar & 0xFFFE) == 0xFFFE;
    }

    private void LogInfo(Func<string> messageFactory)
    {
        if (!HugeTextBoxDiagnosticsSettings.Enabled)
            return;

        CommonLogger.LogInfo($"[HugeTextBox #{GetHashCode()} #{++_logSequence}] {messageFactory()}");
    }

    private void LogWarning(Func<string> messageFactory)
    {
        if (!HugeTextBoxDiagnosticsSettings.Enabled)
            return;

        CommonLogger.LogWarning($"[HugeTextBox #{GetHashCode()} #{++_logSequence}] {messageFactory()}");
    }

    private void LogError(Func<string> messageFactory)
    {
        if (!HugeTextBoxDiagnosticsSettings.Enabled)
            return;

        CommonLogger.LogError($"[HugeTextBox #{GetHashCode()} #{++_logSequence}] {messageFactory()}");
    }

    private static string DescribeBrush(IBrush? brush)
    {
        return brush switch
        {
            null => "<null>",
            ISolidColorBrush solid => solid.Color.ToString(),
            _ => brush.GetType().Name,
        };
    }

    private readonly record struct CaretHitInfo(int SourceOffset, BoundaryCaretAffinity Affinity);

    private readonly record struct RenderSliceInfo(int SliceStart, double BaseX, string SliceText);

    private readonly record struct DisplayLineInfo(int SourceStart, int SourceLength, int SourceLineIndex, bool IsFirstVisualSegmentOfSourceLine)
    {
        public static DisplayLineInfo Empty { get; } = new(0, 0, 0, true);
    }
}

public sealed class HugeTextSurface : Control
{
    public HugeTextBox? Owner { get; set; }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        Owner?.RenderSurface(context, this);
    }
}
