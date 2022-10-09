using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoRevealer.StegoCore
{
    public static class IntExtensions
    {
        // Обрезает int до диапазона [0.255]
        public static byte ToByte(this int value)
        {
            byte byteValue = (byte)value;
            if (value > 255)
                byteValue = (byte)255;
            else if (value < 0)
                byteValue = (byte)0;

            return byteValue;
        }
    }
}
