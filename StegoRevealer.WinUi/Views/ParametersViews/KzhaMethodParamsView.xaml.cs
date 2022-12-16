using HandyControl.Tools.Extension;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using StegoRevealer.WinUi.Lib.MethodsHelper;
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
        public KzhaMethodParamsView(KzhaParamsDto parameters) : this()
        {
            SetParameters(parameters);
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

        private void SetParameters(KzhaParamsDto parameters)
        {
            TryToExtract.IsChecked = parameters.TryToExtract;
            CutCoefficient.Value = parameters.CutCoefficient;
            Threshold.Value = parameters.Threshold;
            TraverseChoice_Horizontal.IsChecked = !parameters.IsVerticalTraverse;
            TraverseChoice_Vertical.IsChecked = parameters.IsVerticalTraverse;
            IsChannelChecked_Red.IsChecked = parameters.Channels.Contains(ImgChannel.Red);
            IsChannelChecked_Green.IsChecked = parameters.Channels.Contains(ImgChannel.Green);
            IsChannelChecked_Blue.IsChecked = parameters.Channels.Contains(ImgChannel.Blue);
            foreach (var coeff in parameters.AnalysisCoeffs)
                CoeffsList.SelectedItems.Add(coeff);
        }

        private KzhaParamsDto CollectParameters()
        {
            var result = new KzhaParamsDto();
            result.TryToExtract = TryToExtract.IsChecked is not null ? TryToExtract.IsChecked.Value : false;
            result.CutCoefficient = CutCoefficient.Value;
            result.Threshold = Threshold.Value;

            if (TraverseChoice_Vertical.IsChecked.HasValue && TraverseChoice_Horizontal.IsChecked.HasValue)
                result.IsVerticalTraverse = TraverseChoice_Vertical.IsChecked.Value && !TraverseChoice_Horizontal.IsChecked.Value;
            else
                result.IsVerticalTraverse = false;

            result.Channels = new();
            if (IsChannelChecked_Red.IsChecked.HasValue && IsChannelChecked_Red.IsChecked.Value)
                result.Channels.Add(ImgChannel.Red);
            if (IsChannelChecked_Green.IsChecked.HasValue && IsChannelChecked_Green.IsChecked.Value)
                result.Channels.Add(ImgChannel.Green);
            if (IsChannelChecked_Blue.IsChecked.HasValue && IsChannelChecked_Blue.IsChecked.Value)
                result.Channels.Add(ImgChannel.Blue);

            result.AnalysisCoeffs = new();
            foreach (var item in CoeffsList.TransferredItems)
            {
                var coeff = item as ScIndexPair?;
                if (coeff is not null)
                    result.AnalysisCoeffs.Add(coeff.Value);
            }

            return result;
        }
    }
}
