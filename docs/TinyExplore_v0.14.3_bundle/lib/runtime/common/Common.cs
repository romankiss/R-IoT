// written by Roman Kiss, May 15, 2026
// version: 1.0.0.0  May 15, 2026  initial version
//          1.0.2.0  May 23, 2026 More LoRa Extensions
//          1.0.2.1  May 23, 2026 FastRandom
//          1.0.2.2  Sep 09, 2026 fixing Join
//          1.0.2.3  Sep 10, 2026 UnwrapTextArg, TextArg
//          1.0.2.4  Sep 16, 2026 Removing: UnwrapTextArg, TextArg (moving to the VirtualModule)
//          1.0.2.5  Sep 19, 2026 Clean-up (JoinArgs)
//
//
using NFAppExtensions;
using System;
using System.Text;
using System.Threading;

namespace NFAppLib
{
    #region Delegates
    public delegate void DelayHandler(int delayInMs);
    public delegate void OnReceive(object source, RcvPacketArgs e);
    public delegate void OnReceiveRxTag(object source, string tag);

    #endregion

    #region Interfaces
    public interface ILoRaDriver
    {
        void SetupLoRa(uint freq, int offset, byte sf, byte bw, byte cr, ushort sync, ushort preamble, byte irqMode);
        void ClearLastRxTag();
        string GetLastRxTag();
        string GetLastSetupLoRa();
        bool TransmitPacket(params byte[] msg);
    }


    #endregion

    #region Event Arguments
    public class RcvPacketArgs : EventArgs
    {
        public ushort AddressID { get; set; }
        public short DataLength { get; set; }
        public string Data { get; set; }
        public short RSSI { get; set; }
        public short SNR { get; set; }
        public byte[] Packet { get; set; }

    }

    #endregion

    #region TextPacket
    public class TextPacket
    {
        const int sizeOfHeader = 5;
        const int sizeOfHeader2 = 9;

        public ushort From { get; set; }
        public ushort To { get; set; }
        public int Id { get; set; }
        public byte Type { get; set; }
        public string Data { get; set; }
        public bool IsText => Type == 0x00;
        public int SizeOfHeader => sizeOfHeader2;

        #region Old version without the Id property (sizeOfHeader = 5)
        public byte[] ToLoraArray()
        {
            // wrap the TextPacket for LoRa simple frame such as "Len + TextPacket + EOD/CRC8" format
            return this.ToArray().Wrapper();
        }
        public byte[] ToArray()
        {
            byte[] buffer = new byte[sizeOfHeader + Data.Length];
            buffer[0] = (byte)((From >> 8) & 0xFF);
            buffer[1] = (byte)(From & 0xFF);
            buffer[2] = (byte)((To >> 8) & 0xFF);
            buffer[3] = (byte)(To & 0xFF);
            buffer[4] = 0x00;  //text packet
            Encoding.UTF8.GetBytes(Data, 0, Data.Length, buffer, sizeOfHeader);
            return buffer;
        }
        public static TextPacket Create(byte[] packet)
        {
            if (packet == null || packet.Length == 0 || packet.Length > 248 || packet.Length < sizeOfHeader) return null;
            TextPacket retVal = new TextPacket();
            retVal.From = packet.ToUshort();
            retVal.To = packet.ToUshort(2);
            retVal.Type = packet[4];
            retVal.Data = Encoding.UTF8.GetString(packet, sizeOfHeader, packet.Length - sizeOfHeader);
            return retVal;
        }
        #endregion

        #region New version with the Id property (sizeOfHeader = 9)
        public byte[] ToLoraArray2()
        {
            // wrap the TextPacket for LoRa simple frame such as "Len + TextPacket + EOD/CRC8" format
            return this.ToArray2().Wrapper();
        }
        public byte[] ToArray2()
        {
            byte[] buffer = new byte[sizeOfHeader2 + Data.Length];
            buffer[0] = (byte)((From >> 8) & 0xFF);
            buffer[1] = (byte)(From & 0xFF);
            buffer[2] = (byte)((To >> 8) & 0xFF);
            buffer[3] = (byte)(To & 0xFF);
            buffer[4] = (byte)(Id >> 24);
            buffer[5] = (byte)(Id >> 16);
            buffer[6] = (byte)(Id >> 8);
            buffer[7] = (byte)Id;
            buffer[8] = 0x00;  //text packet
            Encoding.UTF8.GetBytes(Data, 0, Data.Length, buffer, sizeOfHeader2);
            return buffer;
        }
        public static TextPacket Create2(byte[] packet)
        {
            if (packet == null || packet.Length == 0 || packet.Length > 245 || packet.Length < sizeOfHeader2) return null;
            TextPacket retVal = new TextPacket();
            retVal.From = packet.ToUshort();
            retVal.To = packet.ToUshort(2);
            retVal.Id = (packet[4] << 24) | (packet[5] << 16) | (packet[6] << 8) | packet[7];
            retVal.Type = packet[8];
            retVal.Data = Encoding.UTF8.GetString(packet, sizeOfHeader2, packet.Length - sizeOfHeader2);
            return retVal;
        }
        #endregion
    }
    #endregion

