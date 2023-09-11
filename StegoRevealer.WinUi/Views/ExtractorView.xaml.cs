using StegoRevealer.WinUi.Lib.Entities;
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

namespace StegoRevealer.WinUi.Views
{
    /// <summary>
    /// Логика взаимодействия для ExtractorView.xaml
    /// </summary>
    public partial class ExtractorView : UserControl
    {
        private ExtractorViewModel _vm;

        private const string FailedImagePathText = "Ошибка получения пути изображения";
        private const string ImageNotSelectedText = "Изображение не выбрано";


        #pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public ExtractorView()
        {
            InitializeComponent();
        }
        #pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

        public override void EndInit()
        {
            base.EndInit();
            var viewModel = this.DataContext as ExtractorViewModel;  // Здесь DataContext = null, какая-то пост-привязка
            ImagePathLabel.Text = ImageNotSelectedText;

            #pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
            _vm = viewModel;
            #pragma warning restore CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.

            MethodChoice_Lsb.Checked += MethodChoice_Lsb_Checked;
            MethodChoice_Lsb.Unchecked += MethodChoice_Lsb_Unchecked;
            MethodChoice_Kzh.Checked += MethodChoice_Kzh_Checked;
            MethodChoice_Kzh.Unchecked += MethodChoice_Kzh_Unchecked;
            HidewayChoice_Linear.Checked += HidewayChoice_Linear_Checked;
            HidewayChoice_Linear.Unchecked += HidewayChoice_Linear_Unchecked;
            HidewayChoice_Random.Checked += HidewayChoice_Random_Checked;
            HidewayChoice_Random.Unchecked += HidewayChoice_Random_Unchecked;

            MethodChoice_Lsb.RaiseEvent(new RoutedEventArgs(RadioButton.CheckedEvent));
            HidewayChoice_Linear.RaiseEvent(new RoutedEventArgs(RadioButton.CheckedEvent));
        }

        private void GoToMainViewBtn_Click(object sender, RoutedEventArgs e) => _vm.SwitchToMainView();

        private void LoadImageButton_Click(object sender, RoutedEventArgs e)
        {
            ResetResultsExpander();  // В любом случае, сбрасываем форму результатов

            var successLoad = _vm.TryLoadImage();
            if (successLoad)
            {
                ParamsGrid.IsEnabled = true;
                ImagePathLabel.Text = _vm.CurrentImage?.ImgPath ?? FailedImagePathText;

                UpdateImagePreview();  // Отрисовка превью
                ExtractedMessage.MaxHeight = ExtractedMessage.MaxHeight - 100;  // Обновление размера поля вывода текстового результата
            }
            else
            {
                ParamsGrid.IsEnabled = false;
                ImagePathLabel.Text = ImageNotSelectedText;
            }
        }

        private void ResetResultsExpander()
        {
            ResultsExpander.IsEnabled = false;
            ResultsExpander.IsExpanded = false;
            ParamsExpander.IsExpanded = true;
        }

        private void UpdateImagePreview() => ImagePreview.Source = _vm.DrawedImageSource;

        private void ParamsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (ResultsExpander is not null && ParamsExpander is not null)
                ResultsExpander.IsExpanded = !ParamsExpander.IsExpanded;
        }

