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
    /// Логика взаимодействия для MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private MainViewModel? _vm;

        public MainView()
        {
            InitializeComponent();
        }

        public override void EndInit()
        {
            base.EndInit();
            _vm = this.DataContext as MainViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (_vm is null)
                return;
            else
                _vm.Btn_Click();
        }
    }
}
