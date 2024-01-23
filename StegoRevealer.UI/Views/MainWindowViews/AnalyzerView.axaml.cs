using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Lib;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using System;
using System.Collections.Generic;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class AnalyzerView : UserControl
{
    private AnalyzerViewModel _vm = null!;

    public AnalyzerView()
    {
        InitializeComponent();

        base.Loaded += AnalyzerView_Loaded;
    }

    private void AnalyzerView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<AnalyzerViewModel>(this.DataContext);
        _vm.WindowResizeAction += UpdateImagePreview;
        _vm.WindowResizeAction();
    }

    private async void LoadImageButton_Click(object sender, RoutedEventArgs e)
    {
        ResetResultsExpander();  // В любом случае, сбрасываем форму результатов

        var successLoad = await _vm.TryLoadImage();
        if (successLoad)
            UpdateImagePreview();  // Отрисовка превью
    }

    private void MethodChiSqrParamsBtn_Click(object sender, RoutedEventArgs e) =>
            _vm.OpenParametersWindow(AnalyzerMethod.ChiSquare);

    private void MethodRsParamsBtn_Click(object sender, RoutedEventArgs e) =>
        _vm.OpenParametersWindow(AnalyzerMethod.RegularSingular);

    private void MethodKzaParamsBtn_Click(object sender, RoutedEventArgs e) =>
        _vm.OpenParametersWindow(AnalyzerMethod.KochZhaoAnalysis);

    private void StartAnalysis_Click(object sender, RoutedEventArgs e)
    {
        ResetResultsExpander();

        _vm.StartAnalysis();
        UpdateImagePreview();  // Отрисовка превью

        UpdateResults();
    }

    private void UpdateResults()
    {
        if (_vm.HasResults)
        {
            // Загрузка результатов
            var results = _vm.GetCurrentResults();
            if (results is null)
                return;

            // Вывод результатов на форму

            // ChiSqr
            if (results.IsMethodChiSqrExecuted)
                ChiFullnessValue.Text = string.Format("{0:P2}", results.ChiSqrMessageRelativeVolume);

            // RS
            if (results.IsMethodRsExecuted)
                RsFullnessValue.Text = string.Format("{0:P2}", results.RsMessageRelativeVolume);

            // Kzha
            if (results.IsMethodKzhaExecuted)
            {
                KzhaIntervalFoundedValue.Text = results.KzhaSuspiciousIntervalIsFound ? "Да" : "Нет";

                if (results.KzhaSuspiciousIntervalIsFound)
                {
                    KzhaBitsNumBlock.IsVisible = true;
                    KzhaSuspiciousIntervalBlock.IsVisible = true;
                    KzhaThresholdBlock.IsVisible = true;
                    KzhaCoeffsBlock.IsVisible = true;
                    KzhaExtractedDataBlock.IsVisible = true;

                    if (results.KzhaMessageBitsVolume > 0.0)
                    {
                        KzhaBitsNumBlock.IsEnabled = true;
                        KzhaBitsNumValue.Text = results.KzhaMessageBitsVolume.ToString();
                    }

                    if (results.KzhaThreshold > 0.0)
                    {
                        KzhaThresholdBlock.IsEnabled = true;
                        KzhaThresholdValue.Text = string.Format("{0:f2}", results.KzhaThreshold);
                    }

                    // Если порог или предполагаемое количество бит равно 0, то остальные данные явно неактуальны
                    bool kzhaHasRealData = results.KzhaMessageBitsVolume > 0.0 && results.KzhaThreshold > 0.0;

                    if (kzhaHasRealData && results.KzhaCoefficients is not null)
                    {
                        KzhaCoeffsBlock.IsEnabled = true;
                        KzhaCoeffsValue.Text = results.KzhaCoefficients.Value.ToString();
                    }

                    if (kzhaHasRealData && results.KzhaSuspiciousInterval is not null)
                    {
                        KzhaSuspiciousIntervalBlock.IsEnabled = true;
                        KzhaSuspiciousIntervalValue.Text =
                            $"[{results.KzhaSuspiciousInterval.Value.leftInd}, {results.KzhaSuspiciousInterval.Value.rightInd}]";
                    }

                    if (results.KzhaExtractedData is not null)
                    {
                        KzhaExtractedDataBlock.IsEnabled = true;
                        KzhaExtractedDataLabelValue.IsVisible = false;
                        KzhaExtractedDataValue.IsVisible = true;
                        KzhaExtractedDataValue.Text = results.KzhaExtractedData;
                    }
                }
            }

            // Затрачено времени
            ElapsedTimeValue.Text = results.ElapsedTime.ToString() + " мс";

            // Переключение экспандера
            ResultsExpander.IsExpanded = true;
        }
    }


    private void UpdateImagePreview() => ImagePreview.Source = _vm.DrawedImageSource;  // ?


    private void MethodsExpander_Expanded(object sender, RoutedEventArgs e)
    {
        ResultsExpander.ClearValue(RelativePanel.AlignTopWithProperty);
        ResultsExpander.IsExpanded = false;
        RightPanelSeparator.ClearValue(RelativePanel.BelowProperty);
        RightPanelSeparator.SetValue(RelativePanel.AboveProperty, ResultsExpander);
        MethodsExpander.SetValue(RelativePanel.AlignBottomWithProperty, RightPanelSeparator);

        //if (ResultsExpander is not null && MethodsExpander is not null)
        //    ResultsExpander.IsExpanded = !MethodsExpander.IsExpanded;
    }
    private void ResultsExpander_Expanded(object sender, RoutedEventArgs e)
    {
        MethodsExpander.ClearValue(RelativePanel.AlignBottomWithProperty);
        MethodsExpander.IsExpanded = false;
        RightPanelSeparator.ClearValue(RelativePanel.AboveProperty);
        RightPanelSeparator.SetValue(RelativePanel.BelowProperty, MethodsExpander);
        ResultsExpander.SetValue(RelativePanel.AlignTopWithProperty, RightPanelSeparator);

        //if (MethodsExpander is not null && ResultsExpander is not null && (_vm.HasResults ?? false))
        //    MethodsExpander.IsExpanded = !ResultsExpander.IsExpanded;
    }
    private void MethodsExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        if (!_vm.HasResults)
            MethodsExpander.IsExpanded = true;
        else
            ResultsExpander.IsExpanded = true;
    }
    private void ResultsExpander_Collapsed(object sender, RoutedEventArgs e) => MethodsExpander.IsExpanded = true;


    private void ResetResultsExpander()
    {
        _vm.HasResults = false;

        ChiFullnessBlock.IsEnabled = false;
        ChiFullnessValue.Text = "Анализ не проводился";
        RsFullnessBlock.IsEnabled = false;
        RsFullnessValue.Text = "Анализ не проводился";
        KzhaIntervalFoundedBlock.IsEnabled = false;
        KzhaIntervalFoundedValue.Text = "Анализ не проводился";
        KzhaBitsNumBlock.IsVisible = false;
        KzhaBitsNumBlock.IsEnabled = false;
        KzhaBitsNumValue.Text = "Нет данных";
        KzhaSuspiciousIntervalBlock.IsVisible = false;
        KzhaSuspiciousIntervalBlock.IsEnabled = false;
        KzhaSuspiciousIntervalValue.Text = "Нет данных";
        KzhaThresholdBlock.IsVisible = false;
        KzhaThresholdBlock.IsEnabled = false;
        KzhaThresholdValue.Text = "Нет данных";
        KzhaCoeffsBlock.IsVisible = false;
        KzhaCoeffsBlock.IsEnabled = false;
        KzhaCoeffsValue.Text = "Нет данных";
        KzhaExtractedDataBlock.IsVisible = false;
        KzhaExtractedDataBlock.IsEnabled = false;
        KzhaExtractedDataLabelValue.Text = "Отсутствует";
        KzhaExtractedDataValue.IsVisible = false;
        KzhaExtractedDataValue.Text = string.Empty;
        ElapsedTimeValue.Text = "0 мс";

        ResultsExpander.IsEnabled = false;
        ResultsExpander.IsExpanded = false;
        MethodsExpander.IsExpanded = true;
    }
}
