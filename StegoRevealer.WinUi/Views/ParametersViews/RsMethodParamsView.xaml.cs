using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.ImageHandlerLib;
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
    /// Логика взаимодействия для RsMethodParamsView.xaml
    /// </summary>
    public partial class RsMethodParamsView : UserControl
    {
        public RsMethodParamsView()
        {
            InitializeComponent();
        }
        public RsMethodParamsView(RsParamsDto parameters) : this()
        {
            SetParameters(parameters);
        }

        private void SetParameters(RsParamsDto parameters)
        {
            PixelsGroupLength.Value = parameters.PixelsGroupLength;

            string mask = string.Empty;
            foreach (int val in parameters.FlippingMask)
                mask += val.ToString();
            FlippingMask.Text = mask;

            IsChannelChecked_Red.IsChecked = parameters.Channels.Contains(ImgChannel.Red);
            IsChannelChecked_Green.IsChecked = parameters.Channels.Contains(ImgChannel.Green);
            IsChannelChecked_Blue.IsChecked = parameters.Channels.Contains(ImgChannel.Blue);
        }

        private RsParamsDto CollectParameters()
        {
            var result = new RsParamsDto();
            result.PixelsGroupLength = (int)PixelsGroupLength.Value;

            string currentNum = string.Empty;
            var mask = new List<int>();
            for (int i = 0; i < FlippingMask.Text.Length; i++)
            {
                currentNum += FlippingMask.Text[i];
                if (currentNum == "-")
                    continue;
                if (currentNum[..^0] == "0" || currentNum[..^0] == "1")
                    if (int.TryParse(currentNum, out int value))
                        mask.Add(value);
                currentNum = string.Empty;
            }
            result.FlippingMask = mask.ToArray();

            result.Channels = new();
            if (IsChannelChecked_Red.IsChecked.HasValue && IsChannelChecked_Red.IsChecked.Value)
                result.Channels.Add(ImgChannel.Red);
            if (IsChannelChecked_Green.IsChecked.HasValue && IsChannelChecked_Green.IsChecked.Value)
                result.Channels.Add(ImgChannel.Green);
            if (IsChannelChecked_Blue.IsChecked.HasValue && IsChannelChecked_Blue.IsChecked.Value)
                result.Channels.Add(ImgChannel.Blue);

            return result;
        }
    }
}
