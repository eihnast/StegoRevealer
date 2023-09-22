using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.TypeExtensions;
using System.Collections;

namespace StegoRevealer.StegoCore.ModuleTests
{
    [TestClass]
    public class BitArrayTests
    {
        #region BitArrayExtensions Tests

        [TestMethod]
        public void CheckForOneByteByByteValue()
        {
            byte byteValue = 78;
            var bitArray = BitArrayExtensions.NewFromByte(byteValue);
            Assert.AreEqual(true, bitArray.IsOneByte());
        }

        [TestMethod]
        public void CheckForOneByteByBits()
        {
            var bits = new BitArray(8);
            for (int i = 0; i < 8; i++)
                bits[i] = (i % 2 == 0 ? true : false);
            Assert.AreEqual(true, bits.IsOneByte());
        }

        [TestMethod]
        public void CheckForNotOneByte()
        {
            var bitsOverhead = new BitArray(9);
            for (int i = 0; i < 9; i++)
                bitsOverhead[i] = (i % 2 == 0 ? true : false);
            var bitsUnderhead = new BitArray(7);
            for (int i = 0; i < 7; i++)
                bitsUnderhead[i] = (i % 2 == 0 ? true : false);
            Assert.AreEqual(false, bitsOverhead.IsOneByte());
            Assert.AreEqual(false, bitsUnderhead.IsOneByte());
        }

        [TestMethod]
        public void CheckAsByte()
        {
            // 78 = 01001110
            // Однако по умолчанию BitArray хранит биты в обратном порядке: 01110010
            const byte byteValue = 78;
            bool[] byteAsBits = new bool[] { false, true, true, true, false, false, true, false };
            var bitArray = new BitArray(8);
            for (int i = 0; i < 8; i++)
                bitArray[i] = byteAsBits[i];
            Assert.AreEqual(byteValue, bitArray.AsByte());
        }

        [TestMethod]
        public void CheckAsByteWithExtraBits()
        {
            // 78 = 01001110
            // Однако по умолчанию BitArray хранит биты в обратном порядке: 01110010
            bool[] byteAsBitsWithExtra = new bool[] { false, true, true, true, false, false, true, false, false, true };
            var bitArray = new BitArray(10);
            for (int i = 0; i < 10; i++)
                bitArray[i] = byteAsBitsWithExtra[i];

            string errorExpectedText = "Ожидалось исключение: 'System.ArgumentException: Offset and length were out of bounds for the array or " +
                "count is greater than the number of elements from index to the end of the source collection..'";
            try
            {
                var result = bitArray.AsByte();
                Assert.Fail(errorExpectedText);
            }
            catch (ArgumentException ex) { }
            catch (Exception)
            {
                Assert.Fail(errorExpectedText);
            }
        }

        [TestMethod]
        public void CheckAsByteWithExtraBitsButProvide()
        {
            // 78 = 01001110
            // Однако по умолчанию BitArray хранит биты в обратном порядке: 01110010
            // "Лишние" биты должны быть "обрезаны"
            const byte byteValue = 78;
            bool[] byteAsBitsWithExtra = new bool[] { false, true, true, true, false, false, true, false, false, true };  // Лишние 0 и 1 в конце
            var bitArray = new BitArray(10);
            for (int i = 0; i < 10; i++)
                bitArray[i] = byteAsBitsWithExtra[i];

            var result = bitArray.AsByte(provideOneByte: true);
            Assert.AreEqual(result, byteValue);
        }

        [TestMethod]
        public void CheckAsByteWithMissingBitsButProvide()
        {
            // Недостающие биты должны быть заполнены нулями: 10011 -> 10011000 = 152
            const byte expectedByteValue = 152;
            bool[] byteAsBitsWithExtra = new bool[] { true, true, false, false, true };  // Заполнение с конца: 10011 -> 11001
            var bitArray = new BitArray(5);
            for (int i = 0; i < 5; i++)
                bitArray[i] = byteAsBitsWithExtra[i];

            var result = bitArray.AsByte(provideOneByte: true);
            Assert.AreEqual(result, expectedByteValue);
        }

        [TestMethod]
        public void CheckAsInt()
        {
            // 78 = 01001110
            // Однако по умолчанию BitArray хранит биты в обратном порядке: 01110010
            const int value = 78;
            bool[] intAsBits = new bool[] { false, true, true, true, false, false, true, false };
            var bitArray = new BitArray(8);
            for (int i = 0; i < 8; i++)
                bitArray[i] = intAsBits[i];
            Assert.AreEqual(value, bitArray.AsInt());
        }

