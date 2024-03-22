using StegoRevealer.StegoCore.CommonLib;
using System.Collections;

namespace StegoRevealer.Utils.DataPreparer.Lib;

public static class TextDataHelper
{
    public class RandomData
    {
        public string Data { get; set; } = string.Empty;  // Выбранные рандомно данные
        public int StartIndex { get; set; }  // Стартовый индекс начала выборки из изначальных данных
        public int? RandomLength { get; set; }  // Сгенерированный рандомно размер (в битах или символах)
    }

    private static Dictionary<string, string> _loadedFiles = new Dictionary<string, string>();
    private static Dictionary<string, BitArray> _loadedBitArrays = new Dictionary<string, BitArray>();

    private static string? GetOrLoadText(string dataPath)
    {
        try
        {
            if (!_loadedFiles.ContainsKey(dataPath))
                _loadedFiles.Add(dataPath, File.ReadAllText(dataPath));
            return _loadedFiles[dataPath];
        }
        catch
        {
            return null;
        }
    }

    private static BitArray? GetOrCreateBitArray(string dataPath)
    {
        try
        {
            if (!_loadedBitArrays.ContainsKey(dataPath))
            {
                var textData = GetOrLoadText(dataPath);
                if (string.IsNullOrEmpty(textData))
                    throw new Exception("TextData is null");

                _loadedBitArrays.Add(dataPath, StringBitsTools.StringToBitArray(textData));
            }
            return _loadedBitArrays[dataPath];
        }
        catch
        {
            return null;
        }
    }

    public static void InitializeTextFile(string dataPath) => _ = GetOrCreateBitArray(dataPath);


    public static RandomData? GetRandomDataPart(string dataPath, int length)
    {
        var textData = GetOrLoadText(dataPath);
        if (textData is null)
            return null;

        var rnd = new Random();
        int textDataLength = textData.Length;

        int startIndex = rnd.Next(0, Math.Max(textDataLength - length, 0));

        try
        {
            var selectedData = textData[startIndex..(startIndex + length)];
            return new RandomData
            {
                Data = selectedData,
                StartIndex = startIndex
            };
        }
        catch
        {
            return null;
        }
    }

    public static RandomData? GetRandomDataPartWithRandomLength(string dataPath, int? minLength = null, int? maxLength = null)
    {
        var textData = GetOrLoadText(dataPath);
        if (textData is null)
            return null;

        var rnd = new Random();
        int textDataLength = textData.Length;

        int actualMinLength = minLength ?? 0;
        int actualMaxLength = maxLength ?? textDataLength;
        int randomLength = rnd.Next(actualMinLength, actualMaxLength + 1);

        var result = GetRandomDataPart(dataPath, randomLength);
        if (result is null)
            return null;

        return new RandomData
        {
            Data = result.Data,
            StartIndex = result.StartIndex,
            RandomLength = randomLength
        };
    }

    public static RandomData? GetRandomDataPartByBitLength(string dataPath, int bitLength)
    {
        var textData = GetOrLoadText(dataPath);
        if (textData is null)
            return null;

        var rnd = new Random();
        int textDataLength = textData.Length;

        int startIndex = rnd.Next(0, textDataLength - 1);

        try
        {
            int remainingDataBitLength = StringBitsTools.StringToBitArray(textData[startIndex..]).Length;

            // Сдвигаем startIndex влево, если не хватает (условно считаем, что символ = 8 бит, т.е. сдвигаем на возможный максимум символов)
            if (remainingDataBitLength < bitLength)
                startIndex -= (bitLength - remainingDataBitLength) / 8 - 1;
            startIndex = Math.Max(startIndex, 0);
        }
        catch
        {
            return null;
        }

        int actualBitLength = 0;
        string resultData = string.Empty;

        try
        {
            int k = startIndex;
            while (actualBitLength < bitLength)
            {
                actualBitLength += StringBitsTools.StringToBitArray(textData[k].ToString()).Length;
                if (actualBitLength <= bitLength)  // Результат может быть меньше заданного bitLength, если символ "не влезает"
                    resultData += textData[k];
                k++;
            }
        }
        catch
        {
            return null;
        }

        return new()
        {
            Data = resultData,
            StartIndex = startIndex
        };
    }

    public static RandomData? GetRandomDataPartWithRandomBitLength(string dataPath, int? minLength = null, int? maxLength = null)
    {
        var textData = GetOrLoadText(dataPath);
        if (textData is null)
            return null;

        var rnd = new Random();
        int textDataLength = textData.Length;

        int actualMinBitLength = minLength ?? 0;
        int actualMaxBitLength = maxLength ?? textDataLength;
        int randomBitLength = rnd.Next(actualMinBitLength, actualMaxBitLength + 1);

        var result = GetRandomDataPartByBitLength(dataPath, randomBitLength);
        if (result is null)
            return null;

        return new()
        {
            Data = result.Data,
            StartIndex = result.StartIndex,
            RandomLength = randomBitLength
        };
    }
}
