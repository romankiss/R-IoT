// written by Roman Kiss, January 3, 2025
// version: 1.0.0.0  01/03/2025  initial version
//
// Useful extensions for string, array, etc.
//
using System;
using System.Text;
using System.Threading;

namespace CanSat-28.4.E22_900T22D.Extensions
{
    /// <summary>
    /// Based on the https://err.se/crc8-for-ibutton-in-c/
    /// </summary>
    public static class Definitions
    {
        public static readonly byte[] CrcTable = new byte[] {
            0, 94, 188, 226, 97, 63, 221, 131, 194, 156, 126,
            32, 163, 253, 31, 65, 157, 195, 33, 127, 252, 162,
            64, 30, 95, 1, 227, 189, 62, 96, 130, 220, 35,
            125, 159, 193, 66, 28, 254, 160, 225, 191, 93, 3,
            128, 222, 60, 98, 190, 224, 2, 92, 223, 129, 99,
            61, 124, 34, 192, 158, 29, 67, 161, 255, 70, 24,
            250, 164, 39, 121, 155, 197, 132, 218, 56, 102,
            229, 187, 89, 7, 219, 133, 103, 57, 186, 228, 6,
            88, 25, 71, 165, 251, 120, 38, 196, 154, 101, 59,
            217, 135, 4, 90, 184, 230, 167, 249, 27, 69, 198,
            152, 122, 36, 248, 166, 68, 26, 153, 199, 37, 123,
            58, 100, 134, 216, 91, 5, 231, 185, 140, 210, 48,
            110, 237, 179, 81, 15, 78, 16, 242, 172, 47, 113,
            147, 205, 17, 79, 173, 243, 112, 46, 204, 146,
            211, 141, 111, 49, 178, 236, 14, 80, 175, 241, 19,
            77, 206, 144, 114, 44, 109, 51, 209, 143, 12, 82,
            176, 238, 50, 108, 142, 208, 83, 13, 239, 177,
            240, 174, 76, 18, 145, 207, 45, 115, 202, 148, 118,
            40, 171, 245, 23, 73, 8, 86, 180, 234, 105, 55,
            213, 139, 87, 9, 235, 181, 54, 104, 138, 212, 149,
            203, 41, 119, 244, 170, 72, 22, 233, 183, 85, 11,
            136, 214, 52, 106, 43, 117, 151, 201, 74, 20, 246,
            168, 116, 42, 200, 150, 21, 75, 169, 247, 182, 232,
            10, 84, 215, 137, 107, 53
        };
    }


    public static class ArrayExtension
    {
        public static byte[] Add(this byte[] first, byte[] second)
        {
            byte[] concat = new byte[first.Length + second.Length];
            first.CopyTo(concat, 0);
            second.CopyTo(concat, first.Length);
            return concat;
        }

        public static byte[] AddByte(this byte[] first, byte value)
        {
            byte[] concat = new byte[first.Length + 1];
            first.CopyTo(concat, 0);
            concat[first.Length] = value;
            return concat;
        }

        public static byte[] AddInt(this byte[] first, int value, bool BigEndian = true)
        {
            var arrayOfValue = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian && BigEndian)
                arrayOfValue.Reverse();
            return first.Add(arrayOfValue);
        }

        // https://stackoverflow.com/questions/6088287/reverse-an-array-without-using-array-reverse
        public static byte[] Reverse(this byte[] arr)
        {
            for (int i = 0; i < arr.Length / 2; i++)
            {
                var tmp = arr[i];
                arr[i] = arr[arr.Length - i - 1];
                arr[arr.Length - i - 1] = tmp;
            }
            return arr;
        }

