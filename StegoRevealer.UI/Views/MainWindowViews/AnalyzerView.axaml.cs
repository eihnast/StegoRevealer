using Avalonia.Controls;
using Avalonia.Interactivity;
using StegoRevealer.UI.Lib;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using System;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class AnalyzerView : UserControl
{
    // Стандартные сообщения и заглушки
    private const string MessageNotAnalyzed = "Анализ не проводился";
    private const string MessageUnknown = "Нет данных";
    private const string MessageNotFoundData = "Отсутствует";
    private const string MessageNullElapsedTime = "0 мс";

    private AnalyzerViewModel _vm = null!;


    // Конструкторы и стартовые настройки
    public AnalyzerView()
    {
        InitializeComponent();
        base.Loaded += AnalyzerView_Loaded;
    }

    private void AnalyzerView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<AnalyzerViewModel>(this.DataContext);
        _vm.WindowResizeAction();  // Для изначальной установки MaxWidth и MaxHeight для изображения
        UpdateResults();
    }

    private async void LoadImageButton_Click(object sender, RoutedEventArgs e)
    {
        _vm.ResetResults();
        ResetResultsExpander();  // При попытке загрузке изображения в любом случае сбрасываем форму результатов
        await _vm.TryLoadImage();
    }


    // Запуск стегоанализа
    private async void StartAnalysis_Click(object sender, RoutedEventArgs e)
    {
        _vm.ResetResults();  // Сбрасываем результаты
        ResetResultsExpander();  // Сбрасываем форму результатов
        await _vm.StartAnalysis();  // Запускаем стегоанализ

        UpdateResults();  // Обновляем форму результатов
        _vm.IsMethodsOpened = false;  // Переключение экспандера
    }

    // Обновление результатов
    private void UpdateResults()
    {
        if (_vm.HasResults)
        {
            // Загрузка результатов
            var results = _vm.CurrentResults;
            if (results is null)
                return;

            // Вывод результатов на форму

            // ChiSqr
            if (results.IsMethodChiSqrExecuted)
                ChiFullnessValue.Text = string.Format("{0:P2}", results.ChiSqrMessageRelativeVolume);

            // RS
            if (results.IsMethodRsExecuted)
                RsFullnessValue.Text = string.Format("{0:P2}", Math.Min(1.0, results.RsMessageRelativeVolume));

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

            // Statm
            StatResultsNoise2Value.Text = string.Format("{0:F5}", results.StatmNoiseValueMethod2);
            StatResultsSharpnessValue.Text = string.Format("{0:F5}", results.StatmSharpnessValue);
            StatResultsBlurValue.Text = string.Format("{0:F5}", results.StatmBlurValue);
            StatResultsContrastValue.Text = string.Format("{0:F5}", results.StatmContrastValue);
            StatResultsEntropyTsallisValue.Text = string.Format("{0:F5}", results.StatmEntropyTsallisValue);
            StatResultsEntropyVaidaValue.Text = string.Format("{0:F5}", results.StatmEntropyVaidaValue);
            StatResultsEntropyShennonValue.Text = string.Format("{0:F5}", results.StatmEntropyShennonValue);
            StatResultsEntropyRenyiValue.Text = string.Format("{0:F5}", results.StatmEntropyRenyiValue);
            StatResultsEntropyHavardValue.Text = string.Format("{0:F5}", results.StatmEntropyHavardValue);


            // Затрачено времени
            ElapsedTimeValue.Text = results.ElapsedTime.ToString() + " мс";
        }
    }


    // Кнопки открытия параметров
    private async void MethodChiSqrParamsBtn_Click(object sender, RoutedEventArgs e) =>
        await _vm.OpenParametersWindow(AnalyzerMethod.ChiSquare);
    private async void MethodRsParamsBtn_Click(object sender, RoutedEventArgs e) =>
        await _vm.OpenParametersWindow(AnalyzerMethod.RegularSingular);
    private async void MethodKzaParamsBtn_Click(object sender, RoutedEventArgs e) =>
        await _vm.OpenParametersWindow(AnalyzerMethod.KochZhaoAnalysis);


    // Настройки экспандеров (выбор методов и результатов)
    private void MethodsExpander_Expanded(object sender, RoutedEventArgs e)
    {
        ResultsExpander.ClearValue(RelativePanel.AlignTopWithProperty);
        RightPanelSeparator.ClearValue(RelativePanel.BelowProperty);
        RightPanelSeparator.SetValue(RelativePanel.AboveProperty, ResultsExpander);
        MethodsExpander.SetValue(RelativePanel.AlignBottomWithProperty, RightPanelSeparator);
    }
    private void ResultsExpander_Expanded(object sender, RoutedEventArgs e)
    {
        MethodsExpander.ClearValue(RelativePanel.AlignBottomWithProperty);
        RightPanelSeparator.ClearValue(RelativePanel.AboveProperty);
        RightPanelSeparator.SetValue(RelativePanel.BelowProperty, MethodsExpander);
        ResultsExpander.SetValue(RelativePanel.AlignTopWithProperty, RightPanelSeparator);
    }
    private void MethodsExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        if (!_vm.HasResults)
            _vm.IsMethodsOpened = true;
    }
    private void ResultsExpander_Collapsed(object sender, RoutedEventArgs e) { }


    // Сброс результатов
    private void ResetResultsExpander()
    {
        // Переключение экспандера результатов
        _vm.IsMethodsOpened = true;

        // Сброс формы результатов
        CommonTools.SetFields("IsEnabled", false, 
            ChiFullnessBlock, RsFullnessBlock, KzhaIntervalFoundedBlock,
            KzhaBitsNumBlock, KzhaSuspiciousIntervalBlock, KzhaThresholdBlock, KzhaCoeffsBlock, KzhaExtractedDataBlock);

        CommonTools.SetFields("IsVisible", false, 
            KzhaBitsNumBlock, KzhaSuspiciousIntervalBlock, KzhaThresholdBlock, KzhaCoeffsBlock, KzhaExtractedDataBlock, KzhaExtractedDataValue);

        KzhaExtractedDataValue.Text = string.Empty;

        ChiFullnessValue.Text = MessageNotAnalyzed;
        RsFullnessValue.Text = MessageNotAnalyzed;
        KzhaIntervalFoundedValue.Text = MessageNotAnalyzed;
        KzhaBitsNumValue.Text = MessageUnknown;
        KzhaSuspiciousIntervalValue.Text = MessageUnknown;
        KzhaThresholdValue.Text = MessageUnknown;
        KzhaCoeffsValue.Text = MessageUnknown;
        StatResultsNoise2Value.Text = MessageUnknown;
        StatResultsSharpnessValue.Text = MessageUnknown;
        StatResultsBlurValue.Text = MessageUnknown;
        StatResultsContrastValue.Text = MessageUnknown;
        StatResultsEntropyTsallisValue.Text = MessageUnknown;
        StatResultsEntropyVaidaValue.Text = MessageUnknown;
        StatResultsEntropyShennonValue.Text = MessageUnknown;
        StatResultsEntropyRenyiValue.Text = MessageUnknown;
        StatResultsEntropyHavardValue.Text = MessageUnknown;
        KzhaExtractedDataLabelValue.Text = MessageNotFoundData;
        ElapsedTimeValue.Text = MessageNullElapsedTime;
    }
}
