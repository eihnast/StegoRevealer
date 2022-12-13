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

namespace StegoRevealer.WinUi.Views.ParametersViews
{
    /// <summary>
    /// Логика взаимодействия для ChiSqrMethodParamsView.xaml
    /// </summary>
    public partial class ChiSqrMethodParamsView : UserControl
    {
        public ChiSqrMethodParamsView()
        {
            InitializeComponent();

            IsUseUnifiedCathegoriesChecked.Checked += IsUseUnifiedCathegoriesChecked_Checked;
            IsUseUnifiedCathegoriesChecked.Unchecked += IsUseUnifiedCathegoriesChecked_Unchecked;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TitleLabel.Content = string.Empty;
        }

        private void IsUseUnifiedCathegoriesChecked_Unchecked(object sender, RoutedEventArgs e)
        {
            UnifyingCathegoriesThresholdGroup.IsEnabled = false;
        }

        private void IsUseUnifiedCathegoriesChecked_Checked(object sender, RoutedEventArgs e)
        {
            UnifyingCathegoriesThresholdGroup.IsEnabled = true;
        }
    }
}
