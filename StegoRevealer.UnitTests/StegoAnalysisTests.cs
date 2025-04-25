using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.Fan;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Collections.Concurrent;

namespace StegoRevealer.StegoCore.ModuleTests;

[TestClass]
public class StegoAnalysisTests
{
    [TestMethod]
    public void StegoAnalysisCommonTest()
    {
        var imgNames = new List<string>() { "SA_img1.png", "SA_img2.png", "SA_img3.png", "SA_img4.png", "SA_img5.png" };
        var chiSquareResults = new ConcurrentDictionary<string, ChiSquareResult>();
        var rsResults = new ConcurrentDictionary<string, RsResult>();
        var kzhaResults = new ConcurrentDictionary<string, KzhaResult>();

        var imgAnalysisTasks = new List<Task>();
        foreach (var imgName in imgNames)
        {
            imgAnalysisTasks.Add(Task.Run(() =>
            {
                string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", imgName);
                var img = new ImageHandler(imagePath);

                var chiSqrAnalyser = new ChiSquareAnalyser(img);
                var rsAnalyser = new RsAnalyser(img);
                var kzhaAnalyser = new KzhaAnalyser(img);

                var analysisTasks = new List<Task>()
                {
                    Task.Run(() => chiSquareResults.AddOrUpdate(imgName, chiSqrAnalyser.Analyse(), (name, result) => result)),
                    Task.Run(() => rsResults.AddOrUpdate(imgName, rsAnalyser.Analyse(), (name, result) => result)),
                    Task.Run(() => kzhaResults.AddOrUpdate(imgName, kzhaAnalyser.Analyse(), (name, result) => result))
                };

                foreach (var task in analysisTasks)
                    task.Wait();
            }));
        }

        foreach (var task in imgAnalysisTasks)
            task.Wait();

        var chiSquareExpectedResults = new Dictionary<string, ChiSquareResult>()
        {
            { "SA_img1.png", new ChiSquareResult { MessageRelativeVolume = 0.9935 } },
            { "SA_img2.png", new ChiSquareResult { MessageRelativeVolume = 0.0 } },  // 0.0013
            { "SA_img3.png", new ChiSquareResult { MessageRelativeVolume = 0.1667 } },  // 0.2347
            { "SA_img4.png", new ChiSquareResult { MessageRelativeVolume = 1.0 } },
            { "SA_img5.png", new ChiSquareResult { MessageRelativeVolume = 0.55 } }  // 0.5833
        };
        var rsExpectedResults = new Dictionary<string, RsResult>()
        {
            { "SA_img1.png", new RsResult { MessageRelativeVolume = 0.218 } },
            { "SA_img2.png", new RsResult { MessageRelativeVolume = 0.5266 } },
            { "SA_img3.png", new RsResult { MessageRelativeVolume = 0.8348 } },
            { "SA_img4.png", new RsResult { MessageRelativeVolume = 0.4289 } },
            { "SA_img5.png", new RsResult { MessageRelativeVolume = 0.2121 } }
        };
        var kzhaExpectedResults = new Dictionary<string, KzhaResult>()
        {
            { "SA_img1.png", new KzhaResult { SuspiciousIntervalIsFound = false, MessageBitsVolume = 0, Threshold = 0.0 } },
            { "SA_img2.png", new KzhaResult { SuspiciousIntervalIsFound = true, MessageBitsVolume = 872, Threshold = 83.52 } },
            { "SA_img3.png", new KzhaResult { SuspiciousIntervalIsFound = true, MessageBitsVolume = 1776, Threshold = 78.29 } },
            { "SA_img4.png", new KzhaResult { SuspiciousIntervalIsFound = false, MessageBitsVolume = 0, Threshold = 0.0 } },
            { "SA_img5.png", new KzhaResult { SuspiciousIntervalIsFound = true, MessageBitsVolume = 792, Threshold = 81.19 } }
        };

        foreach (var imgName in imgNames)
        {
            Assert.AreEqual(chiSquareExpectedResults[imgName].MessageRelativeVolume, Math.Round(chiSquareResults[imgName].MessageRelativeVolume, 4));
            Assert.AreEqual(rsExpectedResults[imgName].MessageRelativeVolume, Math.Round(rsResults[imgName].MessageRelativeVolume, 4));
            Assert.AreEqual(kzhaExpectedResults[imgName].SuspiciousIntervalIsFound, kzhaResults[imgName].SuspiciousIntervalIsFound);
            if (kzhaExpectedResults[imgName].SuspiciousIntervalIsFound)
            {
                Assert.AreEqual(kzhaExpectedResults[imgName].MessageBitsVolume, kzhaResults[imgName].MessageBitsVolume);
                Assert.AreEqual(kzhaExpectedResults[imgName].Threshold, Math.Round(kzhaResults[imgName].Threshold, 2));
            }
        }
    }

