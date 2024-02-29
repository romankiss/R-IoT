// written by Roman Kiss, February 29th, 2024
// tool for fonts: https://xantorohara.github.io/led-matrix-editor/#0000000000a000e0
//
//
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace NFAppM5CapsuleS3_MQTT
{
    public enum BlinkType
    {
        Off = 0,
        Slow = 1,
        Fast = 2
    }
    
    public class SimpleHT16K33
    {
        private I2cDevice _i2cDevice = null;
        private AutoResetEvent are = new AutoResetEvent(true); // only one writeString function at the time
        private const int MAX_TEXT_LENGTH = 64;
        private const int MAX_BIG_BUFFER_SIZE = MAX_TEXT_LENGTH * 8; //512; 
        private byte[] _bigBuffer = new byte[MAX_BIG_BUFFER_SIZE];  
        private int noCirculate = 0;  // flag for circulation of the text 
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
            0x0000101010101010,     // -
            0x0000000006060000,     // .
            0x0080c06030180c06,     // /
            0x00007e8181817e00,     // 0
            0x00000001FF410000,     // 1
            0x0000718985834100,     // 2
            0x00006e9191814200,     // 3 //0x1c2220201820221c,   
            0x0000ff4424140c00,     // 4 //0x20203e2224283020,
            0x00008e919191f200,     // 5
            0x00004e9191917e00,     // 6
            0x0000e09088878000,     // 7
            0x00006e9191916e00,     // 8
            0x00007e8989897200,     // 9
            0x0000000066660000,     // :
            0x0000000066670100,     // ;
            0x00000082c66c3810,     // <
            0x0000242424242424,     // =
            0x000010386cc68200,     // >
            0x000060f09a8ac040,     // ?
            0x0004325a5a423c00,     // @
            0x00003e7ec8c87e3e,     // A
            0x006cfe9292fefe82,     // B
            0x0044c68282c67c38,     // C
            0x00387cc682fefe82,     // D
            0x00c682ba92fefe82,     // E
            0x00c080b892fefe82,     // F
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

        public SimpleHT16K33(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        public void Init()
        {
            _i2cDevice.WriteByte(0x21);
        }

        public void SetBrightness(byte brightness = 0)
        {
            _i2cDevice.WriteByte((byte)(0xE0 | brightness));
        }

        public void SetBlinkRate(BlinkType blinkType = BlinkType.Off)
        {
            _i2cDevice.WriteByte((byte)(0x80 | 0x01 | ((byte)blinkType << 1)));
        }

        public void Clear()
        {
            Array.Clear(_bigBuffer, 0, MAX_BIG_BUFFER_SIZE);
        }


        // write each character of the text using their font8x8 matrix 8x8 bytes such as ulong number (64 bits)
        // if the text is longer than max configured 64 charaktes, it will be truncated (not wisible)
        public int WriteString(string s)
        {
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

        // shift 16 bytes from the cursorStart to the HT16K33 device then sleep 50 ms, increase cursor by 1 and again until the cursorEnd is reach it 
        // in the case of bCirculate = true and noCirculate is not 0, made this process again and again.
        // Note, that zero byte is pushed to the device when the cursor is out of the buffer range
        // function also can handle device8x8 or device16x8 on the runtime
        public void writeDisplay(int scrollingTimeInMs = 50, int cursorEnd = MAX_BIG_BUFFER_SIZE, int cusrsorStart = -15, bool bCirculate = false, bool bDevice8x8 = false)
        {
            int sizeOfBuffer = _bigBuffer.Length - 1;
            byte[] buffer = new byte[17];               // cmd(1 byte) + data(2*8 bytes); Low1 High1 Low2 High2 ... Low8 High8
            buffer[0] = 0x00;                           // start at address $00

            do
            {
                for (int cursor = cusrsorStart; cursor < cursorEnd; cursor += 1)
                {
                    if (bCirculate && Interlocked.CompareExchange(ref noCirculate, 0, 0) == 0)
                    {   
                        // stop and exit from this function
                        are.Set();
                        return;
                    }

                    // reverse to 8:15/0:7
                    for (byte col = 0; col < 8; col++)
                    {
                        int posLeft = cursor + 7 - col;
                        int posRight = cursor + 15 - col;
                        buffer[1 + 2 * col] = (posRight > sizeOfBuffer || posRight < 0) ? (byte)0x00 : _bigBuffer[posRight];
                        buffer[2 + 2 * col] = bDevice8x8 ? (byte)0x00 : (posLeft > sizeOfBuffer || posLeft < 0) ? (byte)0x00 : _bigBuffer[posLeft];
                    }
                    _i2cDevice.Write(new SpanByte(buffer));
                    Thread.Sleep(scrollingTimeInMs);
                }
            } while(bCirculate);
            are.Set();
        }

        #region API functions
        // Simple show the text in the scrolling mode on the device in the background task
        // // scrolling text can be stoped at the last two characters
        // the scrolling time can be changed
        public void ShowMessageAsync(string s, bool bScrollLastCharacters = true, int msPerLetter = 50, bool bDevice8x8 = false)
        {         
            int lastPixel = WriteString(s);
            if(!bScrollLastCharacters) 
                lastPixel -= 15;
            new Thread(() => writeDisplay(scrollingTimeInMs: msPerLetter, cursorEnd: lastPixel, bDevice8x8: bDevice8x8)).Start();
        }

        // Simple show the text in the scrolling mode on the device in the synchronnously manner
        // // scrolling text can be stoped at the last two characters
        // the scrolling time can be changed
        public void ShowMessage(string s, bool bScrollLastCharacters = true, int msPerLetter = 50, bool bDevice8x8 = false)
        {
            int lastPixel = WriteString(s);
            if (!bScrollLastCharacters)
                lastPixel -= 15;
            writeDisplay(scrollingTimeInMs: msPerLetter, cursorEnd:lastPixel, bDevice8x8: bDevice8x8);
        }

        // Simple show and circulate text on the device in the background process
        // the circulate text can be stopped by inserting a new text
        public void ShowAndCirculateMessageAsync(string s, int msPerLetter = 50, bool bDevice8x8 = false)
        {
            // show and circulate text until a new text is comming
            int lastPixel = WriteString(s);
            Interlocked.Increment(ref noCirculate);
            new Thread(() => writeDisplay(scrollingTimeInMs: msPerLetter, cursorEnd: lastPixel, bCirculate: true, bDevice8x8: bDevice8x8)).Start();
        }
        #endregion
    }
}