        [TestMethod]
        public void CheckToBytes()
        {
            // По умолчанию BitArray хранит биты в обратном порядке: 01110010, 10111010
            // однако массив байт будет сформирован в порядке, соответствующем порядку следования восьмёрок бит в массиве
            byte[] bytes = new byte[] { 78, 93 };  // 01001110 01011101
            bool[] bits = new bool[] { 
                false, true, true, true, false, false, true, false,  // 01110010 (78 наоборот)
                true, false, true, true, true, false, true, false    // 10111010 (93 наоборот)
            };
            var bitArray = new BitArray(16);
            for (int i = 0; i < 16; i++)
                bitArray[i] = bits[i];

            var convertedBytes = bitArray.ToBytes();
            for (int i = 0; i < 2; i++)
                Assert.AreEqual(bytes[i], convertedBytes[i]);  // Порядок байт должен быть тот же
        }

        [TestMethod]
        public void CheckNewFromByte()
        {
            // Должен из байта сформировать BitArray: при этом биты будут записаны в обратном порядке (от НЗБ в 0-м индексе)
            byte byteValue = 78;  // 01001110
            bool[] expectedBitsInArray = new bool[] { false, true, true, true, false, false, true, false };  // 01001110 (78 наоборот)
            var bitArray = BitArrayExtensions.NewFromByte(byteValue);
            for (int i = 0; i < 8; i++)
                Assert.AreEqual(expectedBitsInArray[i], bitArray[i]);
        }

        [TestMethod]
        public void CheckNewFromByteWithLinearOrder()
        {
            // Должен из байта сформировать BitArray: при этом биты будут записаны в прямом порядке (от старшего бита в 0-м индексе)
            byte byteValue = 78;  // 01001110
            bool[] expectedBitsInArray = new bool[] { false, true, false, false, true, true, true, false };
            var bitArray = BitArrayExtensions.NewFromByte(byteValue, linearOrder: true);
            for (int i = 0; i < 8; i++)
                Assert.AreEqual(expectedBitsInArray[i], bitArray[i]);
        }

        #endregion

        #region PixelsTools Tests

        [TestMethod]
        public void CheckInvertBit()
        {
            bool valueZero = false;
            bool valueOne = true;
            Assert.AreEqual(true, PixelsTools.InvertBit(valueZero));
            Assert.AreEqual(false, PixelsTools.InvertBit(valueOne));
        }

        [TestMethod]
        public void CheckInvertBitRef()
        {
            bool valueZero = false;
            bool valueOne = true;
            PixelsTools.InvertBit(ref valueZero);
            PixelsTools.InvertBit(ref valueOne);
            Assert.AreEqual(true, valueZero);
            Assert.AreEqual(false, valueOne);
        }

        [TestMethod]
        public void CheckLsbInverting()
        {
            byte originalByte = 78;  // 01001110
            byte[] invertedLsbBytes = new byte[] { 79, 77, 73, 65, 81, 113, 49, 177 };
            // 1 lsb = 01001111; 2 lsb = 01001101; 3 lsb = 01001001; 4 lsb = 01000001;
            // 5 lsb = 01010001; 6 lsb = 01110001; 7 lsb = 00110001; 8 lsb = 10110001.

            for (int i = 0; i < 8; i++)
            {
                var invertedLsbByte = PixelsTools.InvertLsb(originalByte, i + 1);
                Assert.AreEqual(invertedLsbBytes[i], invertedLsbByte);
            }
        }

        [TestMethod]
        public void CheckLsbInvertingByRef()
        {
            byte originalByte = 78;  // 01001110
            byte[] invertedLsbBytes = new byte[] { 79, 76, 75, 68, 91, 100, 27, 228 };
            // 01001110 -> 01001111 -> 01001100 -> 01001011 -> 01000100 -> 01011011 -> 01100100 -> 00011011 -> 11100100

            byte currentByte = originalByte;
            for (int i = 0; i < 8; i++)
            {
                currentByte = PixelsTools.InvertLsb(currentByte, i + 1);
                Assert.AreEqual(invertedLsbBytes[i], currentByte);
            }
        }

        [TestMethod]
        public void CheckLsbInvertingForInteger()
        {
            int originalByte = 78;  // 01001110
            int[] invertedLsbBytes = new int[] { 79, 77, 73, 65, 81, 113, 49, 177 };
            // 1 lsb = 01001111; 2 lsb = 01001101; 3 lsb = 01001001; 4 lsb = 01000001;
            // 5 lsb = 01010001; 6 lsb = 01110001; 7 lsb = 00110001; 8 lsb = 10110001.

            for (int i = 0; i < 8; i++)
            {
                var invertedLsbByte = PixelsTools.InvertLsb(originalByte, i + 1);
                Assert.AreEqual(invertedLsbBytes[i], invertedLsbByte);
            }
        }