    [TestMethod]
    public void SpaMethodTest()
    {
        var imgNames = new[] { "SPA_img1.png", "SPA_img2.png", "SPA_img3.png" };  // real: 52% 82% 0%
        var spaResults = new ConcurrentDictionary<string, SpaResult>();

        var imgAnalysisTasks = new List<Task>();
        foreach (var imgName in imgNames)
        {
            imgAnalysisTasks.Add(Task.Run(() =>
            {
                string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", imgName);
                var img = new ImageHandler(imagePath);
                var spaAnalyser = new SpaAnalyser(img);
                spaResults.TryAdd(imgName, spaAnalyser.Analyse());
            }));
        }
        Task.WaitAll(imgAnalysisTasks);

        var spaExpectedResults = new Dictionary<string, SpaResult>()
        {
            { 
                "SPA_img1.png", 
                new SpaResult 
                {
                    MessageRelativeVolume = 0.3684,
                    MessageRelativeVolumesByChannels = new()
                    {   { ImgChannel.Red, 0.3630 },
                        { ImgChannel.Green, 0.3830 },
                        { ImgChannel.Blue, 0.3590 }
                    }
                } 
            },
            {
                "SPA_img2.png",
                new SpaResult
                {
                    MessageRelativeVolume = 0.5745,
                    MessageRelativeVolumesByChannels = new()
                    {   { ImgChannel.Red, 0.5603 },
                        { ImgChannel.Green, 0.595 },
                        { ImgChannel.Blue, 0.5682 }
                    }
                }
            },
            {
                "SPA_img3.png",
                new SpaResult
                {
                    MessageRelativeVolume = 0.0102,
                    MessageRelativeVolumesByChannels = new()
                    {   { ImgChannel.Red, 0.0089 },
                        { ImgChannel.Green, 0.0027 },
                        { ImgChannel.Blue, 0.0191 }
                    }
                }
            }
        };

        Console.WriteLine("SPA results:");
        foreach (var imgName in imgNames)
        {
            Console.WriteLine($"Image: {imgName}");
            Console.WriteLine($"\tAvgHidedDataVolume: {spaResults[imgName].MessageRelativeVolume}");
            foreach (var channel in spaResults[imgName].MessageRelativeVolumesByChannels.Keys)
                Console.WriteLine($"\tChannel: {channel}, Volume: {spaResults[imgName].MessageRelativeVolumesByChannels[channel]}");
        }

        foreach (var imgName in imgNames)
        {
            Assert.AreEqual(spaExpectedResults[imgName].MessageRelativeVolume, Math.Round(spaResults[imgName].MessageRelativeVolume, 4));
            foreach (var channel in spaResults[imgName].MessageRelativeVolumesByChannels.Keys)
                Assert.AreEqual(spaExpectedResults[imgName].MessageRelativeVolumesByChannels[channel], Math.Round(spaResults[imgName].MessageRelativeVolumesByChannels[channel], 4));
        }
    }

