// VirtualTextViewer.cs — версия 3.1 с точным разбиением и безопасным выделением
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.Controls.Documents;
using Avalonia.Media.TextFormatting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using StegoRevealer.StegoCore.CommonLib.Exceptions;

namespace StegoRevealer.UI.Components
{
    public partial class VirtualTextViewer : UserControl
    {
        private ScrollViewer _scrollViewer;
        private Grid _virtualContainer;
        private Border _textHost;
        private TextBlock _textBlock;
        private Border _loadingOverlay;

        private string _fullText = string.Empty;
        private int _selectionStart = -1;
        private int _selectionEnd = -1;
        private bool _selectAllMode = false;
        private bool _isDragging = false;

        private List<(int start, int length)> _visualLines = new();
        private double _lineHeight = 16;
        private int _linesVisible = 25;

        private DispatcherTimer _autoScrollTimer;
        private Point _lastPointerPosition;

        public int ChunkSize { get; set; } = 10000;

        public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
            AvaloniaProperty.Register<VirtualTextViewer, TextWrapping>(nameof(TextWrapping));

        public new static readonly StyledProperty<bool> IsEnabledProperty =
            AvaloniaProperty.Register<VirtualTextViewer, bool>(nameof(IsEnabled), true);

        public new static readonly StyledProperty<double> FontSizeProperty =
            AvaloniaProperty.Register<VirtualTextViewer, double>(nameof(FontSize), 12);

