using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.WinUi.Lib.MethodsHelper;
using StegoRevealer.WinUi.Lib.ParamsHelpers;
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
    public partial class ChiSqrMethodParamsView : UserControl, ICollectableParamsView
    {
        public ChiSqrMethodParamsView()
        {
            InitializeComponent();

            IsUseUnifiedCathegoriesChecked.Checked += IsUseUnifiedCathegoriesChecked_Checked;
            IsUseUnifiedCathegoriesChecked.Unchecked += IsUseUnifiedCathegoriesChecked_Unchecked;
        }
        public ChiSqrMethodParamsView(ChiSqrParamsDto parameters) : this()
        {
            SetParameters(parameters);
        }


        private void SetParameters(ChiSqrParamsDto parameters)
        {
            IsVisualizeChecked.IsChecked = parameters.Visualize;
            IsExcludeZeroPairsChecked.IsChecked = parameters.ExcludeZeroPairs;
            IsUseUnifiedCathegoriesChecked.IsChecked = parameters.UseUnifiedCathegories;
            UnifyingCathegoriesThreshold.Value = parameters.UnifyingCathegoriesThreshold;
            IsChannelChecked_Red.IsChecked = parameters.Channels.Contains(ImgChannel.Red);
            IsChannelChecked_Green.IsChecked = parameters.Channels.Contains(ImgChannel.Green);
            IsChannelChecked_Blue.IsChecked = parameters.Channels.Contains(ImgChannel.Blue);
            TraverseChoice_Horizontal.IsChecked = parameters.TraverseType is TraverseType.Horizontal;
            TraverseChoice_Vertical.IsChecked = parameters.TraverseType is TraverseType.Vertical;
            PValueThreshold.Value = parameters.Threshold;
            BlockWidth.Value = parameters.BlockWidth;
            BlockHeight.Value = parameters.BlockHeight;
        }

        public object CollectParameters()
        {
            var result = new ChiSqrParamsDto();
            result.Visualize = IsVisualizeChecked.IsChecked is not null ? IsVisualizeChecked.IsChecked.Value : false;
            result.ExcludeZeroPairs = IsExcludeZeroPairsChecked.IsChecked is not null ? IsExcludeZeroPairsChecked.IsChecked.Value : false;
            result.UseUnifiedCathegories = IsUseUnifiedCathegoriesChecked.IsChecked is not null ? IsUseUnifiedCathegoriesChecked.IsChecked.Value : false;
            result.UnifyingCathegoriesThreshold = (int)UnifyingCathegoriesThreshold.Value;

            result.Channels = new();
            if (IsChannelChecked_Red.IsChecked.HasValue && IsChannelChecked_Red.IsChecked.Value)
                result.Channels.Add(ImgChannel.Red);
            if (IsChannelChecked_Green.IsChecked.HasValue && IsChannelChecked_Green.IsChecked.Value)
                result.Channels.Add(ImgChannel.Green);
            if (IsChannelChecked_Blue.IsChecked.HasValue && IsChannelChecked_Blue.IsChecked.Value)
                result.Channels.Add(ImgChannel.Blue);

            if (TraverseChoice_Vertical.IsChecked.HasValue && TraverseChoice_Horizontal.IsChecked.HasValue)
                result.TraverseType = (TraverseChoice_Vertical.IsChecked.Value && !TraverseChoice_Horizontal.IsChecked.Value 
                    ? TraverseType.Vertical 
                    : TraverseType.Horizontal);
            else
                result.TraverseType = TraverseType.Horizontal;

            result.Threshold = PValueThreshold.Value;
            result.BlockWidth = (int)BlockWidth.Value;
            result.BlockHeight = (int)BlockHeight.Value;

            return result;
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
