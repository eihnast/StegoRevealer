using System.Collections;

namespace StegoRevealer.StegoCore.CommonLib.TypeExtensions;

/* 
 * "Конструктор генерирует содержимое BitArray из массива байтов.
 * Каждый байт разделен на восемь битов в созданной коллекции.
 * Первый байт копируется в первые восемь битов BitArray с наименее значимым битом в качестве первого элемента в коллекции.
 * Второй байт копируется в индексы с восьми по пятнадцать и так далее."
 * Пример: bitArray = new BitArray(new byte[] { 78 })
 * Несмотря на то, что 78 == 01001110, в массиве bitArray (при следовании от индекса 0 к 7) это будет выглядеть так: 01110010
*/

/// <summary>
/// Расширения класса BitArray
/// </summary>
public static class BitArrayExtensions
{
    /// <summary>
    /// Содержит ли BitArray только один байт
    /// </summary>
    public static bool IsOneByte(this BitArray bitArray)
    {
        return bitArray.Length == 8;
    }

    /// <summary>
    /// Преобразование в байт (если хранит больше одного байта - будет возвращён только первый байт)<br/>
    /// Байт будет преобразован с конца<br/>
    /// (предполагается, что биты в BitArray хранятся от младшего разряда к старшему - с 0 по 8 индексы).
    /// </summary>
    public static byte AsByte(this BitArray bitArray, bool provideOneByte = false)
    {
        var actualBitArray = bitArray;
        if (provideOneByte && !IsOneByte(bitArray))
        {
            if (bitArray.Length > 8)
            {
                actualBitArray = new BitArray(8);
                for (int i = 0; i < 8; i++)
                    actualBitArray[i] = bitArray[i];  // Т.к. копирую целиком 1 байт, инверсия записи бит в байте неважна
            }
            else if (bitArray.Length < 8)
            {
                // Заполняю нулями все недостающие НЗБ (т.к. инверсия записи - нули в начале)
                actualBitArray = new BitArray(new byte[] { 0 });
                int missingBitsNum = 8 - bitArray.Length;
                for (int i = missingBitsNum; i < 8; i++)
                    actualBitArray[i] = bitArray[i - missingBitsNum];
            }
        }

        byte[] resultArray = new byte[1];
        actualBitArray.CopyTo(resultArray, 0);
        return resultArray[0];
    }

    /// <summary>
    /// Возвращает байт в виде integer<br/>
    /// Байт будет преобразован с конца<br/>
    /// (предполагается, что биты в BitArray хранятся от младшего разряда к старшему - с 0 по 8 индексы).
    /// </summary>
    public static int AsInt(this BitArray value, bool provideOneByte = false)
    {
        return value.AsByte(provideOneByte: provideOneByte);
    }

    /// <summary>
    /// Возвращает массив байт<br/>
    /// Каждые 8 бит последовательно будут преобразованы в байты. Каждый байт будет преобразован с конца<br/>
    /// (предполагается, что биты в BitArray хранятся от младшего разряда к старшему - с 0 по 8 индексы).
    /// </summary>
    public static byte[] ToBytes(this BitArray bitArray)
    {
        int bytesNum = bitArray.Length / 8;
        var extraByte = bitArray.Length % 8 != 0;

        byte[] resultArray = new byte[bytesNum + (extraByte ? 1 : 0)];
        bitArray.CopyTo(resultArray, 0);

        if (extraByte)
            return resultArray[0..^1];
        else
            return resultArray;
    }

    /// <summary>
    /// Новый BitArray из одного байта<br/>
    /// Биты в BitArray будут записаны от младшего разряда к старшему - с 0 по 8 индексы<br/>
    /// Если linearOrder - true, биты будут записаны в "прямом" порядке: на 0 индексе - старший бит
    /// </summary>
    public static BitArray NewFromByte(byte value, bool linearOrder = false)
    {
        var bitArray = new BitArray(new byte[] { value });

        if (linearOrder)
        {
            var reversedBitArray = new BitArray(8);
            for (int i = 0; i < 8; i++)
                reversedBitArray[i] = bitArray[7 - i];
            return reversedBitArray;
        }

        return bitArray;
    }

    /// <summary>
    /// Новый BitArray из одного integer (обрезает значения до диапазона [0..255])<br/>
    /// Биты в BitArray будут записаны от младшего разряда к старшему - с 0 по 8 индексы<br/>
    /// Если linearOrder - true, биты будут записаны в "прямом" порядке: на 0 индексе - старший бит
    /// </summary>
    public static BitArray NewFromInt(int value, bool linearOrder = false)
    {
        return NewFromByte(value.ToByte(), linearOrder: linearOrder);
    }
}