        public TextWrapping TextWrapping
        {
            get => GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public new bool IsEnabled
        {
            get => GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        public new double FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public VirtualTextViewer()
        {
            InitializeComponent();

            _scrollViewer = this.FindControl<ScrollViewer>("PART_ScrollViewer") ?? throw new NullValueException("Missing PART_ScrollViewer"); ;
            _virtualContainer = this.FindControl<Grid>("PART_VirtualContainer") ?? throw new NullValueException("Missing PART_VirtualContainer"); ;
            _textHost = this.FindControl<Border>("PART_TextHost") ?? throw new NullValueException("Missing PART_TextHost"); ;
            _textBlock = this.FindControl<TextBlock>("PART_TextBlock") ?? throw new NullValueException("Missing PART_TextBlock"); ;
            _loadingOverlay = this.FindControl<Border>("PART_LoadingOverlay") ?? throw new NullValueException("Missing PART_LoadingOverlay"); ;

            _scrollViewer.ScrollChanged += OnScrollChanged;
            this.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            this.PointerPressed += OnPointerPressed;
            this.PointerReleased += OnPointerReleased;
            this.PointerMoved += OnPointerMoved;

            this.AttachedToVisualTree += (_, _) =>
            {
                _scrollViewer.GetObservable(BoundsProperty).Subscribe(_ => RecalculateMetricsAndUpdate());
                Focus();
            };

            _autoScrollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _autoScrollTimer.Tick += (_, _) => HandleAutoScroll();
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        private void OnScrollChanged(object? sender, ScrollChangedEventArgs e) => UpdateVisibleText();

        public async Task SetText(string text)
        {
            _fullText = text ?? string.Empty;
            _selectionStart = -1;
            _selectionEnd = -1;
            _selectAllMode = false;
            _visualLines.Clear();

            _loadingOverlay.IsVisible = true;

            var layout = new TextLayout(
                _fullText,
                new Typeface(_textBlock.FontFamily),
                _textBlock.FontSize,
                Brushes.Black,
                TextAlignment.Left,
                TextWrapping.Wrap,
                null, null,
                FlowDirection.LeftToRight,
                _textBlock.Bounds.Width,
                double.PositiveInfinity);

            await Task.Run(() =>
            {
                var lines = layout.TextLines
                    .Select(line => (line.FirstTextSourceIndex, line.Length))
                    .ToList();

                Dispatcher.UIThread.Invoke(() =>
                {
                    _visualLines = lines;
                    _virtualContainer.Height = _visualLines.Count * _lineHeight;
                    UpdateVisibleText();
                    _loadingOverlay.IsVisible = false;
                });
            });
        }

        private void RecalculateMetricsAndUpdate()
        {
            var viewport = _scrollViewer.Viewport;
            if (viewport.Width <= 0 || viewport.Height <= 0)
                return;

            _lineHeight = FontSize + 4;
            _linesVisible = Math.Max(1, (int)(viewport.Height / _lineHeight));

            _visualLines = CalculateVisualLines(_fullText);
            _virtualContainer.Height = _visualLines.Count * _lineHeight;

            UpdateVisibleText();
        }

        private List<(int start, int length)> CalculateVisualLines(string text)
        {
            var lines = new List<(int start, int length)>();
            if (string.IsNullOrEmpty(text))
                return lines;

            var layout = new TextLayout(
                text,
                new Typeface(_textBlock.FontFamily),
                _textBlock.FontSize,
                Brushes.Black,
                TextAlignment.Left,
                TextWrapping.Wrap,
                null, null,
                FlowDirection.LeftToRight,
                _textBlock.Bounds.Width,
                double.PositiveInfinity);

            foreach (var line in layout.TextLines)
            {
                int start = line.FirstTextSourceIndex;
                int length = line.Length;
                lines.Add((start, length));
            }

            return lines;
        }

        private void UpdateVisibleText()
        {
            if (_visualLines.Count == 0)
                return;

            int scrollLine = (int)(_scrollViewer.Offset.Y / _lineHeight);
            scrollLine = Math.Clamp(scrollLine, 0, Math.Max(0, _visualLines.Count - 1));

            int totalLength = 0;
            int firstChar = _visualLines[scrollLine].start;

            for (int i = scrollLine; i < Math.Min(scrollLine + _linesVisible, _visualLines.Count); i++)
                totalLength += _visualLines[i].length;

            string visible = _fullText.Substring(firstChar, Math.Min(totalLength, _fullText.Length - firstChar));

            if (string.Concat(_textBlock.Inlines?.OfType<Run>().Select(r => r.Text) ?? Enumerable.Empty<string>()) == visible && !_selectAllMode && _selectionStart == -1)
                return; // пропускаем, если текст тот же и нет выделения

            _textHost.Margin = new Thickness(0, scrollLine * _lineHeight, 0, 0);
            _textBlock.Inlines?.Clear();

            if (_selectAllMode || (_selectionStart >= 0 && _selectionEnd >= 0))
            {
                int globalStart = _selectAllMode ? 0 : Math.Min(_selectionStart, _selectionEnd);
                int globalEnd = _selectAllMode ? _fullText.Length : Math.Max(_selectionStart, _selectionEnd);

                int localStart = globalStart - firstChar;
                int localEnd = globalEnd - firstChar;

                int safeStart = Math.Clamp(Math.Min(localStart, localEnd), 0, visible.Length);
                int safeEnd = Math.Clamp(Math.Max(localStart, localEnd), 0, visible.Length);
                int safeLength = Math.Max(0, safeEnd - safeStart);

                string before = visible.Substring(0, safeStart);
                string selected = visible.Substring(safeStart, safeLength);
                string after = visible.Substring(safeStart + safeLength);

                if (!string.IsNullOrEmpty(before))
                    _textBlock.Inlines?.Add(new Run(before));
                if (!string.IsNullOrEmpty(selected))
                    _textBlock.Inlines?.Add(new Run(selected) { Background = Brushes.LightBlue });
                if (!string.IsNullOrEmpty(after))
                    _textBlock.Inlines?.Add(new Run(after));
            }
            else
            {
                _textBlock.Inlines?.Add(new Run(visible));
            }
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _isDragging = true;
            _lastPointerPosition = e.GetPosition(this);
            int index = GetCharIndexFromPoint(e.GetPosition(_textBlock));
            _selectionStart = _selectionEnd = index;
            _selectAllMode = false;
            _autoScrollTimer.Start();
            UpdateVisibleText();
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                _lastPointerPosition = e.GetPosition(this);
                int index = GetCharIndexFromPoint(e.GetPosition(_textBlock));
                _selectionEnd = index;
                UpdateVisibleText();
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
            _autoScrollTimer.Stop();
        }

        private async void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                if (e.Key == Key.A)
                {
                    _selectionStart = 0;
                    _selectionEnd = _fullText.Length;
                    _selectAllMode = true;
                    UpdateVisibleText();
                    e.Handled = true;
                }
                else if (e.Key == Key.C)
                {
                    var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                    int start = Math.Min(_selectionStart, _selectionEnd);
                    int end = Math.Max(_selectionStart, _selectionEnd);
                    if (start >= 0 && end > start && clipboard != null)
                    {
                        await clipboard.SetTextAsync(_fullText.Substring(start, end - start));
                        e.Handled = true;
                    }
                }
            }
        }

        private void HandleAutoScroll()
        {
            if (!_isDragging) return;

            double margin = 10;
            double speed = 20;

            if (_lastPointerPosition.Y < margin)
            {
                _scrollViewer.Offset = _scrollViewer.Offset.WithY(Math.Max(0, _scrollViewer.Offset.Y - speed));
            }
            else if (_lastPointerPosition.Y > _scrollViewer.Bounds.Height - margin)
            {
                _scrollViewer.Offset = _scrollViewer.Offset.WithY(_scrollViewer.Offset.Y + speed);
            }
        }

        private int GetCharIndexFromPoint(Point point)
        {
            string visibleText = string.Concat(_textBlock.Inlines?.OfType<Run>().Select(r => r.Text) ?? Enumerable.Empty<string>());
            if (string.IsNullOrEmpty(visibleText))
                return 0;

            var layout = new TextLayout(
                visibleText,
                new Typeface(_textBlock.FontFamily),
                _textBlock.FontSize,
                Brushes.Black,
                TextAlignment.Left,
                TextWrapping.Wrap,
                null, null,
                FlowDirection.LeftToRight,
                _textBlock.Bounds.Width,
                double.PositiveInfinity);

            var hit = layout.HitTestPoint(point);
            int localIndex = hit.TextPosition;

            int scrollLine = (int)(_scrollViewer.Offset.Y / _lineHeight);
            int firstChar = _visualLines[Math.Clamp(scrollLine, 0, _visualLines.Count - 1)].start;
            return firstChar + localIndex;
        }
    }
}