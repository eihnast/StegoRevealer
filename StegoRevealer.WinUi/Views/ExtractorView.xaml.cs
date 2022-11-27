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
        private ExtractorViewModel? _vm;

        public ExtractorView()
        {
            InitializeComponent();
        }

        public override void EndInit()
        {
            base.EndInit();
            _vm = this.DataContext as ExtractorViewModel;
        }
    }
}
