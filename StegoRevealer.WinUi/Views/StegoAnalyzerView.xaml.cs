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

        #pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public StegoAnalyzerView()
        {
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
            var successLoad = _vm.TryLoadImage();
            if (successLoad)
            {
                MethodsSelectGrid.IsEnabled = true;
                ImagePathLabel.Text = _vm.CurrentImage?.ImgPath ?? FailedImagePathText;
            }
            else
            {
                MethodsSelectGrid.IsEnabled = false;
                ImagePathLabel.Text = ImageNotSelectedText;
            }
        }

        private void MethodChiSqrParamsBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm.OpenParametersWindow(AnalyzerMethod.ChiSquare);
        }

        private void MethodRsParamsBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm.OpenParametersWindow(AnalyzerMethod.RegularSingular);
        }

        private void MethodKzaParamsBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm.OpenParametersWindow(AnalyzerMethod.KochZhaoAnalysis);
        }
    }
}
