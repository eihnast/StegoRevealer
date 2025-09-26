using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using StegoRevealer.Common;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class ExtractorView : UserControl
{
    // Стандартные сообщения и заглушки
    private string MessageNullElapsedTime = "0 мс";


    private ExtractorViewModel _vm = null!;

    public ExtractorView()
    {
        InitializeComponent();
        base.Loaded += ExtractorView_Loaded;
    }

    private void ExtractorView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<ExtractorViewModel>(this.DataContext);
        _vm.WindowResizeAction();  // Для изначальной установки MaxWidth и MaxHeight для изображения

        MessageNullElapsedTime = "0 " + _vm.L["Common.Ms"];

        SetImagePathText();

        if (_vm.LinearModeSelected)
            SetupFieldsForLinearMode();
        else if (_vm.RandomModeSelected)
            SetupFieldsForRandomMode();
        UpdateResults();
    }

    private async void LoadImageButton_Click(object sender, RoutedEventArgs e)
    {
        _vm.ResetResults();
        ResetResultsExpander();  // При попытке загрузке изображения в любом случае сбрасываем форму результатов
        ResetImagePathText();

        await _vm.TryLoadImage();
        SetImagePathText();
    }


    private async void StartExtraction_Click(object sender, RoutedEventArgs e)
    {
        StartExtraction.IsEnabled = false;  // Блокириуем кнопку запуска СА
        LoadImageButton.IsEnabled = false;  // Блокируем кнопку выбора изображения
        ParamsExpander.IsEnabled = false;  // Блокируем всю панель выбора методов
        LoadingOverlay.IsVisible = true;
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

        _vm.ResetResults();
        ResetResultsExpander();
        await Task.Run(_vm.StartExtraction);

        UpdateResults();
        _vm.IsParamsOpened = false;

        LoadingOverlay.IsVisible = false;
        LoadImageButton.IsEnabled = true;  // Снимаем блокировку кнопки выбора изображения
        ParamsExpander.IsEnabled = true;  // Снимаем блокировку всей панели выбора методов
        StartExtraction.IsEnabled = true;  // Снимаем блокировку кнопки запуска СА
    }

    private void UpdateResults()
    {
        if (_vm.HasResults)
        {
            // Загрузка результатов
            var results = _vm.CurrentResults;
            if (results is null)
                return;

            // Вывод результатов на форму
            ExtractedMessage.SetText(CommonTools.FilterBadSymbols(results.ExtractedMessage));
            Dispatcher.UIThread.Post(() => { ExtractedMessage.UpdateLayout(); });

            // Затрачено времени
            ElapsedTimeValue.Text = results.ElapsedTime.ToString() + " " + _vm.L["Common.Ms"];

            ExtractedMessage.ScrollToTop();
        }
    }


    // Настройки экспандеров (выбора параметров и результатов)
    private void ParamsExpander_Expanded(object sender, RoutedEventArgs e)
    {
        ResultsExpander.ClearValue(RelativePanel.AlignTopWithProperty);
        RightPanelSeparator.ClearValue(RelativePanel.BelowProperty);
        RightPanelSeparator.SetValue(RelativePanel.AboveProperty, ResultsExpander);
        ParamsExpander.SetValue(RelativePanel.AlignBottomWithProperty, RightPanelSeparator);
    }
    private void ResultsExpander_Expanded(object sender, RoutedEventArgs e)
    {
        ParamsExpander.ClearValue(RelativePanel.AlignBottomWithProperty);
        RightPanelSeparator.ClearValue(RelativePanel.AboveProperty);
        RightPanelSeparator.SetValue(RelativePanel.BelowProperty, ParamsExpander);
        ResultsExpander.SetValue(RelativePanel.AlignTopWithProperty, RightPanelSeparator);
    }
    private void ParamsExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        if (!_vm.HasResults)
            _vm.IsParamsOpened = true;
    }
    private void ResultsExpander_Collapsed(object sender, RoutedEventArgs e) => ParamsExpander.IsExpanded = true;


    // Сброс результатов
    private void ResetResultsExpander()
    {
        // Переключение экспандера результатов
        _vm.IsParamsOpened = true;

        // Сброс формы результатов
        ExtractedMessage.SetText(string.Empty);
        ElapsedTimeValue.Text = MessageNullElapsedTime;
    }


    // Поведение переключателей
    private void SetLsbMethod(object? sender, RoutedEventArgs e) => _vm?.SelectLsbMethod();
    private void SetKzMethod(object? sender, RoutedEventArgs e) => _vm?.SelectKzMethod();

    private void SetLinearMode(object? sender, RoutedEventArgs e)
    {
        if (_vm is null || Common.Tools.IsActionWhileTabChanged())
            return;

        _vm.SelectLinearMode();
        SetupFieldsForLinearMode();
    }
    private void SetRandomMode(object? sender, RoutedEventArgs e)
    {
        if (_vm is null || Common.Tools.IsActionWhileTabChanged())
            return;

        _vm.SelectRandomMode();
        SetupFieldsForRandomMode();
    }

    private void SetupFieldsForLinearMode()
    {
        _vm.LsbRandomSeedSelected = false;
        LsbParamsGrid_RandomSeedCheckBox.IsEnabled = false;
        _vm.KzRandomSeedSelected = false;
        KzhParamsGrid_RandomSeedCheckBox.IsEnabled = false;

        LsbParamsGrid_StartIndexCheckBox.IsEnabled = true;
        KzhParamsGrid_IndexesCheckBox.IsEnabled = true;
    }
    private void SetupFieldsForRandomMode()
    {
        LsbParamsGrid_RandomSeedCheckBox.IsEnabled = true;
        KzhParamsGrid_RandomSeedCheckBox.IsEnabled = true;

        _vm.LsbStartIndexSelected = false;
        LsbParamsGrid_StartIndexCheckBox.IsEnabled = false;
        _vm.KzIndexesSelected = false;
        KzhParamsGrid_IndexesCheckBox.IsEnabled = false;
    }

    private void OpenExtractedText_Click(object? sender, RoutedEventArgs e)
    {
        if (!_vm.HasResults || string.IsNullOrEmpty(_vm.CurrentResults?.ExtractedMessage))
            return;

        var rnd = new Random();
        string tempDir = Path.GetTempPath();
        string fileName = $"SR_ExtractedTemp_{DateTime.Now:yy-MM-dd-HH-mm-ss}_{rnd.Next(0, 100)}.txt";
        string filePath = Path.Combine(tempDir, fileName);

        File.WriteAllText(filePath, _vm.CurrentResults.ExtractedMessage);
        CommonLogger.LogInfo($"Raw extracted text saved to temp dir as '{filePath}'");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            }
        };
        process.Start();
    }

    private async void SaveExtractedText_Click(object? sender, RoutedEventArgs e) => await _vm.TrySaveExtractedText();

    private void SetImagePathText()
    {
        if (string.IsNullOrEmpty(_vm.ImagePath))
            ResetImagePathText();
        else
            BindImagePathText();
    }
    private void RemoveImagePathBinding() => BindingOperations.GetBindingExpressionBase(ImagePathLabel, TextBox.TextProperty)?.Dispose();
    private void ResetImagePathText()
    {
        RemoveImagePathBinding();
        ImagePathLabel.Bind(TextBox.TextProperty, new Binding
        {
            Source = _vm,
            Path = "L[Common.ImageNotSelected]",
            Mode = BindingMode.TwoWay
        });
    }
    private void BindImagePathText()
    {
        RemoveImagePathBinding();
        ImagePathLabel.Bind(TextBox.TextProperty, new Binding
        {
            Source = _vm,
            Path = "ImagePath",
            Mode = BindingMode.TwoWay
        });
    }
}
