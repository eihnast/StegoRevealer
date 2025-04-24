using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using StegoRevealer.Common;
using StegoRevealer.StegoCore.AnalysisMethods;
using StegoRevealer.UI.Lib;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.MainWindowViewModels;
using System;

namespace StegoRevealer.UI.Views.MainWindowViews;

public partial class AnalyzerView : UserControl
{
    // Стандартные сообщения и заглушки
    private static string MessageNotAnalyzed = Constants.ResultsDefaults.NotAnalyzed;
    private static string MessageUnknown = Constants.ResultsDefaults.NoData;
    private static string MessageNotFoundData = Constants.ResultsDefaults.NotFoundData;
    private static string MessageNullElapsedTime = Constants.ResultsDefaults.NullElapsedTime + " " + Constants.ResultsDefaults.ElapsedTimeMeasure;
    private static string IsHidingDecisionCannotBeCalculated = Constants.ResultsDefaults.IsHidingDecisionCannotBeCalculated;

    private static SolidColorBrush BadTextBrush = CommonTools.GetBrush("SrDarkRed");
    private static SolidColorBrush GoodTextBrush = CommonTools.GetBrush("SrDarkGreen");
    private static SolidColorBrush DefaultTextBrush = CommonTools.GetBrush("SrDefaultWhite");

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

