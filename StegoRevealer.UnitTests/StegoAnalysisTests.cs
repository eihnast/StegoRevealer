﻿using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
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
            { "SA_img2.png", new ChiSquareResult { MessageRelativeVolume = 0.0013 } },
            { "SA_img3.png", new ChiSquareResult { MessageRelativeVolume = 0.2347 } },
            { "SA_img4.png", new ChiSquareResult { MessageRelativeVolume = 1.0 } },
            { "SA_img5.png", new ChiSquareResult { MessageRelativeVolume = 0.5833 } }
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
}