    #region Extensions
    public static class ByteArrayExtensions
    {
        public static byte GetLeastFrequentNonZeroByte(this byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0x00; // nothing found

            byte firstVal = 0xFF;
            byte secondVal = 0xFF;
            int countFirst = 0;
            int countSecond = 0;

            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                if (b == 0x00)
                    continue;

                if (firstVal == 0xFF)
                {
                    firstVal = b;
                    countFirst++;
                }
                else if (b == firstVal)
                {
                    countFirst++;
                }
                else if (secondVal == 0xFF)
                {
                    secondVal = b;
                    countSecond++;
                }
                else if (b == secondVal)
                {
                    countSecond++;
                }
                else
                {
                    // More than 2 distinct non-zero values
                    return 0x00;
                }
            }

            if (firstVal == 0xFF) return 0x00;       // no non-zero found
            if (secondVal == 0xFF) return firstVal;  // only one non-zero found

            return (countFirst <= countSecond) ? firstVal : secondVal;
        }

        public static string ToText(this byte[] data, int start = 0, int maxLength = -1,
                             bool bStopAtNoAscii = false, string prompt = null,
                             char replaceNoAscii = '.')
        {
            if (data == null || data.Length == 0 || start < 0 || start >= data.Length)
                return string.Empty;

            if (maxLength < 0 || start + maxLength > data.Length)
                maxLength = data.Length - start;

            if (maxLength <= 0)
                return string.Empty;

            int promptLength = (prompt == null) ? 0 : prompt.Length;

            // First pass: count how many bytes we will actually copy
            int length = 0;
            for (int i = 0; i < maxLength; i++)
            {
                byte b = data[start + i];
                if (b >= 0x20 && b <= 0x7E)
                {
                    length++;
                }
                else if (bStopAtNoAscii)
                {
                    break;
                }
                else
                {
                    length++; // Will replace with replaceNoAscii
                }
            }

            if (length == 0 && promptLength == 0)
                return string.Empty;

            char[] chars = new char[promptLength + length];
            int index = 0;

            // Copy prompt
            if (promptLength > 0)
            {
                for (int i = 0; i < promptLength; i++)
                    chars[index++] = prompt[i];
            }

            // Copy data
            for (int i = 0; i < maxLength && index < chars.Length; i++)
            {
                byte b = data[start + i];
                if (b >= 0x20 && b <= 0x7E)
                {
                    chars[index++] = (char)b;
                }
                else
                {
                    if (bStopAtNoAscii)
                        break;
                    else
                        chars[index++] = replaceNoAscii;
                }
            }

            return new string(chars, 0, index);
        }
    }

    public static class CharLoraExtensions
    {
        public static int HexNibble(this char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            return -1;
        }
    }

    public static class StringArrayLoraExtensions
    {
        public static string JoinArgs(this string[] p, char delimiter = ',')
        {
            if (p == null || p.Length == 0) return "";
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < p.Length; i++)
            {
                if (i > 0)
                    sb.Append(delimiter);   

                sb.Append(p[i]);
            }
            return sb.ToString();
        }
    }

    public static class StringLoraExtensions
    {
        public static bool TryParseUint(this string s, out uint value)
        {
            value = 0;
            if (s == null) return false;

            s = s.Trim();

            // HEX: 0x....
            if (s.Length > 2 && s[0] == '0' && (s[1] == 'x' || s[1] == 'X'))
            {
                uint result = 0;

                for (int i = 2; i < s.Length; i++)
                {
                    int nib = s[i].HexNibble();
                    if (nib < 0) return false;

                    result = (result << 4) | (uint)nib;
                }

                value = result;
                return true;
            }

            // DECIMAL
            uint dec = 0;

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c < '0' || c > '9') return false;

                dec = dec * 10 + (uint)(c - '0');
            }

            value = dec;
            return true;
        }
        public static bool TryParseInt(this string s, out int value)
        {
            value = 0;
            if (s == null) return false;

            s = s.Trim();

            // HEX
            if (s.Length > 2 && s[0] == '0' && (s[1] == 'x' || s[1] == 'X'))
            {
                uint tmp = 0;

                for (int i = 2; i < s.Length; i++)
                {
                    int nib = s[i].HexNibble();
                    if (nib < 0) return false;

                    tmp = (tmp << 4) | (uint)nib;
                }

                value = (int)tmp;
                return true;
            }

            // DECIMAL
            int dec = 0;
            bool neg = false;
            int idx = 0;

            if (s.Length > 0 && s[0] == '-')
            {
                neg = true;
                idx = 1;
            }

            for (; idx < s.Length; idx++)
            {
                char c = s[idx];
                if (c < '0' || c > '9') return false;

                dec = dec * 10 + (c - '0');
            }

            value = neg ? -dec : dec;
            return true;
        }
        public static bool TryParseByte(this string s, out byte value)
        {
            value = 0;
            if (s == null) return false;

            s = s.Trim();

            // HEX
            if (s.Length > 2 && s[0] == '0' && (s[1] == 'x' || s[1] == 'X'))
            {
                int result = 0;

                for (int i = 2; i < s.Length; i++)
                {
                    int nib = s[i].HexNibble();
                    if (nib < 0) return false;

                    result = (result << 4) | nib;
                    if (result > 255) return false;
                }

                value = (byte)result;
                return true;
            }

            // DECIMAL
            int dec = 0;

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c < '0' || c > '9') return false;

                dec = dec * 10 + (c - '0');
                if (dec > 255) return false;
            }

            value = (byte)dec;
            return true;
        }
        public static bool TryParseUshort(this string s, out ushort value)
        {
            value = 0;
            if (s == null) return false;

            s = s.Trim();

            // HEX
            if (s.Length > 2 && s[0] == '0' && (s[1] == 'x' || s[1] == 'X'))
            {
                int result = 0;

                for (int i = 2; i < s.Length; i++)
                {
                    int nib = s[i].HexNibble();
                    if (nib < 0) return false;

                    result = (result << 4) | nib;
                    if (result > 65535) return false;
                }

                value = (ushort)result;
                return true;
            }

            // DECIMAL
            int dec = 0;

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c < '0' || c > '9') return false;

                dec = dec * 10 + (c - '0');
                if (dec > 65535) return false;
            }

            value = (ushort)dec;
            return true;
        }
        public static string RemoveChars(this string input, char[] charsToRemove)
        {
            var sb = new System.Text.StringBuilder(input.Length);

            foreach (char c in input)
            {
                bool skip = false;

                for (int i = 0; i < charsToRemove.Length; i++)
                {
                    if (c == charsToRemove[i])
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
        public static byte[] HexToBytes(this string hex)
        {
            int len = hex.Length;

            if ((len & 1) != 0)
                return null;

            byte[] data = new byte[len / 2];

            for (int i = 0; i < len; i += 2)
            {
                int hi = hex[i].HexNibble();
                int lo = hex[i + 1].HexNibble();

                if (hi < 0 || lo < 0)
                    return null;

                data[i / 2] = (byte)((hi << 4) | lo);
            }

            return data;
        }

        // "0x5000" -> 0x5000 (hex)
        // "222" -> 222 (decimal)
        public static bool TryParseLoraAddress(this string text, out ushort value)
        {
            value = 0;
            if (string.IsNullOrEmpty(text)) return false;
            string s = text.Trim();
            if (s.StartsWith("0x") || s.StartsWith("0X"))
            {
                string hex = s.Substring(2);
                return hex.TryParseHexCharsToUInt32(out value);       // Extensions.cs (NFAppLib)
            }
            return s.TryParseUshort(out value);                        
        }

        // Consumer-side unwrap rule (producer/consumer convention, agreed 09/11/2026):
        // the producer wraps a text arg in quotes ONLY when it contains a comma, so it
        // travels as ONE arg through the transport. The transport (WiFiAgent SplitAtArgs
        // v2.4.8.5) KEEPS the producer's quote chars - the wrapper travels the chain as
        // given (route_entry shows it), so this consumer IS ACTIVE on the HTTP path.
        // It strips the outer quote pair iff the arg starts AND ends with '"' AND
        // contains a comma. Naked args pass through untouched; quotes around comma-free
        // text are payload, NOT syntax (pay-safe for JSON/braced payloads).
        // Moved here from LoraModule v3.0.0.10 -> LoraModule v3.0.0.13 calls
        // args.TextArg() (extension, using NFAppLib already present) — TextArg =
        // rejoin-if-split + this unwrap, see below.
        // NOTE v3.0.0.12: SendHandler FIRST rejoins args[1..] with ',' when the
        // transport split the payload at a comma inside a JSON string value
        // (trace 23:40:14: "{"a":"1,1"}" -> args[1]="{"a":"1, args[2]="1"}").
        // v1.0.2.4: the rejoin is now INSIDE TextArg(this string[], int index=1).
        //public static string UnwrapTextArg(this string arg)
        //{
        //    if (string.IsNullOrEmpty(arg)) return "";
        //    if (arg.Length >= 2 && arg[0] == '"' && arg[arg.Length - 1] == '"' && arg.IndexOf(',') >= 0)
        //        return arg.Substring(1, arg.Length - 2);
        //    return arg;
        //}

        // Reusable consumer-side text-payload extraction (v1.0.2.4) — THE one-liner for
        // ANY subscriber that consumes a text payload following a fixed argument prefix:
        //     string data = args.TextArg();          // payload after args[0]
        //     string data = args.TextArg(2);         // payload after args[0..1] (prefix)
        // It is EXACTLY "rejoin-if-split + UnwrapTextArg" in one call, so a subscriber
        // NEVER re-implements the quote-parity/rejoin logic:
        //   * one-piece payload (normal):   returns args[index].UnwrapTextArg()
        //   * split payload (comma inside a JSON string VALUE — trace 23:40:14:
        //     "{"a":"1,1"}" -> args[1]="{"a":"1, args[2]="1"}"): REJOINS args[index..]
        //     with ',' (the transport consumed the separator commas; rejoin restores them
        //     exactly — identity), then UnwrapTextArg strips the outer pair.
        //     DEVICE-CONFIRMED 23:53:24: {"a":"1,1"} TX byte-perfect (comma 2C in data).
        // Returns "" when the payload arg is missing. For commands with GENUINELY multiple
        // argument slots (each arg stands alone), use the raw args[] array instead — this
        // helper is for the "everything after the prefix is ONE payload" command shape
        // (e.g. LORA/SEND to,<text payload>).
        //public static string TextArg(this string[] args, int index = 1)
        //{
        //    if (args == null || args.Length <= index) return "";
        //    if (args.Length == index + 1)
        //        return args[index].UnwrapTextArg();
        //    var sb = new System.Text.StringBuilder();
        //    for (int i = index; i < args.Length; i++)
        //    {
        //        if (i > index) sb.Append(',');
        //        sb.Append(args[i]);
        //    }
        //    return sb.ToString().UnwrapTextArg();
        //}

    }




    #endregion

    #region Utils
    public class ByteBuffer
    {
        private byte[] _buffer;
        private int _index;

        public ByteBuffer(int size)
        {
            _buffer = new byte[size];
            _index = 0;
        }

        public bool Add(byte value)
        {
            if (_index >= _buffer.Length)
                return false; // no more space

            _buffer[_index++] = value;
            return true;
        }

        public byte[] ToArray()
        {
            byte[] result = new byte[_index];
            for (int i = 0; i < _index; i++)
                result[i] = _buffer[i];
            return result;
        }

        public int Count => _index;
    }

    #endregion


    public static class FastRandom
    {
        // Simple thread-safe PRNG (Xorshift32)
        private static uint _seed = 0xA341316C;

        private static int Next(int max)
        {
            // Xorshift32
            uint x = _seed;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _seed = x;

            return (int)(x % (uint)max);
        }

        public static void DelayAsync(DelayHandler handler, int minMs = 500, int maxMs = 2000)
        {
            if (handler == null)
                return;

            if (minMs < 0) minMs = 0;
            if (maxMs < minMs) maxMs = minMs;

            int range = maxMs - minMs + 1;
            int delay = minMs + Next(range);

            new Thread(() =>
            {
                try
                {
                    Thread.Sleep(delay);
                    handler(delay);
                }
                catch
                {
                    // swallow NF exceptions
                }
            })
            { Priority = ThreadPriority.BelowNormal }.Start();
        }
    }


}
