using SkiaSharp;
using StegoRevealer.WinUi.Lib;
using StegoRevealer.WinUi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Accord.Math.Random;
using System.Windows.Media.Media3D;

namespace StegoRevealer.WinUi.Views
{
    /// <summary>
    /// Логика взаимодействия для StegoAnalyzerView.xaml
    /// </summary>
    public partial class StegoAnalyzerView : UserControl
    {
        private StegoAnalyzerViewModel _vm;

        private const string FailedImagePathText = "Ошибка получения пути изображения";
        private const string ImageNotSelectedText = "Изображение не выбрано";

        private Dictionary<AnalyzerMethod, bool> ActiveMethods { get; } = new();

        #pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public StegoAnalyzerView()
        {
            foreach (AnalyzerMethod method in Enum.GetValues(typeof(AnalyzerMethod)))
                ActiveMethods.Add(method, true);

            InitializeComponent();
        }
        #pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

        public override void EndInit()
        {
            base.EndInit();
            var viewModel = this.DataContext as StegoAnalyzerViewModel;  // Здесь DataContext = null, какая-то пост-привязка
            ImagePathLabel.Text = ImageNotSelectedText;

            #pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
            _vm = viewModel;
            #pragma warning restore CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
        }

        private void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            ResetResultsExpander();  // В любом случае, сбрасываем форму результатов

            var successLoad = _vm.TryLoadImage();
            if (successLoad)
            {
                MethodsSelectGrid.IsEnabled = true;
                ImagePathLabel.Text = _vm.CurrentImage?.ImgPath ?? FailedImagePathText;

                UpdateImagePreview();  // Отрисовка превью
            }
            else
            {
                MethodsSelectGrid.IsEnabled = false;
                ImagePathLabel.Text = ImageNotSelectedText;
            }
        }

        private void MethodChiSqrParamsBtn_Click(object sender, RoutedEventArgs e) =>
            _vm?.OpenParametersWindow(AnalyzerMethod.ChiSquare);

        private void MethodRsParamsBtn_Click(object sender, RoutedEventArgs e) =>
            _vm?.OpenParametersWindow(AnalyzerMethod.RegularSingular);

        private void MethodKzaParamsBtn_Click(object sender, RoutedEventArgs e) =>
            _vm?.OpenParametersWindow(AnalyzerMethod.KochZhaoAnalysis);

        private void StartAnalysis_Click(object sender, RoutedEventArgs e)
        {
            _vm?.StartAnalysis(ActiveMethods);
            UpdateImagePreview();  // Отрисовка превью

            ResetResultsExpander();
            UpdateResults();
        }

