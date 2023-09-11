using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StegoRevealer.WinUi.ViewModels;

namespace StegoRevealer.WinUi.Views
{
    /// <summary>
    /// Логика взаимодействия для MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private MainViewModel _vm;

        #pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public MainView()
        {
            InitializeComponent();

            // TODO: Временно - авто-переход к стегоанализатору. Авто-переход отключён.
            // Loaded += (object sender, RoutedEventArgs e) => ToAnalyzerButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
        #pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

        public override void EndInit()
        {
            base.EndInit();
            var viewModel = this.DataContext as MainViewModel;  // Здесь DataContext = null, какая-то пост-привязка

            #pragma warning disable CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
            _vm = viewModel;
            #pragma warning restore CS8601 // Возможно, назначение-ссылка, допускающее значение NULL.
        }

        private void ToAnalyzerButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.SwitchToStegoAnalyzerView();
        }

        private void ToHiderButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.SwitchToHiderView();
        }

        private void ToExtractorButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.SwitchToExtractorView();
        }
    }
}
