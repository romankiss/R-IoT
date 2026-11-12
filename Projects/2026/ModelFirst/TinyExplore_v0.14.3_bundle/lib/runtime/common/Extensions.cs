// written by Roman Kiss, January 3, 2025
// version: 1.0.0.0  01/03/2025  initial version
//          1.0.0.1  03/03/2025  adding GNGGA parser
//          1.0.0.3  15/03/2025  adding StringArrayExtension
//          1.0.0.4  02/05/2025  adding GetUntilOrAll
//          1.0.0.5  08/08/2025  adding DebugEx
//          1.0.0.6  21/08/2025  adding ToUshort
//          1.0.0.7  19/08/2025  adding StringArrayExtension.IndexOf
//          1.0.0.8  19/08/2025  fix bugs for text == null
//          1.0.0.9  24/08/2025  adding more function into StringArrayExtension, HashtableExtension
//          1.0.1.0  26/09/2025  adding StringArrayExtension: EndSubstringOf, Add; 
//          1.0.0.1  29/12/2025  adding TryParseHexCharsToUInt32, FromHexString without throwing an exception (this version has been forgot, it should be 1.0.0.5)
//          1.0.1.2  21/01/2026  adding InterlockedExt
//          1.0.1.3  04/06/2026  adding ButtonPressedException
//
//
//
// Useful extensions for string, array, etc.
//
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace NFAppExtensions
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

        // wrap the byte array (body) with byte at the front and at the end
        // if the frontByte is 255 then use the length of the body as a frontByte.
        // if the endByte is 255 then use the value of the CRC_8 calculation
        public static byte[] Wrapper(
            this byte[] body,
            byte frontByte = byte.MaxValue,
            byte endByte = byte.MaxValue, //byte.MinValue,
            int maxBodyLength = byte.MaxValue - 2,
            byte[] destination = null
            )
        {
            if (body == null || body.Length == 0 || body.Length > maxBodyLength)
                return null;

            if (destination != null && destination.Length != body.Length + 2)
                return null;

            if (destination == null)
                destination = new byte[body.Length + 2];

            // Front byte
            destination[0] = (frontByte == byte.MaxValue) ? (byte)body.Length : frontByte;

            // Body
            for (int i = 0; i < body.Length; i++)
                destination[i + 1] = body[i];

            // End byte
            destination[body.Length + 1] = (endByte == byte.MaxValue) ? body.ComputeChecksum() : endByte;

            return destination;
        }

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
        /// Byte array to hexdecimal string
        /// </summary>
        /// <param name="bytes">array of bytes to convert it</param>
        /// <param name="startIndex">start index of the array to convert it</param>
        /// <param name="length">number of bytes to convert it, if -1 then until the end of byte array</param>
        /// <returns></returns>
        public static string ToHex(this byte[] bytes, int startIndex = 0, int length = -1)
        {
            string bs = "";
            length = length == -1 ? bytes.Length : startIndex + length;
            if (bytes != null && (startIndex >= 0 && startIndex < bytes.Length) && length <= bytes.Length)
            {
                for (int ii = startIndex; ii < length; ii++)
                {
                    bs += bytes[ii].ToString("X2");
                }
            }
            return bs;
        }

        /// <summary>
        /// Converts two bytes from a byte array into a <see cref="ushort"/>.
        /// By default, interprets the data as big-endian (high byte first, e.g. ADDH/ADDL).
        /// </summary>
        /// <param name="bytes">Source byte array.</param>
        /// <param name="startIndex">Index of the first byte (default 0).</param>
        /// <param name="bigEndian">If true, interpret as big-endian (default). 
        /// If false, interpret as little-endian.</param>
        /// <returns>The converted ushort, or 0 if invalid input.</returns>
        public static ushort ToUshort(this byte[] bytes, int startIndex = 0, bool bigEndian = true)
        {
            if (bytes == null || startIndex < 0 || startIndex + 1 >= bytes.Length)
                return 0;

            return bigEndian
                ? (ushort)((bytes[startIndex] << 8) | bytes[startIndex + 1])   // ADDH/ADDL
                : (ushort)((bytes[startIndex + 1] << 8) | bytes[startIndex]); // Low/High
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
        public static bool IsComputedChecksumValidOrEOD(this byte[] bytes, byte computedChecksum, byte initialCRC = 0x00)
        {
            return computedChecksum == 0x00 ? true : computedChecksum == bytes.ComputeChecksum(0, (uint)bytes.Length, initialCRC);
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

        // credit: https://stackoverflow.com/questions/1857513/get-substring-everything-before-certain-char
        public static string GetUntilOrAll(this string text, string stopAt = "\r")
        {
            if (String.IsNullOrEmpty(text)) return text;
            int charLocation = text.IndexOf(stopAt);
            return charLocation == -1 ? text : text.Substring(0, charLocation);
        }

        public static string ReadLine(this string text, string crlf = "\r\n")
        {
            if (text == null) return text;
            return text.TrimStart(new char[] { '\r', '\n' }).Split(new char[] { '\r' }, 2)[0];
        }

        public static string Center(this string text, int width = 14)
        {
            if (text == null) return text;
            return text.PadLeft((width + text.Length) / 2).PadRight(width);
        }
        public static string Prefix(this string text, char ch, bool flag = true)
        {
            if (text == null) return text;
            return flag ? ch + text.Substring(1) : text;
        }
        public static string Postfix(this string text, char ch, bool flag = true)
        {
            if (text == null) return text;
            return flag ? text.Substring(0, text.Length - 1) + ch : text;
        }
        public static string Truncate(this string text, int maxchars = 50)
        {
            if (text == null) return text;
            return text != null && text.Length <= maxchars ? text : text.Substring(0, maxchars) + "...";
        }
        public static bool IsEven(this string text)
        {
            if (text == null) return false;
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
            if (checkForJson)
            {
                if (!text.StartsWith("7B") || !text.EndsWith("7D"))
                    return null;
            }
            byte[] bytes = text.ToByteArray();
            if (bytes == null) return null;
            //  --- replacing Encoding.UTF8.GetString(bytes, 0, bytes.Length) without throwing the exception  ---
            char[] chars = new char[bytes.Length]; // Worst-case: 1 byte = 1 char (ASCII)
            int charPos = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                byte b = bytes[i];

                if (b < 0x80)
                    chars[charPos++] = (char)b;
                else
                    return null;
            }
            return new string(chars, 0, charPos);
            // ------ end of replacement -------
            //return bytes == null ? null : Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
        public static bool IsJson(this string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            string input = text.Trim();
            return input.StartsWith("{") && input.EndsWith("}") || input.StartsWith("[") && input.EndsWith("]");
            //return text.Trim().Substring(0, 1).IndexOfAny(new[] { '[', '{' }) == 0;
        }

        // https://openrtk.readthedocs.io/en/latest/communication_port/nmea.html
        public static bool TryParseGNGGA(this string text, out float latitude, out float longitude, out float amplitude)
        {
            latitude = 0;
            longitude = 0;
            amplitude = 0;
            if (string.IsNullOrEmpty(text) || !text.StartsWith("$GNGGA"))
                return false;
            Debug.Write($"{text}");

            var parts = text.Split(',');
            if (!float.TryParse(parts[2], out latitude))
                return false;
            if (!float.TryParse(parts[4], out longitude))
                return false;
            if (!float.TryParse(parts[9], out amplitude))
                return false;

            int latDD = (int)latitude / 100;
            float latMM = latitude - latDD * 100;
            latitude = latDD + latMM / 60;

            int lonDD = (int)longitude / 100;
            float lonMM = longitude - lonDD * 100;
            longitude = lonDD + lonMM / 60;

            return true;
        }

        public static string EndSubstringOf(this string text, char value = '|')
        {
            if (text == null || text.Length == 0) return text;
            int x = text.LastIndexOf(value);
            return x < 0 ? text : ++x < text.Length ? text.Substring(x) : string.Empty;
        }
        public static string Add(this string text, string tag, char value = '|')
        {
            if (text == null || tag == null) return text;
            return $"{text}{value}{tag}";
        }

        public static bool TryParseHexCharsToUInt32(this string text, out ushort result, uint startIndex = 0)
        {
            result = 0;

            // Ensure we have exactly 8 hex chars starting at startIndex
            if (text == null || text.Length < startIndex + 4)
                return false;

            for (int i = 0; i < 4; i++)
            {
                char c = text[(int)(startIndex + i)];
                ushort hexVal;

                if (c >= '0' && c <= '9')
                    hexVal = (ushort)(c - '0');
                else if (c >= 'A' && c <= 'F')
                    hexVal = (ushort)(c - 'A' + 10);
                else if (c >= 'a' && c <= 'f')
                    hexVal = (ushort)(c - 'a' + 10);
                else
                    return false; // Invalid hex char

                result = (ushort)((result << 4) | hexVal);
            }

            return true;
        }



    }

    public static class StringArrayExtension
    {
        public static string ToString(this string[] sarray, string separator = " ")
        {
            if (sarray == null)
                return null;

            var sb = new StringBuilder();
            foreach (var s in sarray)
                sb.Append($"{s}{separator}");
            return string.IsNullOrEmpty(separator) ? sb.ToString() : sb.ToString().TrimEnd(separator?.ToCharArray());
        }

        public static int IndexOf(this string[] sarray, string key, bool ignoreCase = true)
        {
            if (sarray == null || key == null) return -1;

            for (int i = 0; i < sarray.Length; i++)
            {
                if (ignoreCase)
                {
                    if (sarray[i] != null && sarray[i].ToLower() == key.ToLower())
                        return i;
                }
                else
                {
                    if (sarray[i] == key)
                        return i;
                }
            }
            return -1;
        }

        public static string[] Add(this string[] source, params string[] items)
        {
            if (items == null || items.Length == 0)
                return source;

            if (source == null)
                return (string[])items.Clone();

            string[] result = new string[source.Length + items.Length];
            source.CopyTo(result, 0);
            items.CopyTo(result, source.Length);
            return result;
        }

        public static string[] InsertAt(this string[] source, int index, params string[] items)
        {
            if (source == null) return (string[])items.Clone();
            if (items == null || items.Length == 0) return source;

            if (index < 0) index = 0;
            if (index > source.Length) index = source.Length;

            string[] result = new string[source.Length + items.Length];
            Array.Copy(source, 0, result, 0, index);
            Array.Copy(items, 0, result, index, items.Length);
            Array.Copy(source, index, result, index + items.Length, source.Length - index);

            return result;
        }

        public static string[] InsertAfter(this string[] source, string afterItem, string item, bool ignoreCase = true)
        {
            if (source == null) return new string[] { item };

            int index = source.IndexOf(afterItem, ignoreCase);
            if (index < 0)
                return source.Add(item);

            return source.InsertAt(index + 1, item);
        }

        /// <summary>
        /// Remove item at index.
        /// </summary>
        public static string[] RemoveAt(this string[] source, int index)
        {
            if (source == null || source.Length == 0 || index < 0 || index >= source.Length)
                return source;

            var result = new string[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, result, 0, index);
            if (index < source.Length - 1)
                Array.Copy(source, index + 1, result, index, source.Length - index - 1);
            return result;
        }

        /// <summary>
        /// Remove the first occurrence of an item.
        /// </summary>
        public static string[] RemoveItem(this string[] source, string item, bool ignoreCase = true)
        {
            int idx = source.IndexOf(item, ignoreCase);
            return idx >= 0 ? source.RemoveAt(idx) : source;
        }

    }

    public static class HashtableExtension
    {
        public static string ToString(this Hashtable ht, string separator = " ")
        {
            if (ht == null)
                return null;

            var sb = new StringBuilder();
            foreach (var s in ht.Keys)
                sb.Append($"{s}{separator}");
            return string.IsNullOrEmpty(separator) ? sb.ToString() : sb.ToString().TrimEnd(separator?.ToCharArray());
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

    public static class InterlockedExt
    {
        /// <summary>
        /// Add value to the target in the atomic manner
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value">value to add</param>
        /// <returns>computed value (target + value)</returns>
        public static int Add(this ref int target, int value)
        {
            int initial, computed;

            do
            {
                initial = target;
                computed = initial + value;
            } while (Interlocked.CompareExchange(ref target, computed, initial) != initial);

            return computed;
        }
    }


    //[Conditional("DEBUG")]
    public static class DebugEx
    {
        // Simple message first, condition optional (default true)
        public static void WriteIf(string message, bool condition = true)
        {
#if DEBUG
            if (condition)
                Debug.Write(message);
#endif
        }

        public static void WriteLineIf(string message, bool condition = true)
        {
#if DEBUG
            if (condition)
                Debug.WriteLine(message);
#endif
        }
    }


  
    public class ButtonPressedException : Exception
    {
        public ButtonPressedException()
        {
        }
        public ButtonPressedException(string message)
            : base(message)
        {
        }
        public ButtonPressedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}