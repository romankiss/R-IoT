// written by Roman Kiss, February 29th, 2024,
// version: 1.1.0.0 March 13,2024
//
// tool for fonts: https://xantorohara.github.io/led-matrix-editor/#0000000000a000e0
// example: https://github.com/CreepyMemes/SimpleHT16K33
//
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace MyLib
{
    public static class ArrayExtension
    {
        public static byte[] ConvertToByteArray(this ulong[] first)
        {
            byte[] buffer = new byte[first.Length * 8];
            for (int i = 0; i < first.Length; i++)
            {
                BitConverter.GetBytes(first[i]).CopyTo(buffer, i * 8);
            }
            return buffer;
        }

        //https://stackoverflow.com/questions/812022/c-sharp-bitwise-rotate-left-and-rotate-right
        public static ulong RotateRight(this ulong value) => (value >> 1) | (value << 63);
        public static ulong RotateRight(this ulong value, int count) => (value >> count) | (value << (64 - count));
        public static ulong RotateLeft(this ulong value) => (value << 1) | (value >> 63);
        public static ulong RotateLeft(this ulong value, int count) => (value << count) | (value >> (64 - count));
        public static void Rol(ref this ulong value) => value = (value << 1) | (value >> 63);
        public static void Rol(ref this ulong value, int count) => value = (value << count) | (value >> (64 - count));
        public static void Ror(ref this ulong value) => value = (value << 63) | (value >> 1);
        public static void Ror(ref this ulong value, int count) => value = (value << (64 - count)) | (value >> count);

    }

    public enum BlinkType
    {
        Off = 0,       // off
        Slow = 1,       // 2 Hz blink
        Fast = 2,       // 1 Hz blink
        ExtraFast = 3   // 0.5 Hz blink
    }

    public enum ColorMode
    {
        Default = 0,    // Color is done by manufacture
        Green = 1,      // Bi-Color is Green
        Red = 2,        // Bi-Color is Red
        Orange = 3      // Bi-Color is Orange
    }

    public enum BackpackWiring
    {
        Default = 0,            // 
        Backpack8x8 = 1,
        Backpack8x8_HW572 = 2,  // 
        Backpack8x8_BiColor = 4,  // 
        Backpack16x8 = 8,       //
        BackPack8x16_miniAdafruit = 16,       //
    }

    public class SimpleHT16K33
    {
        // chip values
        const byte HT16K33_BLINK_CMD = 0x80;        // BLINK setting
        const byte HT16K33_TURN_OSCILLATOR = 0x21;  // turn on oscillator
        const byte HT16K33_BLINK_DISPLAYON = 0x01;  // display on
        const byte HT16K33_CMD_BRIGHTNESS = 0xE0;   // BRIGHTNESS setting
        public const byte DefaultI2cAddress = 0x70;
        //
        private I2cDevice _i2cDevice = null;
        private AutoResetEvent are = new AutoResetEvent(true); // only one writeString function at the time
        private const int MAX_TEXT_LENGTH = 64;
        private const int MAX_BIG_BUFFER_SIZE = MAX_TEXT_LENGTH * 8; //512; 
        private byte[] _bigBuffer = new byte[MAX_BIG_BUFFER_SIZE];
        private int noCirculate = 0;  // flag for circulation of the text 
        //
        //differences on the backpack wiring for populare products 
        private bool bBackpack8x8 = false;
        private bool bBackpack8x8_HW572 = false;
        private bool bBackpack8x8_BiColor = false;
        private bool bBackPack16x8 = false;
        private bool bBackPack8x16_miniAdafruit = false;
        //
        // Ascii font 8x8 pixels rotated by right to allow direct stremming in the HT16K33 device
        private static readonly ulong[] ASCII = {
            0x0000000000000000,     // space
            0x000060fafa600000,     // !
            0x0000607000706000,     // "
            0x0028fefe28fefe28,     // #
            0x0000485cd6d67424,     // $
            0x00466630180c6662,     // %
            0x00125eecbaf25e0c,     // &
            0x0000000000c0e020,     // '
            0x00000082c67c3800,     // (
            0x000000387cc68200,     // )
            0x082a3e1c3e2a0800,     // *
            0x000010107c7c1010,     // +
            0x0000000006070100,     // ,
            0x0000101010101000,     // -
            0x0000000006060000,     // .
            0x0080c06030180c06,     // /
            0x007cfeb29a8efe7c,     // 0 
            0x00000202fefe4202,     // 1 
            0x000066f6929ace46,     // 2
            0x00006cfe9292c644,     // 3  
            0x000afefeca683818,     // 4 
            0x00009cbea2a2e6e4,     // 5
            0x00000c9e92d27e3c,     // 6
            0x0000e0f09e8ec0c0,     // 7
            0x00006cfe9292fe6c,     // 8
            0x000078fc9692f260,     // 9
            0x0000000066660000,     // :
            0x0000000066670100,     // ;
            0x00000082c66c3810,     // <
            0x0000242424240000,     // =
            0x000010386cc68200,     // >
            0x000060f09a8ac040,     // ?
            0x0004325a5a423c00,     // @
            0x00003e7ec8c87e3e,     // A
            0x006cfe9292fefe82,     // B
            0x0044c68282c67c38,     // C
            0x00387cc682fefe82,     // D
            0x00c682ba92fefe82,     // E
            0x00c080b892fefe82,     // F swapped:c0c0c0f0f0c0fcfc, right:00c080b892fefe82
            0x004ece8a82c67c38,     // G
            0x0000fefe1010fefe,     // H
            0x00000082fefe8200,     // I
            0x0080fcfe82020e0c,     // J
            0x00c6ee3810fefe82,     // K
            0x000e060282fefe82,     // L
            0x00fefe703870fefe,     // M
            0x00fefe183060fefe,     // N
            0x00387cc682c67c38,     // O
            0x0060f09092fefe82,     // P
            0x00007afe8e84fc78,     // Q
            0x0066fe9890fefe82,     // R
            0x00004cce9ab2f664,     // S
            0x0000c082fefe82c0,     // T
            0x0000fefe0202fefe,     // U
            0x0000f8fc0606fcf8,     // V
            0x00fefe0c180cfefe,     // W
            0x00c2e63c183ce6c2,     // X
            0x0000e0f21e1ef2e0,     // Y
            0x00cee6b29a8ec6e2,     // Z
            0x0000008282fefe00,     // [
            0x00060c183060c080,     // '\'
            0x000000fefe828200,     // ]
            0x00103060c0603010,     // ^
            0x0001010101010100,     // _
            0x00000020e0c00000,     // `
            0x00021e3c2a2a2e04,     // a
            0x000c1e1212fcfe82,     // b
            0x0000143622223e1c,     // c
            0x0002fefc92121e0c,     // d
            0x0000183a2a2a3e1c,     // e
            0x000040c092fe7e12,     // f
            0x00203e1f25253d19,     // g
            0x001e3e2010fefe82,     // h
            0x00000002bebe2200,     // i
            0x0000bebf01010706,     // j
            0x0022361c08fefe82,     // k
            0x00000002fefe8200,     // l
            0x001e3e381c183e3e,     // m
            0x00001e3e20203e3e,     // n
            0x00001c3e22223e1c,     // o
            0x00183c24251f3f21,     // p
            0x00213f1f25243c18,     // q
            0x00183820321e3e22,     // r
            0x0000242e2a2a3a12,     // s
            0x00002422fe7c2000,     // t
            0x00023e3c02023e3c,     // u
            0x0000383c06063c38,     // v
            0x003c3e0e1c0e3e3c,     // w
            0x0022361c081c3622,     // x
            0x00003e3f05053d39,     // y
            0x000026323a2e2632,     // z
            0x00008282ee7c1010,     // {
            0x000000fefe000000,     // |
            0x000010107cee8282,     // }
            0x0080c040c080c040      // ~
        };

        /// <summary>
        /// Create object for handling Matrix8x8, Matrix8x16, Matrix16x8 and Matrix8x8 Bi-Color
        /// </summary>
        /// <param name="i2cDevice"></param>
        /// <param name="backpackWiring parameter to specify a wiring beetween chip and Matrix
        /// Default value is BackpackWiring.Default and it used for accepting a setup based on the DeviceAddress"></param>
        /// Note, that the address A0 = 1 is used for Matrix8x8 devices, address A1 = 1 for specific backpack wiring HW-572 product
        public SimpleHT16K33(I2cDevice i2cDevice, BackpackWiring backpackWiring = BackpackWiring.Default)
        {
            _i2cDevice = i2cDevice;

            // config based on the address
            this.bBackpack8x8 = (DeviceAddress & 0x01) == 0x01;
            this.bBackpack8x8_HW572 = (DeviceAddress & 0x03) == 0x03;

            // overwrite config based on the code
            if (backpackWiring != BackpackWiring.Default)
            {
                bBackpack8x8 = ((backpackWiring & BackpackWiring.Backpack8x8) == BackpackWiring.Backpack8x8);
                bBackpack8x8_HW572 = ((backpackWiring & BackpackWiring.Backpack8x8_HW572) == BackpackWiring.Backpack8x8_HW572);
                bBackpack8x8_BiColor = ((backpackWiring & BackpackWiring.Backpack8x8_BiColor) == BackpackWiring.Backpack8x8_BiColor);
                bBackPack16x8 = ((backpackWiring & BackpackWiring.Backpack16x8) == BackpackWiring.Backpack16x8);
                bBackPack8x16_miniAdafruit = ((backpackWiring & BackpackWiring.BackPack8x16_miniAdafruit) == BackpackWiring.BackPack8x16_miniAdafruit);
            }

            Debug.WriteLine($"SimpleHT16K33[0x{DeviceAddress.ToString("x")}] created for: " +
                $"Backpack8x8={bBackpack8x8}, Backpack8x8_HW572={bBackpack8x8_HW572}, Backpack8x8_BiColor={bBackpack8x8_BiColor}, BackPack16x8={bBackPack16x8}, BackPack8x16_miniAdafruit={bBackPack8x16_miniAdafruit}");
        }

        public static SimpleHT16K33 Create(int busId = 1, BackpackWiring backpackWiring = BackpackWiring.Default)
        {
            I2cDevice i2c_ht16k33 = null;
            for (int ii = 0; ii < 8; ii++)
            {
                i2c_ht16k33 = new(new I2cConnectionSettings(busId, SimpleHT16K33.DefaultI2cAddress + ii)); 
                var res = i2c_ht16k33.WriteByte(0x07);
                if(res.Status == I2cTransferStatus.FullTransfer)
                    return new SimpleHT16K33(i2c_ht16k33, backpackWiring);
            }
            return null;
        }

        public int DeviceAddress => _i2cDevice.ConnectionSettings.DeviceAddress;

        public void Init()
        {
            _i2cDevice.WriteByte(HT16K33_TURN_OSCILLATOR);  //0x21
        }

        public void SetBrightness(byte brightness = 0)
        {
            _i2cDevice.WriteByte((byte)(HT16K33_CMD_BRIGHTNESS | brightness));   //0xE0
        }

        public void SetBlinkRate(BlinkType blinkType = BlinkType.Off)
        {
            _i2cDevice.WriteByte((byte)(HT16K33_BLINK_CMD | HT16K33_BLINK_DISPLAYON | ((byte)blinkType << 1)));  //0x80 | 0x01
        }

        public void Clear()
        {
            Array.Clear(_bigBuffer, 0, MAX_BIG_BUFFER_SIZE);
        }

        // this is the lowest driver level when 2x8 bytes is written to the chip for showing pixels
        public void write16BytesToDisplay(byte[] fromArray, int cursor = 0, ColorMode colorMode = ColorMode.Default )
        {
            if (fromArray == null)
                return;

            int sizeOfArray = fromArray.Length;
            byte[] buffer = new byte[17];       // cmd(1 byte) + data(2*8 bytes); Low1 High1 Low2 High2 ... Low8 High8
            buffer[0] = 0x00;                   // start at address $00
     
            for (byte col = 0; col < 8; col++)
            {
                int posRight = cursor + 7 + col;
                byte valRight = (posRight >= sizeOfArray || posRight < 0) ? (byte)0x00 : fromArray[posRight];

                if (bBackpack8x8)
                {
                    // Backpack 8x8
                    if (bBackpack8x8_HW572)
                    {
                        // this is a special case for handling HW-572 device (8x8 with Grove connector)
                        byte currentcol = (byte)(1 + 2 * (7 - col));
                        currentcol = (byte)(currentcol == 7 ? 11 : currentcol == 11 ? 7 : currentcol);
                        buffer[currentcol] = valRight;
                    }
                    else
                    {
                        if (bBackpack8x8_BiColor)
                        {
                            if (colorMode == ColorMode.Default || colorMode == ColorMode.Green)
                            {
                                buffer[1 + 2 * col] = valRight;
                            }
                            else if (colorMode == ColorMode.Orange)
                            {
                                buffer[1 + 2 * col] = valRight;
                                buffer[2 + 2 * col] = valRight;
                            }
                            else
                            {
                                buffer[2 + 2 * col] = valRight;
                            }
                        }
                        else
                        {
                            //Adafruit 0.8 8x8 Led(mini)
                            buffer[1 + 2 * col] = (byte)((valRight >> 1) | (valRight << 7));
                        }
                    }
                }
                else
                {
                    //BackPack 8x16 or 16x8
                    int posLeft = cursor - 1 + col;
                    byte valLeft = (posLeft >= sizeOfArray || posLeft < 0) ? (byte)0x00 : fromArray[posLeft];

                    if (bBackPack16x8)
                    {
                        //open smart 16x8  
                        buffer[1 + 2 * (7 - col)] = valRight;
                        buffer[2 + 2 * (7 - col)] = valLeft;
                    }
                    else
                    {
                        // adafruit8x16
                        if (bBackPack8x16_miniAdafruit)
                        {
                            buffer[1 + 2 * col] = valLeft;
                            buffer[2 + 2 * col] = valRight;
                        }
                        else
                        {
                            // rotate 8x8 right for scrolling backpack from 8x16 to 16x8 
                            for (int ii = 0; ii < 8; ii++)
                            {
                                buffer[1 + 2 * ii] = (byte)(buffer[1 + 2 * ii] << 1 | (valRight >> ii & 0x01));
                                buffer[2 + 2 * ii] = (byte)(buffer[2 + 2 * ii] << 1 | (valLeft >> ii & 0x01));
                            }
                        }
                    }
                }
            }
            _i2cDevice.Write(new SpanByte(buffer));
        }
     
        // write each character of the text using their font8x8 matrix 8x8 bytes such as ulong number (64 bits)
        // the text is truncated at the 64th character
        public int WriteString(string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;

            Interlocked.Exchange(ref noCirculate, 0);   // stop message circulation
            are.WaitOne();                              // wait for exit from display, release only one thread and then block it
            //
            int picNumber = s == null ? 0 : s.Length;
            if (picNumber > (MAX_BIG_BUFFER_SIZE / 8))
            {
                picNumber = MAX_BIG_BUFFER_SIZE / 8 - 1;
                Debug.WriteLine($"String has been truncated to {picNumber} from {s.Length}");
            }
            if (picNumber > 0)
            {

                Array.Clear(_bigBuffer, 0, MAX_BIG_BUFFER_SIZE);
                for (int m = 0; m < picNumber; m++)
                {
                    BitConverter.GetBytes(ASCII[s[m] - 32]).CopyTo(_bigBuffer, m * 8);
                }
            }
            return picNumber * 8;
        }

        // shift 16 bytes from the fromArray at the cursorStart to the HT16K33 device then sleep 50 ms, increase cursor by 1 and again until the cursorEnd is reach it 
        // in the case of bCirculate = true and noCirculate is not 0, made this process again and again.
        // Note, that zero byte is pushed to the device when the cursor is out of the buffer range
        // function also can handle device8x8 or device16x8 on the runtime
        public void writeDisplay(byte[] fromArray, int cursorEnd = 15, int cursorStart = -15, int scrollingTimeInMs = 50, bool bCirculate = false, ColorMode colorMode = ColorMode.Default)
        {
            lock (_i2cDevice)
            {
                do
                {
                    for (int cursor = cursorStart; cursor < cursorEnd; cursor += 1)
                    {
                        if (bCirculate && Interlocked.CompareExchange(ref noCirculate, 0, 0) == 0)
                        {
                            // stop and exit from this function
                            return;
                        }
                        write16BytesToDisplay(fromArray, cursor, colorMode);
                        Thread.Sleep(scrollingTimeInMs);
                    }

                } while (bCirculate);
            }
        }

        #region API functions
        // Simple show the text in the scrolling mode on the device in the background task
        public void ShowMessageAsync(string s, bool bScrollLastCharacters = true, int msPerLetter = 50, ColorMode colorMode = ColorMode.Default)
        {
            int lastPixel = WriteString(s);
            if (!bScrollLastCharacters)
                lastPixel -= 15;
            var task = new Thread(() =>
            {
                writeDisplay(_bigBuffer, scrollingTimeInMs: msPerLetter, cursorEnd: lastPixel, colorMode: colorMode);
                are.Set();
            }) { Priority = ThreadPriority.BelowNormal };
            task.Start();
        }

        // Simple show the text in the scrolling mode on the device in the synchronnously manner
        public void ShowMessage(string s, bool bScrollLastCharacters = true, int msPerLetter = 50, ColorMode colorMode = ColorMode.Default)
        {
            int lastPixel = WriteString(s);
            if (!bScrollLastCharacters)
                lastPixel -= 15;
            writeDisplay(_bigBuffer, scrollingTimeInMs: msPerLetter, cursorEnd: lastPixel, colorMode: colorMode);
            are.Set();
        }

        // Simple show and circulate text on the device in the background process
        // the circulate text can be stopped by inserting a new text
        public void ShowAndCirculateMessageAsync(string s, int msPerLetter = 50, ColorMode colorMode = ColorMode.Default)
        {
            // show and circulate text until a new text is comming
            int lastPixel = WriteString(s);
            Interlocked.Increment(ref noCirculate);
            var task = new Thread(() => 
            {
                writeDisplay(_bigBuffer, scrollingTimeInMs: msPerLetter, cursorEnd: lastPixel, bCirculate: true, colorMode: colorMode);
                are.Set();
            }) { Priority = ThreadPriority.BelowNormal };
            task.Start();
        }

        // show scrollable byte array on the device in the sync manner
        public void ShowArray(byte[] byteArray, bool bScrollLastCharacters = true, int msPerLetter = 50, ColorMode colorMode = ColorMode.Default)
        {
            int lastPixel = byteArray.Length + 1;
            if (!bScrollLastCharacters)
                lastPixel -= 15;
            writeDisplay(byteArray, scrollingTimeInMs: msPerLetter, cursorEnd: lastPixel, colorMode: colorMode);
        }

        // show scrollable array of ulong (8x8 = 64 bits) on the device in the sync manner
        public void ShowArray(ulong[] matrix8x8, bool bScrollLastCharacters = true, int msPerLetter = 50, ColorMode colorMode = ColorMode.Default)
        {
            int lastPixel = matrix8x8.Length * 8 + 1;
            if (!bScrollLastCharacters)
                lastPixel -= 15;
            writeDisplay(matrix8x8.ConvertToByteArray(), scrollingTimeInMs: msPerLetter, cursorEnd: lastPixel, colorMode: colorMode);
        }

        public void ShowMatrix(ulong matrix8x8, bool bScrollLastCharacters = true, int msPerLetter = 50, ColorMode colorMode = ColorMode.Default)
        {
            int lastPixel = 8 + 1;
            if (!bScrollLastCharacters)
                lastPixel -= 15;
            writeDisplay(BitConverter.GetBytes(matrix8x8), cursorEnd: lastPixel, scrollingTimeInMs: msPerLetter, colorMode: colorMode);
        }
        #endregion
    }
}
