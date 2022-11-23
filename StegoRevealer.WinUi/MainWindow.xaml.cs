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
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using System.IO;
using StegoRevealer.StegoCore;
using System.Globalization;

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
            TextBox1.Text += $"Изображение {handler.ImgPath} загружено\n";
            TextBox1.Text += $"{handler.ImgArray.Width}x{handler.ImgArray.Height}x{handler.ImgArray.Depth}\n";
            TextBox1.Text += "\n";

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

            //ChiSquareAnalyser chiSqr = new(handler);
            //var chiResult = chiSqr.Analyse(true);
            //TextBox1.Text += $"Относительный объём: {chiResult.MessageRelativeVolume}\n";
            //TextBox1.Text += $"Размер: {chiResult.MessageLength}\n";
            //TextBox1.Text += "\n";

            //var file = new StreamWriter("chi.log");
            //foreach (var logMsg in chiResult.LogRecords)
            //    file.WriteLine(logMsg.Message);
            //file.Close();


            //RsAnalyser rs = new(handler);
            //var rsResult = rs.Analyse(true);
            //TextBox1.Text += $"Относительный объём: {rsResult.MessageRelativeVolume}\n";
            //TextBox1.Text += "\n";

            //file = new StreamWriter("rs.log");
            //foreach (var logMsg in rsResult.LogRecords)
            //    file.WriteLine(logMsg.Message);
            //file.Close();


            //KzhaAnalyser kzha = new(handler);
            //var kzhaResult = kzha.Analyse(true);
            //TextBox1.Text += $"Подозрительный интервал: {kzhaResult.SuspiciousInterval}\n";
            //TextBox1.Text += $"Битовый размер скрытого сообщения: {kzhaResult.MessageBitsVolume}\n";
            //TextBox1.Text += $"Коэффициенты, в которых обнаружено скрытие: {kzhaResult.Coefficients}\n";
            //TextBox1.Text += $"Порог: {kzhaResult.Threshold}\n";
            //TextBox1.Text += $"Извлечённая информация: {kzhaResult.ExtractedData}\n";
            //TextBox1.Text += "\n";

            //file = new StreamWriter("kzha.log");
            //foreach (var logMsg in kzhaResult.LogRecords)
            //    file.WriteLine(logMsg.Message);
            //file.Close();


            // KOCH-ZHAO TESTS
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

            KzhaAnalyser kzha = new(handler);
            kzha.Params.LoggingCSequences = true;
            kzha.Params.AnalysisCoeffs = new() { new ScIndexPair(3, 6) };
            kzha.Params.IsVerticalTraverse = true;
            var kzhaResult = kzha.Analyse(true);
            TextBox1.Text += $"Подозрительный интервал: {kzhaResult.SuspiciousInterval}\n";
            TextBox1.Text += $"Битовый размер скрытого сообщения: {kzhaResult.MessageBitsVolume}\n";
            TextBox1.Text += $"Коэффициенты, в которых обнаружено скрытие: {kzhaResult.Coefficients}\n";
            TextBox1.Text += $"Порог: {kzhaResult.Threshold}\n";
            TextBox1.Text += $"Извлечённая информация: {kzhaResult.ExtractedData}\n";
            TextBox1.Text += "\n";

            var file = new StreamWriter("kzha2.log");
            foreach (var logMsg in kzhaResult.LogRecords)
                file.WriteLine(logMsg.Message);
            file.Close();


            // Проверка ДКП
            //byte[,] m = new byte[8, 8]
            //{
            //    { 154, 123, 123, 123, 123, 123, 123, 136 },
            //    { 192, 180, 136, 154, 154, 154, 136, 110 },
            //    { 254, 198, 154, 154, 180, 154, 123, 123 },
            //    { 239, 180, 136, 180, 180, 166, 123, 123 },
            //    { 180, 154, 136, 167, 166, 149, 136, 136 },
            //    { 128, 136, 123, 136, 154, 180, 198, 154 },
            //    { 123, 105, 110, 149, 136, 136, 180, 166 },
            //    { 110, 136, 123, 123, 123, 136, 154, 136 },
            //};
            //var dctM = KochZhaoCommon.DctBlock(m);
            //var idctM = KochZhaoCommon.IDctBlockAndNormalize(dctM);

            //TextBox1.Text += "DCT_M:\n";
            //for (int i = 0; i < 8; i++)
            //{
            //    string temp = "";
            //    for (int j = 0; j < 8; j++)
            //        temp += string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:f1}, ", dctM[i, j]);
            //    temp = temp[..^2] + "\n";
            //    TextBox1.Text += temp;
            //}

            //TextBox1.Text += "\n";

            //TextBox1.Text += "IDCT_M:\n";
            //for (int i = 0; i < 8; i++)
            //{
            //    string temp = "";
            //    for (int j = 0; j < 8; j++)
            //        temp += string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:f1}, ", idctM[i, j]);
            //    temp = temp[..^2] + "\n";
            //    TextBox1.Text += temp;
            //}


            // Сохранение файла
            // handler.Save("hidedTest");
            /*
             * Есть проблема. Она не решена. https://github.com/mono/SkiaSharp/issues/931
             * Если открыть оба изображения в Вивальди, они одинаковы. В просмоторщиках - разные. Что-то с кодеками?
             * Возможно, придётся поменять библиотеку. Возможно, забить. В целом, непонятно, но это не проблема методов и вычислений.
             */
        }
    }
}