        /// <summary>
        /// Based on the https://err.se/crc8-for-ibutton-in-c/
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="crc"></param>
        /// <returns></returns>
        public static byte ComputeChecksum(this byte[] bytes, byte crc = 0)
        {
            return bytes.ComputeChecksum(0, (uint)bytes.Length, crc);
        }
        public static byte ComputeChecksum(this byte[] bytes, uint start, uint length, byte crc = 0x00)
        {
            uint numberOfBytes = start + length;
            if (bytes != null && numberOfBytes <= bytes.Length)
            {
                for (uint ii = start; ii < numberOfBytes; ii++)
                {
                    crc = Definitions.CrcTable[crc ^ bytes[ii]];
                }
            }
            return crc;
        }
    }

    public static class NumericExtensions
    {
        public static int MinValue(this int value, int minValue)
        {
            return value < minValue ? minValue : value;
        }

        public static int MaxValue(this int value, int maxValue)
        {
            return value < maxValue ? value : maxValue;
        }
    }

    public static class StringExtensions
    {
        public static string Replace(this string text, string oldText, string newText)
        {
            var sb = new StringBuilder(text).Replace(oldText, newText);
            return sb.ToString();
        }
        public static string ReplaceCRLF(this string text, string replacement = null)
        {
            if (replacement == null)
                return text.Replace("\r", "\\r").Replace("\n", "\\n");
            else if (text.IndexOf("\r\n") >= 0)
                return text.Replace("\r\n", replacement);
            else
                return text.Replace("\r", replacement);
        }

        public static string IfNullOrEmpty(this string text, string newText)
        {
            return string.IsNullOrEmpty(text) ? newText : text;
        }

        // at list one item in the list must be in the text
        public static bool ContainsIn(this string text, params string[] list)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (string s in list)
            {
                if (text.Length >= s.Length && text.IndexOf(s) >= 0)
                    return true;
            }
            return false;
        }

        // any item in the list must be not in the text
        public static bool NotContainsIn(this string text, params string[] list)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (string s in list)
            {
                if (text.Length >= s.Length && text.IndexOf(s) >= 0)
                    return false;
            }
            return true;
        }

        // all items in the list must be in the text
        public static bool ContainsAllIn(this string text, params string[] list)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (string s in list)
            {
                if (text.Length >= s.Length && text.IndexOf(s) == -1)
                    return false;
            }
            return true;
        }

        public static string UrlDecode(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return text.Replace("%2F", "/").Replace("%3D", "=").Replace("%24", "$").Replace("%20", " ");
        }

        public static string ReadLine(this string text, string crlf = "\r\n")
        {
            return text.TrimStart(new char[] { '\r', '\n' }).Split(new char[] { '\r' }, 2)[0];
        }

        public static string Center(this string text, int width = 14)
        {
            return text.PadLeft((width + text.Length) / 2).PadRight(width);
        }
        public static string Prefix(this string text, char ch, bool flag = true)
        {
            return flag ? ch + text.Substring(1) : text;
        }
        public static string Postfix(this string text, char ch, bool flag = true)
        {
            return flag ? text.Substring(0, text.Length - 1) + ch : text;
        }
        public static string Truncate(this string text, int maxchars = 50)
        {
            return text != null && text.Length <= maxchars ? text : text.Substring(0, maxchars) + "...";
        }
        public static bool IsEven(this string text)
        {
            return text.Length % 2 == 0;
        }
        public static byte[] ToByteArray(this string text)
        {
            if (text == null || !text.IsEven())
                return null;

            int NumberChars = text.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(text.Substring(i, 2), 16);
            return bytes;
        }
        public static string FromHexString(this string text, bool checkForJson = true)
        {
            if (checkForJson && text.StartsWith("7B") && text.EndsWith("7D"))
            {
                byte[] bytes = text.ToByteArray();
                return bytes == null ? null : Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            }
            return null;
        }
        public static bool IsJson(this string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            string input = text.Trim();
            return input.StartsWith("{") && input.EndsWith("}") || input.StartsWith("[") && input.EndsWith("]");
            //return text.Trim().Substring(0, 1).IndexOfAny(new[] { '[', '{' }) == 0;
        }


    }

    public static class ThreadExtension
    {
        public static void AbortIfRunning(this Thread thread)
        {
            if (thread != null && thread.ThreadState == ThreadState.Running)
                thread.Abort();
        }
    }
}