using Microsoft.Win32;
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

using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.Lsb;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;

namespace StegoRevealer.WinUi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string path = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
            }
            TextBox1.Text += path + "\n";

            ImageHandler handler = new ImageHandler(path);
            TextBox1.Text += "loaded\n";
            TextBox1.Text += $"{handler.ImgArray.Width}x{handler.ImgArray.Height}x{handler.ImgArray.Depth}\n\n";


            // LSB TEST

            //LsbHider hiderLsb = new(handler);
            //var hideResultLsb = hiderLsb.Hide("TestData");
            //foreach (var s in hideResultLsb.AsLog().LogRecords)
            //    TextBox1.Text += $"{s}\n";
            //TextBox1.Text += $"\n";

            //var resultLsbPath = hideResultLsb.GetResultPath();
            //if (resultLsbPath != null)
            //{
            //    var newImgHandler = new ImageHandler(resultLsbPath);
            //    LsbExtractor ext = new(newImgHandler);
            //    var res = ext.Extract();
            //    string? resultData = res.GetResultData();
            //    TextBox1.Text += $"{resultData?[0..Math.Min(resultData.Length, 100)]}\n";
            //    foreach (var s in res.AsLog().LogRecords)
            //        TextBox1.Text += $"{s}\n";
            //    TextBox1.Text += $"\n";
            //}


            // KOCH-ZHAO TEST

            //KochZhaoHider hiderKz = new(handler);
            //var hideResultKz = hiderKz.Hide("TestData");
            //foreach (var s in hideResultKz.AsLog().LogRecords)
            //    TextBox1.Text += $"{s}\n";
            //TextBox1.Text += $"\n";

            //var resultKzPath = hideResultKz.GetResultPath();
            //if (resultKzPath != null)
            //{
            //    var newImgHandler = new ImageHandler(resultKzPath);
            //    KochZhaoExtractor ext = new(newImgHandler);
            //    ext.Params.Threshold = 0;
            //    ext.Params.ToExtractBitLength = 64;
            //    var res = ext.Extract();
            //    string? resultData = res.GetResultData();
            //    TextBox1.Text += $"{resultData?[0..Math.Min(resultData.Length, 100)]}\n";
            //    foreach (var s in res.AsLog().LogRecords)
            //        TextBox1.Text += $"{s}\n";
            //    TextBox1.Text += $"\n";
            //}


            // CHI-SQUARE TEST

            // var chiHandler = new ImageHandler(resultLsbPath);
            ChiSquareAnalyser chiSqr = new(handler);
            var chiResult = chiSqr.Analyse(true);
            var resultData = chiResult.MessageRelativeVolume;
            TextBox1.Text += $"Объём: {resultData}\n";
            foreach (var s in chiResult.AsLog().LogRecords)
                TextBox1.Text += $"{s}\n";
            TextBox1.Text += $"\n";
        }
    }
}