        private void ResultsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (ParamsExpander is not null && ResultsExpander is not null && (_vm?.HasResults ?? false))
                ParamsExpander.IsExpanded = !ResultsExpander.IsExpanded;
        }

        private void StartExtraction_Click(object sender, RoutedEventArgs e)
        {
            var parameters = CollectParameters();
            _vm.SetParameters(parameters);
            _vm.StartExtraction();

            ResetResultsExpander();
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
                ExtractedMessage.Text = results.ExtractedMessage;
                ExtractedMessage.IsEnabled = !string.IsNullOrEmpty(results.ExtractedMessage);

                // Затрачено времени
                ElapsedTimeValue.Text = results.ElapsedTime.ToString() + " мс";

                // Переключение экспандера
                ResultsExpander.IsEnabled = true;
                ResultsExpander.IsExpanded = true;
            }
        }

        private ExtractionParams CollectParameters()
        {
            var parameters = new ExtractionParams();

            parameters.LsbExtration = MethodChoice_Lsb.IsChecked ?? false;
            // parameters.KzExtraction = MethodChoice_Kzh.IsChecked ?? false;
            parameters.LinearHided = HidewayChoice_Linear.IsChecked ?? false;
            // parameters.RandomHided = HidewayChoice_Random.IsChecked ?? false;

            parameters.LsbSeed = (int)LsbParamsGrid_RandomSeed.Value;
            parameters.LsbStartIndex = (int)LsbParamsGrid_StartIndex.Value;
            parameters.LsbByteLength = (int)LsbParamsGrid_ByteLength.Value;
            parameters.KzSeed = (int)KzhParamsGrid_RandomSeed.Value;
            parameters.KzIndexFirst = (int)KzhParamsGrid_IndexFirst.Value;
            parameters.KzIndexSecond = (int)KzhParamsGrid_IndexSecond.Value;
            parameters.KzThreshold = (int)KzhParamsGrid_Threshold.Value;

            return parameters;
        }

        private void KzhParamsGrid_RandomSeedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            KzhParamsGrid_RandomSeed.IsEnabled = true;
        }

        private void KzhParamsGrid_RandomSeedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            KzhParamsGrid_RandomSeed.IsEnabled = false;
        }

        private void KzhParamsGrid_IndexesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            KzhParamsGrid_IndexFirst.IsEnabled = true;
            KzhParamsGrid_IndexSecond.IsEnabled = true;
        }

        private void KzhParamsGrid_IndexesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            KzhParamsGrid_IndexFirst.IsEnabled = false;
            KzhParamsGrid_IndexSecond.IsEnabled = false;
        }

        private void KzhParamsGrid_ThresholdCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            KzhParamsGrid_Threshold.IsEnabled = true;
        }

        private void KzhParamsGrid_ThresholdCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            KzhParamsGrid_Threshold.IsEnabled = false;
        }

        private void LsbParamsGrid_RandomSeedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            LsbParamsGrid_RandomSeed.IsEnabled = true;
        }

        private void LsbParamsGrid_RandomSeedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            LsbParamsGrid_RandomSeed.IsEnabled = false;
        }

        private void LsbParamsGrid_StartIndexCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            LsbParamsGrid_StartIndex.IsEnabled = true;
        }

        private void LsbParamsGrid_StartIndexCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            LsbParamsGrid_StartIndex.IsEnabled = false;
        }

        private void LsbParamsGrid_ByteLengthCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            LsbParamsGrid_ByteLength.IsEnabled = true;
        }

        private void LsbParamsGrid_ByteLengthCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            LsbParamsGrid_ByteLength.IsEnabled = false;
        }

        private void MethodChoice_Lsb_Checked(object sender, RoutedEventArgs e)
        {
            LsbParamsGrid.Visibility = Visibility.Visible;
            KzhParamsGrid.Visibility = Visibility.Hidden;
        }

        private void MethodChoice_Lsb_Unchecked(object sender, RoutedEventArgs e)
        {
            KzhParamsGrid.Visibility = Visibility.Visible;
            LsbParamsGrid.Visibility = Visibility.Hidden;
        }

        private void MethodChoice_Kzh_Checked(object sender, RoutedEventArgs e)
        {
            KzhParamsGrid.Visibility = Visibility.Visible;
            LsbParamsGrid.Visibility = Visibility.Hidden;
        }

        private void MethodChoice_Kzh_Unchecked(object sender, RoutedEventArgs e)
        {
            LsbParamsGrid.Visibility = Visibility.Visible;
            KzhParamsGrid.Visibility = Visibility.Hidden;
        }

        private void HidewayChoice_Linear_Checked(object sender, RoutedEventArgs e)
        {
            LsbParamsGrid_RandomSeed.IsEnabled = false;
            LsbParamsGrid_RandomSeedCheckBox.IsChecked = false;
            LsbParamsGrid_RandomSeedCheckBox.IsEnabled = false;
            KzhParamsGrid_RandomSeed.IsEnabled = false;
            KzhParamsGrid_RandomSeedCheckBox.IsChecked = false;
            KzhParamsGrid_RandomSeedCheckBox.IsEnabled = false;
        }

        private void HidewayChoice_Linear_Unchecked(object sender, RoutedEventArgs e)
        {
            //LsbParamsGrid_RandomSeed.IsEnabled = true;
            LsbParamsGrid_RandomSeedCheckBox.IsEnabled = true;
            //KzhParamsGrid_RandomSeed.IsEnabled = true;
            KzhParamsGrid_RandomSeedCheckBox.IsEnabled = true;
        }

        private void HidewayChoice_Random_Checked(object sender, RoutedEventArgs e)
        {
            LsbParamsGrid_StartIndex.IsEnabled = false;
            LsbParamsGrid_StartIndexCheckBox.IsChecked = false;
            LsbParamsGrid_StartIndexCheckBox.IsEnabled = false;
            KzhParamsGrid_IndexFirst.IsEnabled = false;
            KzhParamsGrid_IndexSecond.IsEnabled = false;
            KzhParamsGrid_IndexesCheckBox.IsChecked = false;
            KzhParamsGrid_IndexesCheckBox.IsEnabled = false;
        }

        private void HidewayChoice_Random_Unchecked(object sender, RoutedEventArgs e)
        {
            //LsbParamsGrid_StartIndex.IsEnabled = true;
            LsbParamsGrid_StartIndexCheckBox.IsEnabled = true;
            //KzhParamsGrid_IndexFirst.IsEnabled = true;
            //KzhParamsGrid_IndexSecond.IsEnabled = true;
            KzhParamsGrid_IndexesCheckBox.IsEnabled = true;
        }
    }
}
