using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
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
    /// Логика взаимодействия для KzhaMethodParamsView.xaml
    /// </summary>
    public partial class KzhaMethodParamsView : UserControl
    {
        public List<ScIndexPair> Coefficients { get; set; }

        #pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public KzhaMethodParamsView()
        {
            MapAnalysisCoeffsToList();
            InitializeComponent();
            CoeffsList.DataContext = Coefficients;
        }
        #pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

        private void MapAnalysisCoeffsToList()
        {
            var coeffs = new List<ScIndexPair>();
            foreach (var field in typeof(HidingCoefficients).GetFields())
            {
                var value = field.GetValue(typeof(HidingCoefficients)) as ScIndexPair?;
                if (value is not null)
                    coeffs.Add(value.Value);
            }
            Coefficients = coeffs;
        }
    }
}
