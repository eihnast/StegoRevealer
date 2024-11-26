using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class ExtractorView : UserControl
{
    // Стандартные сообщения и заглушки
    private const string MessageNullElapsedTime = "0 мс";


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
        await _vm.TryLoadImage();
    }


    private void StartExtraction_Click(object sender, RoutedEventArgs e)
    {
        _vm.ResetResults();
        ResetResultsExpander();
        _vm.StartExtraction();

        UpdateResults();
        _vm.IsParamsOpened = false;
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
            ExtractedMessage.Text = CommonTools.FilterBadSymbols(results.ExtractedMessage);

            // Затрачено времени
            ElapsedTimeValue.Text = results.ElapsedTime.ToString() + " мс";
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
        ExtractedMessage.Text = string.Empty;
        ElapsedTimeValue.Text = MessageNullElapsedTime;
    }


    // Поведение переключателей
    private void SetLsbMethod(object? sender, RoutedEventArgs e) => _vm?.SelectLsbMethod();
    private void SetKzMethod(object? sender, RoutedEventArgs e) => _vm?.SelectKzMethod();

    private void SetLinearMode(object? sender, RoutedEventArgs e)
    {
        if (_vm is null || CommonTools.IsActionWhileTabChanged())
            return;

        _vm.SelectLinearMode();
        SetupFieldsForLinearMode();
    }
    private void SetRandomMode(object? sender, RoutedEventArgs e)
    {
        if (_vm is null || CommonTools.IsActionWhileTabChanged())
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
        Logger.LogInfo($"Raw extracted text saved to temp dir as '{filePath}'");

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
}
