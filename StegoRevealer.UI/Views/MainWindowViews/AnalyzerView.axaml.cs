using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
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
        AutoDetectionResultDesc.Text = CommonTools.AddColon(Constants.ResultsNames.HidingDesicionDetection);
        ChiFullnessDesc.Text = CommonTools.AddColon(Constants.ResultsNames.ChiSqrValue);
        RsFullnessDesc.Text = CommonTools.AddColon(Constants.ResultsNames.RsValue);
        KzhaIntervalFoundedDesc.Text = CommonTools.AddColon(Constants.ResultsNames.KzhaDetection);
        KzhaBitsNumDesc.Text = CommonTools.AddColon(Constants.ResultsNames.KzhaBitsNum);
        KzhaSuspiciousIntervalDesc.Text = CommonTools.AddColon(Constants.ResultsNames.KzhaIndexes);
        KzhaThresholdDesc.Text = CommonTools.AddColon(Constants.ResultsNames.KzhaThreshold);
        KzhaCoeffsDesc.Text = CommonTools.AddColon(Constants.ResultsNames.KzhaCoeffs);
        KzhaExtractedDataLabel.Text = CommonTools.AddColon(Constants.ResultsNames.KzhaExtractedInfo);
        StatResultsTitleName.Text = CommonTools.AddColon(Constants.ResultsNames.StatmLabel);
        StatResultsNoise2Desc.Text = CommonTools.AddColon(Constants.ResultsNames.StatmNoise);
        StatResultsSharpnessDesc.Text = CommonTools.AddColon(Constants.ResultsNames.StatmSharpness);
        StatResultsBlurDesc.Text = CommonTools.AddColon(Constants.ResultsNames.StatmBlur);
        StatResultsContrastDesc.Text = CommonTools.AddColon(Constants.ResultsNames.StatmContrast);
        StatResultsEntropyShennonDesc.Text = CommonTools.AddColon(Constants.ResultsNames.StatmShennon);
        StatResultsEntropyRenyiDesc.Text = CommonTools.AddColon(Constants.ResultsNames.StatmRenyi);
        ElapsedTimeLabel.Text = CommonTools.AddColon(Constants.ResultsNames.ElapsedTime);
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
                ChiFullnessValue.Text = CommonTools.GetValueAsPercents(results.ChiSqrMessageRelativeVolume);

            // RS
            if (results.IsMethodRsExecuted)
                RsFullnessValue.Text = CommonTools.GetValueAsPercents(Math.Min(1.0, results.RsMessageRelativeVolume));

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
                        KzhaThresholdValue.Text = CommonTools.GetFormattedDouble(results.KzhaThreshold);
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
            StatResultsNoise2Value.Text = CommonTools.GetLongFormattedDouble(results.StatmNoiseValue);
            StatResultsSharpnessValue.Text = CommonTools.GetLongFormattedDouble(results.StatmSharpnessValue);
            StatResultsBlurValue.Text = CommonTools.GetLongFormattedDouble(results.StatmBlurValue);
            StatResultsContrastValue.Text = CommonTools.GetLongFormattedDouble(results.StatmContrastValue);
            // StatResultsEntropyTsallisValue.Text = string.Format("{0:F5}", results.StatmEntropyTsallisValue);
            // StatResultsEntropyVaidaValue.Text = string.Format("{0:F5}", results.StatmEntropyVaidaValue);
            StatResultsEntropyShennonValue.Text = CommonTools.GetLongFormattedDouble(results.StatmEntropyShennonValue);
            StatResultsEntropyRenyiValue.Text = CommonTools.GetLongFormattedDouble(results.StatmEntropyRenyiValue);
            // StatResultsEntropyHavardValue.Text = string.Format("{0:F5}", results.StatmEntropyHavardValue);


            // Затрачено времени
            ElapsedTimeValue.Text = CommonTools.GetElapsedTime(results.ElapsedTime);


            // Вывод о наличии встраивания
            string hidingDecisionText = results.IsHidingDeceted is null ? IsHidingDecisionCannotBeCalculated :
                (results.IsHidingDeceted is true ? Constants.ResultsDefaults.Deceted : Constants.ResultsDefaults.NotDetected);
            var hidingDecisionTextBrush = results.IsHidingDeceted is null ? DefaultTextBrush :
                (results.IsHidingDeceted is true ? BadTextBrush : GoodTextBrush);
            AutoDetectionResultValue.Text = hidingDecisionText;
            AutoDetectionResultValue.Foreground = hidingDecisionTextBrush;
        }
    }


    // Кнопки открытия параметров
    private async void MethodChiSqrParamsBtn_Click(object sender, RoutedEventArgs e) =>
        await _vm.OpenParametersWindow(AnalysisMethod.ChiSquare);
    private async void MethodRsParamsBtn_Click(object sender, RoutedEventArgs e) =>
        await _vm.OpenParametersWindow(AnalysisMethod.RegularSingular);
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
        // StatResultsEntropyTsallisValue.Text = MessageUnknown;
        // StatResultsEntropyVaidaValue.Text = MessageUnknown;
        StatResultsEntropyShennonValue.Text = MessageUnknown;
        StatResultsEntropyRenyiValue.Text = MessageUnknown;
        // StatResultsEntropyHavardValue.Text = MessageUnknown;
        KzhaExtractedDataLabelValue.Text = MessageNotFoundData;
        ElapsedTimeValue.Text = MessageNullElapsedTime;
        AutoDetectionResultValue.Text = IsHidingDecisionCannotBeCalculated;
        AutoDetectionResultValue.Foreground = DefaultTextBrush;
    }
}