        private void UpdateResults()
        {
            if (_vm?.HasResults ?? false)
            {
                // Загрузка результатов
                var results = _vm?.GetCurrentResults();
                if (results is null)
                    return;

                // Вывод результатов на форму
                
                // ChiSqr
                if (results.IsMethodChiSqrExecuted)
                {
                    ChiFullnessBlock.IsEnabled = true;
                    ChiFullnessValue.Text = string.Format("{0:P2}", results.ChiSqrMessageRelativeVolume);
                }

                // RS
                if (results.IsMethodRsExecuted)
                {
                    RsFullnessBlock.IsEnabled = true;
                    RsFullnessValue.Text = string.Format("{0:P2}", results.RsMessageRelativeVolume);
                }

                // Kzha
                if (results.IsMethodKzhaExecuted)
                {
                    KzhaIntervalFoundedBlock.IsEnabled = true;
                    KzhaIntervalFoundedValue.Text = results.KzhaSuspiciousIntervalIsFound ? "Да" : "Нет";

                    if (results.KzhaSuspiciousIntervalIsFound)
                    {
                        KzhaBitsNumBlock.Visibility = Visibility.Visible;
                        KzhaSuspiciousIntervalBlock.Visibility = Visibility.Visible;
                        KzhaThresholdBlock.Visibility = Visibility.Visible;
                        KzhaCoeffsBlock.Visibility = Visibility.Visible;
                        KzhaExtractedDataBlock.Visibility = Visibility.Visible;

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
                            KzhaExtractedDataLabelValue.Visibility = Visibility.Hidden;
                            KzhaExtractedDataValue.Visibility = Visibility.Visible;
                            KzhaExtractedDataValue.Text = results.KzhaExtractedData;
                        }
                    }
                }

                // Затрачено времени
                ElapsedTimeValue.Text = results.ElapsedTime.ToString() + " мс";

                // Переключение экспандера
                ResultsExpander.IsEnabled = true;
                ResultsExpander.IsExpanded = true;
            }
        }

        private void IsMethodChiSqrChecked_Checked(object sender, RoutedEventArgs e) =>
            ActiveMethods[AnalyzerMethod.ChiSquare] = true;

        private void IsMethodChiSqrChecked_Unchecked(object sender, RoutedEventArgs e) =>
            ActiveMethods[AnalyzerMethod.ChiSquare] = false;

        private void IsMethodRsChecked_Checked(object sender, RoutedEventArgs e) =>
            ActiveMethods[AnalyzerMethod.RegularSingular] = true;

        private void IsMethodRsChecked_Unchecked(object sender, RoutedEventArgs e) =>
            ActiveMethods[AnalyzerMethod.RegularSingular] = false;

        private void IsMethodKzaChecked_Checked(object sender, RoutedEventArgs e) =>
            ActiveMethods[AnalyzerMethod.KochZhaoAnalysis] = true;

        private void IsMethodKzaChecked_Unchecked(object sender, RoutedEventArgs e) =>
            ActiveMethods[AnalyzerMethod.KochZhaoAnalysis] = false;


        private void UpdateImagePreview() => ImagePreview.Source = _vm?.DrawedImageSource;

        private void MethodsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (ResultsExpander is not null && MethodsExpander is not null)
                ResultsExpander.IsExpanded = !MethodsExpander.IsExpanded;
        }

        private void ResultsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (MethodsExpander is not null && ResultsExpander is not null && (_vm?.HasResults ?? false))
                MethodsExpander.IsExpanded = !ResultsExpander.IsExpanded;
        }

        private void ResetResultsExpander()
        {
            ChiFullnessBlock.IsEnabled = false;
            ChiFullnessValue.Text = "Анализ не проводился";
            RsFullnessBlock.IsEnabled = false;
            RsFullnessValue.Text = "Анализ не проводился";
            KzhaIntervalFoundedBlock.IsEnabled = false;
            KzhaIntervalFoundedValue.Text = "Анализ не проводился";
            KzhaBitsNumBlock.Visibility = Visibility.Hidden;
            KzhaBitsNumBlock.IsEnabled = false;
            KzhaBitsNumValue.Text = "Нет данных";
            KzhaSuspiciousIntervalBlock.Visibility = Visibility.Hidden;
            KzhaSuspiciousIntervalBlock.IsEnabled = false;
            KzhaSuspiciousIntervalValue.Text = "Нет данных";
            KzhaThresholdBlock.Visibility = Visibility.Hidden;
            KzhaThresholdBlock.IsEnabled = false;
            KzhaThresholdValue.Text = "Нет данных";
            KzhaCoeffsBlock.Visibility = Visibility.Hidden;
            KzhaCoeffsBlock.IsEnabled = false;
            KzhaCoeffsValue.Text = "Нет данных";
            KzhaExtractedDataBlock.Visibility = Visibility.Hidden;
            KzhaExtractedDataBlock.IsEnabled = false;
            KzhaExtractedDataLabelValue.Text = "Отсутствует";
            KzhaExtractedDataValue.Visibility = Visibility.Hidden;
            KzhaExtractedDataValue.Text = string.Empty;
            ElapsedTimeValue.Text = "0 мс";

            ResultsExpander.IsEnabled = false;
            ResultsExpander.IsExpanded = false;
            MethodsExpander.IsExpanded = true;
        }

        private void GoToMainViewBtn_Click(object sender, RoutedEventArgs e) => _vm.SwitchToMainView();
    }
}