    [TestMethod]
    public void ZcaMethodTest()
    {
        var imgNames = new[] { "ZCA_img1.png", "ZCA_img2.png", "ZCA_img3.png" };  // real: 52% 82% 0%
        var zcaResults = new ConcurrentDictionary<string, ZcaResult>();
        var zcaResultsWithOverall = new ConcurrentDictionary<string, ZcaResult>();

        var imgAnalysisTasks = new List<Task>();
        foreach (var imgName in imgNames)
        {
            imgAnalysisTasks.Add(Task.Run(() =>
            {
                string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", imgName);
                var img = new ImageHandler(imagePath);

                var zcaAnalyser = new ZcaAnalyser(img);
                zcaResultsWithOverall.TryAdd(imgName, zcaAnalyser.Analyse());

                //zcaAnalyser.Params.UseOverallCompression = false;
                //zcaResults.TryAdd(imgName, zcaAnalyser.Analyse());
            }));
        }
        Task.WaitAll(imgAnalysisTasks);

        var zcaExpectedResults = new Dictionary<string, ZcaResult>()
        {
            {
                "ZCA_img1.png",
                new ZcaResult
                {
                    IsHidingDetected = true,
                    IsHidedByChannels = new()
                    {   { ImgChannel.Red, true },
                        { ImgChannel.Green, true },
                        { ImgChannel.Blue, true }
                    }
                }
            },
            {
                "ZCA_img2.png",
                new ZcaResult
                {
                    IsHidingDetected = true,
                    IsHidedByChannels = new()
                    {   { ImgChannel.Red, true },
                        { ImgChannel.Green, true },
                        { ImgChannel.Blue, true }
                    }
                }
            },
            {
                "ZCA_img3.png",
                new ZcaResult
                {
                    IsHidingDetected = false,
                    IsHidedByChannels = new()
                    {   { ImgChannel.Red, false },
                        { ImgChannel.Green, false },
                        { ImgChannel.Blue, false }
                    }
                }
            }
        };

        //Console.WriteLine("ZCA results:");
        //foreach (var imgName in imgNames)
        //{
        //    Console.WriteLine($"Image: {imgName}");
        //    Console.WriteLine($"\tIsHidingDetected: {zcaResults[imgName].IsHidingDetected}");
        //    foreach (var channel in zcaResults[imgName].IsHidedByChannels.Keys)
        //        Console.WriteLine($"\tChannel: {channel}, Hided: {zcaResults[imgName].IsHidedByChannels[channel]}");
        //}
        Console.WriteLine("ZCA with overall analysis results:");
        foreach (var imgName in imgNames)
        {
            Console.WriteLine($"Image: {imgName}");
            Console.WriteLine($"\tIsHidingDetected: {zcaResultsWithOverall[imgName].IsHidingDetected}");
        }

        foreach (var imgName in imgNames)
        {
            Assert.AreEqual(zcaExpectedResults[imgName].IsHidingDetected, zcaResultsWithOverall[imgName].IsHidingDetected);
            //Assert.AreEqual(zcaExpectedResults[imgName].IsHidingDetected, zcaResults[imgName].IsHidingDetected);
            //foreach (var channel in zcaResults[imgName].IsHidedByChannels.Keys)
            //    Assert.AreEqual(zcaExpectedResults[imgName].IsHidedByChannels[channel], zcaResults[imgName].IsHidedByChannels[channel]);
        }
    }

    [TestMethod]
    public void FanMethodTest()
    {
        var imgNames = new[] { "FAN_img1.png", "FAN_img2.png", "FAN_img3.png" };  // real: 52% 82% 0%
        var fanResults = new ConcurrentDictionary<string, FanResult>();

        var imgAnalysisTasks = new List<Task>();
        foreach (var imgName in imgNames)
        {
            var task = Task.Run(() =>
            {
                string imagePath = Path.Combine(Helper.GetAssemblyDir(), "TestData", imgName);
                var img = new ImageHandler(imagePath);

                var fanAnalyser = new FanAnalyser(img);
                fanResults.TryAdd(imgName, fanAnalyser.Analyse());
            });
            task.Wait();
        }
        Task.WaitAll(imgAnalysisTasks);

        var fanExpectedResults = new Dictionary<string, FanResult>()
        {
            {
                "FAN_img1.png",
                new FanResult { IsHidingDetected = true }
            },
            {
                "FAN_img2.png",
                new FanResult { IsHidingDetected = true }
            },
            {
                "FAN_img3.png",
                new FanResult { IsHidingDetected = false }
            }
        };

        Console.WriteLine("FAN with overall analysis results:");
        foreach (var imgName in imgNames)
        {
            Console.WriteLine($"Image: {imgName}");
            Console.WriteLine($"\tIsHidingDetected: {fanResults[imgName].IsHidingDetected}");
        }

        foreach (var imgName in imgNames)
            Assert.AreEqual(fanExpectedResults[imgName].IsHidingDetected, fanResults[imgName].IsHidingDetected);
    }
}