        [TestMethod]
        public void CheckLsbInvertingForIntegerByRef()
        {
            int originalByte = 78;  // 01001110
            int[] invertedLsbBytes = new int[] { 79, 76, 75, 68, 91, 100, 27, 228 };
            // 01001110 -> 01001111 -> 01001100 -> 01001011 -> 01000100 -> 01011011 -> 01100100 -> 00011011 -> 11100100

            int currentByte = originalByte;
            for (int i = 0; i < 8; i++)
            {
                currentByte = PixelsTools.InvertLsb(currentByte, i + 1);
                Assert.AreEqual(invertedLsbBytes[i], currentByte);
            }
        }

        [TestMethod]
        public void CheckLsbInvertingForIntegerWithCutting()
        {
            int originalInt = 124332;  // Будет обрезан до 255: 11111111
            int[] invertedLsbBytes = new int[] { 254, 252, 248, 240, 224, 192, 128, 0 };
            // 1 lsb = 11111110; 2 lsb = 11111100; 3 lsb = 11111000; 4 lsb = 11110000;
            // 5 lsb = 11100000; 6 lsb = 11000000; 7 lsb = 10000000; 8 lsb = 00000000.

            for (int i = 0; i < 8; i++)
            {
                var invertedLsbByte = PixelsTools.InvertLsb(originalInt, i + 1);
                Assert.AreEqual(invertedLsbBytes[i], invertedLsbByte);
            }
        }

        [TestMethod]
        public void CheckLsbValuesSettingUp_OneLsb()
        {
            const byte byteValue = 78;  // 01001110
            bool[] lsbs = new bool[] { true };  // 01001110 -> 01001111
            const byte expectedByte = 79;  // 01001111

            var changedByte = PixelsTools.SetLsbValues(byteValue, lsbs);
            Assert.AreEqual(expectedByte, changedByte);
        }

        [TestMethod]
        public void CheckLsbValuesSettingUp_MoreLsb()
        {
            // LSB заполняются по правому краю битового представления слева (последние n бит в указанном порядке)
            const byte byteValue = 78;  // 01001110
            var lsbs = new List<bool[]>() {
                new bool[] { false, true },  // 2 LSB: 01001110 -> 01001101
                new bool[] { true, false, false, false, true },  // 5 LSB: 01001110 -> 01010001
                new bool[] { true, false, false, true, true, false, true, true }  // 8 LSB: 01001110 -> 10011011
            };
            var expectedBytes = new byte[] { 77, 81, 155 };

            for (int i = 0; i < lsbs.Count; i++)
            {
                var changedByte = PixelsTools.SetLsbValues(byteValue, lsbs[i]);
                Assert.AreEqual(expectedBytes[i], changedByte);
            }
        }

        [TestMethod]
        public void CheckLsbValuesSettingUpByInts()
        {
            // LSB заполняются по правому краю битового представления слева (последние n бит в указанном порядке)
            const byte byteValue = 78;  // 01001110
            var lsbs = new List<int[]>() {
                new int[] { 1 },  // 1 LSB: 01001110 -> 01001111
                new int[] { 0, 1 },  // 2 LSB: 01001110 -> 01001101
                new int[] { 1, 0, 0, 0, 1 },  // 5 LSB: 01001110 -> 01010001
                new int[] { 1, 0, 0, 1, 1, 0, 1, 1 }  // 8 LSB: 01001110 -> 10011011
            };
            var expectedBytes = new byte[] { 79, 77, 81, 155 };

            for (int i = 0; i < lsbs.Count; i++)
            {
                var changedByte = PixelsTools.SetLsbValues(byteValue, lsbs[i]);
                Assert.AreEqual(expectedBytes[i], changedByte);
            }
        }

        [TestMethod]
        public void CheckLsbValueSettingUpForInt_OneLsb()
        {
            const int value = 124122;  // Будет обрезан до 255
            int[] lsbs = new int[] { 0 };  // 11111111 -> 11111110
            const byte expectedByte = 254;  // 11111110

            var changedByte = PixelsTools.SetLsbValues(value, lsbs);
            Assert.AreEqual(expectedByte, changedByte);
        }

        [TestMethod]
        public void CheckLsbValueSettingUpForNegativeInt_OneLsb()
        {
            const int value = -2398;  // Будет обрезан до 0
            int[] lsbs = new int[] { 1 };  // 00000000 -> 00000001
            const byte expectedByte = 1;  // 00000001

            var changedByte = PixelsTools.SetLsbValues(value, lsbs);
            Assert.AreEqual(expectedByte, changedByte);
        }