        // Установка текста блоков результатов
        AutoDetectionResultDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.HidingDesicionDetection);
        ChiFullnessDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.ChiSqrValue);
        RsFullnessDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.RsValue);
        SpaFullnessDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.SpaValue);
        ZcaFullnessDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.ZcaValue);
        KzhaIntervalFoundedDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.KzhaDetection);
        KzhaBitsNumDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.KzhaBitsNum);
        KzhaSuspiciousIntervalDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.KzhaIndexes);
        KzhaThresholdDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.KzhaThreshold);
        KzhaCoeffsDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.KzhaCoeffs);
        KzhaExtractedDataLabel.Text = Common.Tools.AddColon(Constants.ResultsNames.KzhaExtractedInfo);
        StatResultsTitleName.Text = Common.Tools.AddColon(Constants.ResultsNames.StatmLabel);
        StatResultsNoise2Desc.Text = Common.Tools.AddColon(Constants.ResultsNames.StatmNoise);
        StatResultsSharpnessDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.StatmSharpness);
        StatResultsBlurDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.StatmBlur);
        StatResultsContrastDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.StatmContrast);
        StatResultsEntropyShennonDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.StatmShennon);
        StatResultsEntropyRenyiDesc.Text = Common.Tools.AddColon(Constants.ResultsNames.StatmRenyi);
        ElapsedTimeLabel.Text = Common.Tools.AddColon(Constants.ResultsNames.ElapsedTime);
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
        StartAnalysis.IsEnabled = false;  // Блокириуем кнопку запуска СА

        _vm.ResetResults();  // Сбрасываем результаты
        ResetResultsExpander();  // Сбрасываем форму результатов
        await _vm.StartAnalysis();  // Запускаем стегоанализ

        UpdateResults();  // Обновляем форму результатов
        _vm.IsMethodsOpened = false;  // Переключение экспандера

        StartAnalysis.IsEnabled = true;  // Снимаем блокировку кнопки запуска СА
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
                ChiFullnessValue.Text = Common.Tools.GetValueAsPercents(results.ChiSqrMessageRelativeVolume);

            // RS
            if (results.IsMethodRsExecuted)
                RsFullnessValue.Text = Common.Tools.GetValueAsPercents(Math.Min(1.0, results.RsMessageRelativeVolume));

            // SPA
            if (results.IsMethodSpaExecuted)
                SpaFullnessValue.Text = Common.Tools.GetValueAsPercents(Math.Min(1.0, results.SpaMessageRelativeVolume));

            // ZCA
            if (results.IsMethodZcaExecuted)
            {
                string zcaHidingDetectedText = results.IsZcaHidingDetected ? Constants.ResultsDefaults.Detected : Constants.ResultsDefaults.NotDetected;
                var zcaHidingDetectedTextBrush = results.IsZcaHidingDetected ? BadTextBrush : GoodTextBrush;
                ZcaFullnessValue.Foreground = zcaHidingDetectedTextBrush;
                ZcaFullnessValue.Text = zcaHidingDetectedText;
            }

            // Kzha
            if (results.IsMethodKzhaExecuted)
            {
                KzhaIntervalFoundedValue.Text = results.KzhaSuspiciousIntervalIsFound ? Constants.ResultsDefaults.Yes : Constants.ResultsDefaults.No;

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
                        KzhaThresholdValue.Text = Common.Tools.GetFormattedDouble(results.KzhaThreshold);
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
            StatResultsNoise2Value.Text = Common.Tools.GetLongFormattedDouble(results.StatmNoiseValue);
            StatResultsSharpnessValue.Text = Common.Tools.GetLongFormattedDouble(results.StatmSharpnessValue);
            StatResultsBlurValue.Text = Common.Tools.GetLongFormattedDouble(results.StatmBlurValue);
            StatResultsContrastValue.Text = Common.Tools.GetLongFormattedDouble(results.StatmContrastValue);
            StatResultsEntropyShennonValue.Text = Common.Tools.GetLongFormattedDouble(results.StatmEntropyShennonValue);
            StatResultsEntropyRenyiValue.Text = Common.Tools.GetLongFormattedDouble(results.StatmEntropyRenyiValue);


            // Затрачено времени
            ElapsedTimeValue.Text = Common.Tools.GetElapsedTime(results.ElapsedTime);


            // Вывод о наличии встраивания
            if (results.IsComplexMethodExecuted)
            {
                string hidingDecisionText = results.IsHidingDetected ? Constants.ResultsDefaults.Detected : Constants.ResultsDefaults.NotDetected;
                var hidingDecisionTextBrush = results.IsHidingDetected ? BadTextBrush : GoodTextBrush;
                AutoDetectionResultValue.Foreground = hidingDecisionTextBrush;
                AutoDetectionResultValue.Text = hidingDecisionText;
            }
        }
    }


    // Кнопки открытия параметров
    private async void MethodChiSqrParamsBtn_Click(object sender, RoutedEventArgs e) =>
        await _vm.OpenParametersWindow(AnalysisMethod.ChiSquare);
    private async void MethodRsParamsBtn_Click(object sender, RoutedEventArgs e) =>
        await _vm.OpenParametersWindow(AnalysisMethod.RegularSingular);
    private async void MethodSpaParamsBtn_Click(object sender, RoutedEventArgs e) =>
        await _vm.OpenParametersWindow(AnalysisMethod.Spa);
    private async void MethodZcaParamsBtn_Click(object sender, RoutedEventArgs e) =>
        await _vm.OpenParametersWindow(AnalysisMethod.Zca);
    private async void MethodKzaParamsBtn_Click(object sender, RoutedEventArgs e) =>
        await _vm.OpenParametersWindow(AnalysisMethod.KochZhaoAnalysis);


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
    private void ResultsExpander_Collapsed(object sender, RoutedEventArgs e)
    { 
        // При текущей реализации действий на этот экшен не требуется
    }


    // Сброс результатов
    private void ResetResultsExpander()
    {
        // Переключение экспандера результатов
        _vm.IsMethodsOpened = true;

        // Сброс формы результатов
        CommonTools.SetFields("IsEnabled", false, 
            ChiFullnessBlock, RsFullnessBlock, SpaFullnessBlock, KzhaIntervalFoundedBlock,
            KzhaBitsNumBlock, KzhaSuspiciousIntervalBlock, KzhaThresholdBlock, KzhaCoeffsBlock, KzhaExtractedDataBlock);

        CommonTools.SetFields("IsVisible", false, 
            KzhaBitsNumBlock, KzhaSuspiciousIntervalBlock, KzhaThresholdBlock, KzhaCoeffsBlock, KzhaExtractedDataBlock, KzhaExtractedDataValue);

        KzhaExtractedDataValue.Text = string.Empty;

        ChiFullnessValue.Text = MessageNotAnalyzed;
        RsFullnessValue.Text = MessageNotAnalyzed;
        SpaFullnessValue.Text = MessageNotAnalyzed;
        ZcaFullnessValue.Foreground = DefaultTextBrush;
        ZcaFullnessValue.Text = MessageNotAnalyzed;
        KzhaIntervalFoundedValue.Text = MessageNotAnalyzed;
        KzhaBitsNumValue.Text = MessageUnknown;
        KzhaSuspiciousIntervalValue.Text = MessageUnknown;
        KzhaThresholdValue.Text = MessageUnknown;
        KzhaCoeffsValue.Text = MessageUnknown;
        StatResultsNoise2Value.Text = MessageUnknown;
        StatResultsSharpnessValue.Text = MessageUnknown;
        StatResultsBlurValue.Text = MessageUnknown;
        StatResultsContrastValue.Text = MessageUnknown;
        StatResultsEntropyShennonValue.Text = MessageUnknown;
        StatResultsEntropyRenyiValue.Text = MessageUnknown;
        KzhaExtractedDataLabelValue.Text = MessageNotFoundData;
        ElapsedTimeValue.Text = MessageNullElapsedTime;
        AutoDetectionResultValue.Foreground = DefaultTextBrush;
        AutoDetectionResultValue.Text = MessageNotAnalyzed;
    }
}