        [TestMethod]
        public void CheckLsbValuesSettingUpByBitArray()
        {
            // LSB заполняются по правому краю битового представления слева (последние n бит в указанном порядке)
            const byte byteValue = 78;  // 01001110
            var lsbs = new List<BitArray>() {
                new BitArray(1),  // Для 1 LSB: 01001110 -> 01001111
                new BitArray(2),  // Для 2 LSB: 01001110 -> 01001101
                new BitArray(5),  // Для 5 LSB: 01001110 -> 01010001
                new BitArray(8)   // Для 8 LSB: 01001110 -> 10011011
            };
            var lsbsAsBools = new List<bool[]>() {
                new bool[] { true },  // 1 LSB: 01001110 -> 01001111
                new bool[] { false, true },  // 2 LSB: 01001110 -> 01001101
                new bool[] { true, false, false, false, true },  // 5 LSB: 01001110 -> 01010001
                new bool[] { true, false, false, true, true, false, true, true }  // 8 LSB: 01001110 -> 10011011
            };

            for (int i = 0; i < lsbs.Count; i++)
            {
                for (int j = 0; j < lsbs[i].Count; j++)
                    lsbs[i][j] = lsbsAsBools[i][j];
            }

            var expectedBytes = new byte[] { 79, 77, 81, 155 };

            for (int i = 0; i < lsbs.Count; i++)
            {
                var changedByte = PixelsTools.SetLsbValues(byteValue, lsbs[i]);
                Assert.AreEqual(expectedBytes[i], changedByte);
            }
        }

        #endregion

        #region StringBitsTools Tests

        [TestMethod]
        public void CheckStringToBitArrayWithLinearOrder()
        {
            string data = "j2cР—";  // 01101010 , 00110010 , 01100011 , 11010000 10100000 , 11100010 10000000 10010100
            byte[] dataBytes = new byte[] { 106, 50, 99, 208, 160, 226, 128, 148 };
            var dataBitArray = StringBitsTools.StringToBitArray(data);
            
            var expectedBitArray = new BitArray(dataBytes.Length * 8);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                var currentByteBitArray = BitArrayExtensions.NewFromByte(dataBytes[i], linearOrder: true);
                for (int j = 0; j < 8; j++)
                    expectedBitArray[i * 8 + j] = currentByteBitArray[j];
            }

            Assert.AreEqual(dataBitArray.Count, expectedBitArray.Count);
            for (int i = 0; i < dataBitArray.Length; i++)
                Assert.AreEqual(expectedBitArray[i], dataBitArray[i], $"Ошибка на {i} бите");
        }

        [TestMethod]
        public void CheckStringToBitArrayWithReversedOrder()
        {
            string data = "j2cР—";  // 01101010 , 00110010 , 01100011 , 11010000 10100000 , 11100010 10000000 10010100
            byte[] dataBytes = new byte[] { 106, 50, 99, 208, 160, 226, 128, 148 };
            var dataBitArray = StringBitsTools.StringToBitArray(data, linearBitArrays: false);

            var expectedBitArray = new BitArray(dataBytes.Length * 8);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                var currentByteBitArray = BitArrayExtensions.NewFromByte(dataBytes[i]);
                for (int j = 0; j < 8; j++)
                    expectedBitArray[i * 8 + j] = currentByteBitArray[j];
            }

            Assert.AreEqual(dataBitArray.Count, expectedBitArray.Count);
            for (int i = 0; i < dataBitArray.Length; i++)
                Assert.AreEqual(expectedBitArray[i], dataBitArray[i], $"Ошибка на {i} бите");
        }

        [TestMethod]
        public void CheckBitArrayToString_OverStringToPitMethod()
        {
            string data = "j2cР—";  // 01101010 , 00110010 , 01100011 , 11010000 10100000 , 11100010 10000000 10010100
            var bitArrayWithLinearOrder = StringBitsTools.StringToBitArray(data, linearBitArrays: true);
            var bitArrayWithReversedOrder = StringBitsTools.StringToBitArray(data, linearBitArrays: false);

            var dataByLinearBitsOrder = StringBitsTools.BitArrayToString(bitArrayWithLinearOrder, linearBitArrays: true);
            var dataByReversedBitsOrder = StringBitsTools.BitArrayToString(bitArrayWithReversedOrder, linearBitArrays: false);

            Assert.AreEqual(data, dataByLinearBitsOrder);
            Assert.AreEqual(data, dataByReversedBitsOrder);
        }

        #endregion
    }
}
