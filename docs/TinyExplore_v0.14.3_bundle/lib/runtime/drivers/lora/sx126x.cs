// written by Roman Kiss, January 1, 2026
// version: 1.0.0.0  01/01/2026  initial version
//          1.0.0.1  02/01/2026  no rst line support
//          1.1.0.0  05/15/2026  changes for diagnostic AT comamnds, in the Reset (soft reset, unregistering callback, ...)
//          1.1.0.1  05/17/2026  cleanup code 
//          1.1.0.2  05/18/2026  cleanup code (moving same parts to the common.cs) 
//          1.1.0.3  05/18/2026  calibration image const 0xD8 -> 0xDB, BW, SF, CR enums
//          1.1.0.4  05/20/2026  adding LastPreamble
//          1.1.0.5  09/09/2026  fixing Reset():LastSetupLoRa; now driver can be disposed
//
//
//
//
using NFAppExtensions;
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

// https://cdn.sparkfun.com/assets/6/b/5/1/4/SX1262_datasheet.pdf
// https://github.com/Xinyuan-LilyGO/LilyGo-LoRa-Series/blob/master/lib/SX12XX-LoRa/src/SX126XLT.cpp#L1046
// https://github.com/HelTecAutomation/Heltec_ESP32/blob/master/src/driver/sx126x.h
// for testing https://protogen.marcgravell.com/decode


namespace NFAppLib
{
   
    public enum RadioOperatingModes
    {
        SLEEP = 0x00,      //! The radio is in sleep mode
        STDBY_RC,          //! The radio is in standby mode with RC oscillator
        STDBY_XOSC,        //! The radio is in standby mode with XOSC oscillator
        FS,                //! The radio is in frequency synthesis mode
        TX,                //! The radio is in transmit mode
        RX,                //! The radio is in receive mode
        RX_DC,             //! The radio is in receive duty cycle mode
        CAD                //! The radio is in channel activity detection mode
    }

    public enum RadioPacketTypes
    {
        GFSK = 0x00,
        LORA = 0x01,
        NONE = 0x0F,
    }
    public enum RadioStandbyModes
    {
        STDBY_RC = 0x00,
        STDBY_XOSC = 0x01,
    }
    
    public enum RadioCommands : byte
    {
        // --- Status / Registers ---
        GET_STATUS = 0xC0,
        WRITE_REGISTER = 0x0D,
        READ_REGISTER = 0x1D,
        WRITE_BUFFER = 0x0E,
        READ_BUFFER = 0x1E,

        // --- Operating Modes ---
        SET_SLEEP = 0x84,
        SET_STANDBY = 0x80,
        SET_FS = 0xC1,
        SET_TX = 0x83,
        SET_RX = 0x82,
        SET_RX_DUTY_CYCLE = 0x94,
        SET_CAD = 0xC5,
        SET_TX_CONTINUOUS_WAVE = 0xD1,
        SET_TX_CONTINUOUS_PREAMBLE = 0xD2,

        // --- Packet / Modulation ---
        SET_PACKET_TYPE = 0x8A,
        GET_PACKET_TYPE = 0x11,
        SET_RF_FREQUENCY = 0x86,
        SET_TX_PARAMS = 0x8E,
        SET_PA_CONFIG = 0x95,
        SET_CAD_PARAMS = 0x88,
        SET_BUFFER_BASE_ADDRESS = 0x8F,
        SET_MODULATION_PARAMS = 0x8B,
        SET_PACKET_PARAMS = 0x8C,
        SET_LORA_SYMB_TIMEOUT = 0xA0,

        // --- IRQ / Status ---
        GET_RX_BUFFER_STATUS = 0x13,
        GET_PACKET_STATUS = 0x14,
        GET_RSSI_INST = 0x15,
        GET_STATS = 0x10,
        RESET_STATS = 0x00,   // NOTE: This is correct for SX1262
        SET_DIO_IRQ_PARAMS = 0x08,
        GET_IRQ_STATUS = 0x12,
        CLEAR_IRQ_STATUS = 0x02,

        // --- Calibration ---
        CALIBRATE = 0x89,
        CALIBRATE_IMAGE = 0x98,

        // --- Power / RF Switch / TCXO ---
        SET_REGULATOR_MODE = 0x96,
        GET_DEVICE_ERRORS = 0x17,
        SET_TCXO_MODE = 0x97,
        SET_TX_FALLBACK_MODE = 0x93,
        SET_RF_SWITCH_MODE = 0x9D,
        SET_STOP_RX_TIMER_ON_PREAMBLE = 0x9F,
        SET_RX_GAIN = 0xAC,

        // Aliases for clarity
        SET_DIO3_AS_TCXO_CTRL = 0x97,
        SET_DIO2_AS_RF_SWITCH_CTRL = 0x9D
    }
    public enum RadioIrqMasks
    {
        IRQ_RADIO_NONE = 0x0000,                //  x     0     no interrupts
        IRQ_TX_DONE = 0x0001,                   //  0
        IRQ_RX_DONE = 0x0002,                   //  1
        IRQ_PREAMBLE_DETECTED = 0x0004,         //  2     2     preamble detected
        IRQ_SYNCWORD_VALID = 0x0008,            //  3     3     valid sync word detected
        IRQ_HEADER_VALID = 0x0010,              //  4     4     valid LoRa header received
        IRQ_HEADER_ERROR = 0x0020,              //  5     5     LoRa header CRC error
        IRQ_CRC_ERROR = 0x0040,                 //  6     6     wrong CRC received
        IRQ_CAD_DONE = 0x0080,                  //  7     7     channel activity detection finished
        IRQ_CAD_ACTIVITY_DETECTED = 0x0100,     //  8     8     channel activity detected
        IRQ_RX_TX_TIMEOUT = 0x0200,             //  9     9     Rx or Tx timeout
        IRQ_RADIO_ALL = 0x03FF,                 //  x     0     all interrupts
    }
    public enum RadioCmdStatus : byte
    {
        RESERVED = 0x00,
        RFU = 0x01,
        SUCCESS = 0x02,
        TIMEOUT = 0x03,
        ERROR = 0x04,
        FAIL = 0x05,
        TXDONE = 0x06,
        UNKNOWN = 0x07
    }
    public enum ChipMode : byte
    {
        UNUSED = 0x00,
        RFU = 0x01,
        STDBY_RC = 0x02,
        STDBY_XOSC = 0x03,
        FS = 0x04,
        RX = 0x05,
        TX = 0x06,
        RESERVED = 0x07,
    }
    public enum CodingRate : byte
    {
        Cr4_5 = 0x01,
        Cr4_6 = 0x02,
        Cr4_7 = 0x03,
        Cr4_8 = 0x04
    }
    public enum SpreadingFactor : byte
    {
        Sf5 = 5,
        Sf6 = 6,
        Sf7 = 7,
        Sf8 = 8,
        Sf9 = 9,
        Sf10 = 10,
        Sf11 = 11,
        Sf12 = 12
    }
    public enum Bandwidth : byte
    {
        /// <summary>Bandwidth 7.81 kHz</summary>
        Bw7_81 = 0x00,

        /// <summary>Bandwidth 10.42 kHz</summary>
        Bw10_42 = 0x08,

        /// <summary>Bandwidth 15.63 kHz</summary>
        Bw15_63 = 0x01,

        /// <summary>Bandwidth 20.83 kHz</summary>
        Bw20_83 = 0x09,

        /// <summary>Bandwidth 31.25 kHz</summary>
        Bw31_25 = 0x02,

        /// <summary>Bandwidth 41.67 kHz</summary>
        Bw41_67 = 0x0A,

        /// <summary>Bandwidth 62.50 kHz</summary>
        Bw62_50 = 0x03,

        /// <summary>Bandwidth 125.00 kHz</summary>
        Bw125_00 = 0x04,

        /// <summary>Bandwidth 250.00 kHz</summary>
        Bw250_00 = 0x05,

        /// <summary>Bandwidth 500.00 kHz</summary>
        Bw500_00 = 0x06
    }

    public struct PacketStatus
    {
        public byte ChipStatus;
        public double PacketRssi;    // dBm
        public double SignalRssi;    // dBm
        public double Snr;           // dB

        public override string ToString() => $"RSSI[{PacketRssi}]dBm | SigRSSI[{SignalRssi}]dBm | SNR[{Snr:F1}]dB";
    }
    public class SX1262 : SpiDevice, ILoRaDriver
    {
        readonly object _locker = new();
        ushort _loraAddress = 0;
        GpioController _ioctrl = null;
        GpioPin _rstLine = null;
        GpioPin _busyLine = null;
        GpioPin _irqLine = null;

        public event OnReceive OnPacketReceived = null;
        public event OnReceiveRxTag OnPacketRxTag = null;
        private AutoResetEvent _txDoneEvent = new AutoResetEvent(false);
        //private AutoResetEvent _rxDoneEvent = new AutoResetEvent(false);
        private static readonly byte[] EmptyBuffer = new byte[0];

        const int REG_LR_SYNCWORD = 0x0740;
        public const ushort LORA_MAC_PRIVATE_SYNCWORD = 0x1424;
        public const ushort LORA_MAC_PUBLIC_SYNCWORD = 0x3444;
        public const ushort LORA_MESTASTIC_SYNCWORD = 0x1424;  //0x002B;

        const byte MAXLEN_PACKET = 255;

        const byte STBY_RC_RFU = 0x52;
        const byte RX_RFU = 0x22;

        public string LastRxTag { get; set; } = "NONE";
        public string LastSetupLoRa { get; private set; } = string.Empty;
        public ushort LastPreamble { get; private set; } = 0;


        public SX1262(SpiConnectionSettings spiConfig, GpioController ioctrl, int pinBusy, int pinReset, int pinIrq, ushort loraAddress) : base(spiConfig)
        {
            _loraAddress = loraAddress;
            _ioctrl = ioctrl;

            if (pinReset >= 0) 
            {
                if (_ioctrl.IsPinOpen(pinReset)) _ioctrl.ClosePin(pinReset);
                _rstLine = _ioctrl.OpenPin(pinReset, PinMode.Output); 
            }
            if (pinBusy >= 0)
            {
                if (_ioctrl.IsPinOpen(pinBusy)) _ioctrl.ClosePin(pinBusy);
                _busyLine = _ioctrl.OpenPin(pinBusy, PinMode.Input);
            }
            if(pinIrq >= 0)
            {
                if(_ioctrl.IsPinOpen(pinIrq)) _ioctrl.ClosePin(pinIrq);
                _irqLine = _ioctrl.OpenPin(pinIrq, PinMode.InputPullUp);
            }
        }

        public new void Dispose()
        {
            if(_ioctrl != null && _irqLine != null) 
                _ioctrl.UnregisterCallbackForPinValueChangedEvent(_irqLine.PinNumber, OnDio1IrqHandler);
            _rstLine?.Dispose(); _rstLine = null;
            _busyLine?.Dispose(); _busyLine = null;
            base.Dispose();
        }

        #region Shortcut Helpers
        public string showIRQLine => $"irqLine[{_irqLine.Read()}]";
        public PinValue GetIRQLine => _irqLine.Read();
        public string Timestamp => DateTime.UtcNow.ToString("HH:mm:ss.fff");
        public byte RX_SUCCESS => 0x54;
        #endregion

        #region +++ IRQ Handler
        
        private void OnDio1IrqHandler(object sender, PinValueChangedEventArgs args)
        {
            string rcvRxTag = null;
            RcvPacketArgs rcvPacket = null;
            bool shouldRestartRx = false;
            ushort irqFlags = GetIrqStatus(out byte _chipStatus, $"\r\n⭐[{Timestamp}]BeginIrqHandler ");
            if (irqFlags == (ushort)RadioIrqMasks.IRQ_RADIO_NONE)
            {
                shouldRestartRx = true;
                ClearIrqStatus(RadioIrqMasks.IRQ_RADIO_ALL);
            }

            //TX Done(bit 0)
            if (irqFlags.Has(RadioIrqMasks.IRQ_TX_DONE))
            {
                Debug.WriteLine("🚩 IRQ_TX_DONE");
                _txDoneEvent.Set(); // Signal TX completion
                ClearIrqStatus(RadioIrqMasks.IRQ_TX_DONE);
            }

            // RX Done
            if (irqFlags.Has(RadioIrqMasks.IRQ_RX_DONE) || _chipStatus == RX_SUCCESS)
            {
                #region RX_Done
                if (irqFlags.Has(RadioIrqMasks.IRQ_RX_TX_TIMEOUT))
                    Thread.Sleep(1);

                bool isHeaderValid = irqFlags.Has(RadioIrqMasks.IRQ_HEADER_VALID) || (_chipStatus == RX_SUCCESS);
                bool isCRCError = irqFlags.Has(RadioIrqMasks.IRQ_CRC_ERROR);

                var pktStatus = GetPacketStatus();
                Debug.WriteLine((!RadioCmdStatus.SUCCESS.Eq(pktStatus.ChipStatus) ? $"⚠ Wrong PACKET: {pktStatus.ChipStatus.ToText("Status")} | " : "pktStat] ") + $"{pktStatus}");

                // Read and process payload 
                byte bufStatus = GetRxBufferStatus(out byte payloadLength, out byte fifoStartPointer, out byte endOfData);
                bool isBufStatusValid = RadioCmdStatus.SUCCESS.Eq(bufStatus);
                bool payloadLengthValid = payloadLength > 0 && payloadLength <= MAXLEN_PACKET - 2; //Len+EOD bytes

                // get the payload
                if (isBufStatusValid && isHeaderValid && !isCRCError && payloadLengthValid)
                {
                    byte DataAt = (byte)(fifoStartPointer + 1); // LEN + Data + EOD
                    byte[] payload = spiReadBuffer(DataAt, payloadLength, prompt: "[data] ");

                    // check the crc8 wrapper if the endOfData is not 0x00
                    if (payload.IsComputedChecksumValidOrEOD(endOfData))
                    {
                        Debug.WriteLine($"✅ VALID RX: {pktStatus.ChipStatus.ToText("Status")} | EOD[0x{endOfData:X2}] | Len={payloadLength} | StartAt=0x{DataAt:X2} {payload.ToText(prompt: "| ")}");

                        // Process payload...
                        if (OnPacketReceived != null)
                        {
                            rcvPacket = new RcvPacketArgs() { Packet = payload, RSSI = (short)pktStatus.PacketRssi, SNR = (short)pktStatus.Snr };
                            LastRxTag = $"RX:OK;IRQ=0x{irqFlags:X4};RSSI={pktStatus.PacketRssi};SNR={pktStatus.Snr};LEN={payloadLength};CRC=OK;HDR=OK;SETUP={LastSetupLoRa}";
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"❎ Wrong BUFFER: DISCARDED, reason: dataCRC8[0x{endOfData:X2}] is not valid");
                        LastRxTag = $"RX:ERR;IRQ=0x{irqFlags:X4};REASON=CRC_DATA_ERROR;SETUP={LastSetupLoRa}";
                        rcvRxTag = LastRxTag;
                        // try to decode it as Meshtastic packet
                        // to do
                    }
                }
                else
                {
                    string reason = !isHeaderValid ? "Header | " : isCRCError ? "CRC_ERROR | " : !payloadLengthValid ? $"Length={payloadLength} | " : !isBufStatusValid ? $"{pktStatus.ChipStatus.ToText("Status")}" : "?";
                    LastRxTag =$"RX:ERR;IRQ=0x{irqFlags:X4};REASON={reason};SETUP={LastSetupLoRa}";
                    rcvRxTag = LastRxTag;
                    Debug.WriteLine($"❎ Wrong BUFFER: DISCARDED, reason: {reason.TrimEnd(' ', '|')}");
                }

                shouldRestartRx = true; // SX1262 leaves RX after RX_DONE
                ClearIrqStatus(RadioIrqMasks.IRQ_RX_DONE | RadioIrqMasks.IRQ_HEADER_VALID | RadioIrqMasks.IRQ_CRC_ERROR | RadioIrqMasks.IRQ_HEADER_ERROR);
                #endregion
            }

            if (irqFlags.Has(RadioIrqMasks.IRQ_RX_TX_TIMEOUT))
            {
                shouldRestartRx = true;
                LastRxTag = $"RX:ERR;IRQ=0x{irqFlags:X4};REASON=TIMEOUT;SETUP={LastSetupLoRa}";
                rcvRxTag = LastRxTag;
                ClearIrqStatus(RadioIrqMasks.IRQ_RX_TX_TIMEOUT);
            }

            if (irqFlags.HasAll(RadioIrqMasks.IRQ_CRC_ERROR | RadioIrqMasks.IRQ_HEADER_ERROR))
            {
                // Packet invalid
                shouldRestartRx = true;
                Debug.WriteLine("🏁 DISCARDED: RX Done with CRC/Header error.");
                ClearIrqStatus(RadioIrqMasks.IRQ_CRC_ERROR | RadioIrqMasks.IRQ_HEADER_ERROR);
            }

            if (irqFlags.Has(RadioIrqMasks.IRQ_PREAMBLE_DETECTED))
            {
                // You usually keep RX alive here — no restart yet
                LastRxTag = $"RX:PDETECT;IRQ=0x{irqFlags:X4};SETUP={LastSetupLoRa}";
                rcvRxTag = LastRxTag;
                ClearIrqStatus(RadioIrqMasks.IRQ_PREAMBLE_DETECTED);
            }

            if (irqFlags.Has(RadioIrqMasks.IRQ_SYNCWORD_VALID))
            {
                // You usually keep RX alive here — no restart yet
                ClearIrqStatus(RadioIrqMasks.IRQ_SYNCWORD_VALID);
            }

            if (irqFlags.Has(RadioIrqMasks.IRQ_CAD_ACTIVITY_DETECTED))
            {
                //shouldRestartRx = true; // or switch to RX to receive
                LastRxTag = $"RX:ACTIVITY;IRQ=0x{irqFlags:X4};SETUP={LastSetupLoRa}";
                rcvRxTag = LastRxTag;
                ClearIrqStatus(RadioIrqMasks.IRQ_CAD_ACTIVITY_DETECTED);
            }

            // ----------------------------
            // 7️⃣ CAD DONE (only if using CAD mode)
            // ----------------------------
            //if (irqFlags.HasAll(RadioIrqMasks.IRQ_CAD_DONE | RadioIrqMasks.IRQ_CAD_ACTIVITY_DETECTED))
            //{
            //    if (irqFlags.Has(RadioIrqMasks.IRQ_CAD_ACTIVITY_DETECTED))
            //    {
            //        Debug.WriteLine("IRQ: CAD_DETECTED — channel busy");
            //        shouldRestartRx = true; // or switch to RX to receive
            //    }
            //    else
            //    {
            //        Debug.WriteLine("IRQ: CAD_DONE — channel free");
            //        // could trigger TX here if doing LBT
            //    }
            //    ClearIrqStatus(RadioIrqMasks.IRQ_CAD_DONE | RadioIrqMasks.IRQ_CAD_ACTIVITY_DETECTED);
            //}

            // ----------------------------
            // 9️⃣ Restart RX if needed
            // ----------------------------
            if (shouldRestartRx)
            {
                Thread.Sleep(2); // allow radio to settle
                // SET_RX with infinite timeout (0xFFFFFF)
                spiWriteCommand(RadioCommands.SET_RX, 0xFF, 0xFF, 0xFF, prompt: "[setRX] ");
                shouldRestartRx = false;
            }

            // workaround for glitches, etc.
            if(GetIRQLine == PinValue.High)
                ClearIrqStatus(RadioIrqMasks.IRQ_RADIO_ALL);

            if (!string.IsNullOrEmpty(rcvRxTag))
                OnPacketRxTag?.Invoke(this, rcvRxTag);

            if (rcvPacket != null)
                OnPacketReceived?.Invoke(this, rcvPacket);
            
            Debug.WriteLine($"→ LastRxTag: {LastRxTag}");
            Debug.WriteLine($"•[{Timestamp}]EndIrqHandler Done\r\n");
        }
        #endregion

        #region Interface ILoRaDriver
        public void ClearLastRxTag() => LastRxTag = "None";
        public string GetLastRxTag() => LastRxTag;
        public string GetLastSetupLoRa() => LastSetupLoRa;

        #region +++ Init SX1262
        public void SetupLoRa(
                uint frequency = 869_525_000,
                int offset = 0,
                byte modParam1 = 12,    // SF
                byte modParam2 = 0x04,  // BW
                byte modParam3 = 0x01,  // CR
                ushort syncWord = LORA_MAC_PRIVATE_SYNCWORD,
                ushort preamble = 16,   // preamble
                byte irqMode = 0        // 0=default, 1=diagnostic
            )
        {
            int retry = 3;

            // LDRO (Low Data Rate Optimization)
            byte modParam4 = (modParam1 >= 11 && modParam2 <= 0x04) ? (byte)0x01 : (byte)0x00;

        again:
            Debug.WriteLine($"{(retry == 3 ? "🔧" : "🔧🔄")} SetupLoRa...");

            // Hardware reset
            Reset();
            Thread.Sleep(10);

            if (!CheckDevice())
            {
                if (--retry >= 0) goto again;
                throw new Exception("Cannot initialize SX1262 at CheckDevice");
            }

            // Standby (RC oscillator)
            spiWriteCommand(RadioCommands.SET_STANDBY, 0x00, prompt: "[mode] ");

            // Power supply: DCDC
            spiWriteCommand(RadioCommands.SET_REGULATOR_MODE, 0x01, prompt: "[regMode] ");

            // TX power + ramp time (fixed for all modes)
            spiWriteCommand(RadioCommands.SET_TX_PARAMS, 0x16, 0x04, prompt: "[paramsTx] ");

            // High-power PA config for SX1262 (+22 dBm)
            spiWriteCommand(RadioCommands.SET_PA_CONFIG, 0x04, 0x07, 0x00, 0x01, prompt: "[paConfig] ");

            // TCXO control (3.3V, 2 ms startup)
            spiWriteCommand(RadioCommands.SET_DIO3_AS_TCXO_CTRL, new byte[] { 0x07, 0x00, 0x00, 0x02, 0x00 }, prompt: "[ctrlTcx0] ");

            // Full calibration
            spiWriteCommand(RadioCommands.CALIBRATE, 0x7F, prompt: "[calib] ");
            CalibrateImage(frequency);

            // DIO2 free (can be IRQ or left unused)
            spiWriteCommand(RadioCommands.SET_DIO2_AS_RF_SWITCH_CTRL, 0x00, prompt: "[ctrlRF] ");

            // Packet type: LoRa
            spiWriteCommand(RadioCommands.SET_PACKET_TYPE, (byte)RadioPacketTypes.LORA, prompt: "[pktType] ");

            // Base frequency (with optional offset)
            SetRfFrequency(frequency + (uint)offset);

            // Modulation parameters (SF, BW, CR, LDRO)
            spiWriteCommand(RadioCommands.SET_MODULATION_PARAMS, new byte[] {
                modParam1, modParam2, modParam3, modParam4
            }, prompt: "[modulation] ");

            // FIFO base addresses (TX=0, RX=0)
            spiWriteCommand(RadioCommands.SET_BUFFER_BASE_ADDRESS, 0x00, 0x00, prompt: "[baseAddr] ");

            // Packet parameters — max payload, explicit header
            spiWriteCommand(RadioCommands.SET_PACKET_PARAMS, new byte[]
            {
                (byte)(preamble >> 8),      // Preamble MSB
                (byte)(preamble & 0xFF),    // Preamble LSB
                0xFF,         // Max payload (actual len from header)
                0x01,         // CRC ON
                0x00,         // Normal IQ
                0x00
            }, prompt: "[pktParams] ");

            // IRQ config (same for both TX & RX here)

            // IRQ mask
            if (irqMode == 0)
            {
                // default
                spiWriteCommand(RadioCommands.SET_DIO_IRQ_PARAMS, new byte[]
                {
                0x00, 0x72, // RX_DONE | TX_DONE | CRC_ERR | HEADER_ERR | HEADER_VALID  // 0x72
                0x00, 0x62, // Map TX_DONE, RX_DONE, CRC_ERR, HEADER_ERR to DIO1        // 0x62
                0x00, 0x00, // DIO2 unused
                0x00, 0x00  // DIO3 unused
                }, prompt: "[irqParams] ");
            }
            else
            {
                // for diagnostics
                spiWriteCommand(RadioCommands.SET_DIO_IRQ_PARAMS, new byte[]
                {
                0x03, 0xFF,   // IRQ mask: all RX-related IRQs
                0x03, 0xFF,   // DIO1: map all RX IRQs to DIO1
                0x00, 0x00,   // DIO2 unused
                0x00, 0x00    // DIO3 unused
                }, prompt: "[irqParams] ");
            }
           
            // Standby before entering user mode
            spiWriteCommand(RadioCommands.SET_STANDBY, 0x00, prompt: "[mode] ");

            // Clear any IRQs
            ClearIrqStatus(RadioIrqMasks.IRQ_RADIO_ALL);

            // High sensitivity mode (optional, depends on hardware design)
            SetHighSensitivity();

            // Sync word (Meshtastic default or user-defined)
            SetSyncWord(syncWord);
            if (GetSyncWord() != syncWord)
            {
                if (--retry >= 0) goto again;
                throw new Exception($"Cannot initialize SX1262 at SetSyncWord");
            }

            // Register DIO1 IRQ handler 
            if (string.IsNullOrEmpty(LastSetupLoRa) && _ioctrl != null && _irqLine != null)
              _ioctrl.RegisterCallbackForPinValueChangedEvent(_irqLine.PinNumber, PinEventTypes.Rising, OnDio1IrqHandler);
            
            // last succesfull setup
            LastSetupLoRa = $"{frequency},{offset},{modParam1},{modParam2},{modParam3},0x{syncWord:X4},{preamble},{irqMode}";
            LastPreamble = preamble;

            Debug.WriteLine("✅ LoRa setup complete.");
        }

        private void CalibrateImage(uint freqHz)
        {
            byte[] calFreq = new byte[2];

            if (freqHz > 900_000_000)
            {
                calFreq[0] = 0xE1;
                calFreq[1] = 0xE9;
            }
            else if (freqHz > 850_000_000)
            {
                calFreq[0] = 0xD7;
                calFreq[1] = 0xDB;              // updated from 0xD8
            }
            else if (freqHz > 770_000_000)
            {
                calFreq[0] = 0xC1;
                calFreq[1] = 0xC5;
            }
            else if (freqHz > 460_000_000)
            {
                calFreq[0] = 0x75;
                calFreq[1] = 0x81;
            }
            else if (freqHz > 425_000_000)
            {
                calFreq[0] = 0x6B;
                calFreq[1] = 0x6F;
            }
            else
            {
                Debug.WriteLine("⚠ Frequency too low for image calibration.");
                return;
            }

            spiWriteCommand(RadioCommands.CALIBRATE_IMAGE, calFreq);
        }
        private void SetHighSensitivity()
        {
            // Enable RX Boosted gain mode (for ~3dB improved sensitivity)
            const ushort REG_RX_GAIN = 0x08AC;
            const byte BOOSTED_GAIN = 0x96;  // Max LNA gain (per SX126x datasheet)
            Debug.WriteLine("Setting high sensitivity mode (Boosted Gain)...");
            spiWriteRegister(REG_RX_GAIN, BOOSTED_GAIN, prompt: "[gainRX] ");
        }
        
        #endregion
        #endregion

        #region +++ Transmit
        public void TransmitMeshtasticPacket()
        {
            uint fromNodeId = 0x45a350a0; // Set your node ID (unique per device)
            string message = "Hello Meshtastic!";

            using var ms = new MemoryStream();
            ProtobufHelper.WriteVarint(ms, 0x45a350a0);
            var bytes = ms.ToArray();
            Debug.WriteLine(BitConverter.ToString(bytes));

            byte[] packet = MeshtasticPacketBuilder.BuildTextPacket(
                message,
                fromNodeId,
                toNodeId: 0xFFFFFFFF, // broadcast
                wantAck: false,
                hopLimit: 3           // typical mesh hop limit
            );

            // Send via SX1262 driver
            TransmitPacket(packet);
        }
        public void TransmitPacketAsync(string text = null, Encoding encoding = null)
        {
            StartTransmitThread(() => TransmitPacket(text, encoding));
        }
        public void TransmitPacketAsync(params byte[] msg)
        {
            StartTransmitThread(() => TransmitPacket(msg));
        }
        private void StartTransmitThread(ThreadStart action)
        {
            var thread = new Thread(action) { Priority = ThreadPriority.BelowNormal };
            thread.Start();
        }
        public bool TransmitPacket(string text = null, Encoding encoding = null)
        {
            text ??= "Hello";
            encoding ??= Encoding.UTF8;  // default to UTF-8, can be overridden
            return TransmitPacket(encoding.GetBytes(text));
        }
        public bool TransmitPacket(params byte[] msg)
        {
            if (msg == null || msg.Length == 0)
            {
                Debug.WriteLine("⚠️ TransmitPacket: No data to send.");
                return false;
            }
            if (msg.Length > MAXLEN_PACKET - 2) // less two wrapper bytes
            {
                Debug.WriteLine($"⚠️ TransmitPacket: Packet too large ({msg.Length} bytes), max is {MAXLEN_PACKET - 2}.");
                return false;
            }
            //lock (_locker)
            {
                return _TransmitPacket(msg);
            } 
        }

        private bool _TransmitPacket(params byte[] payload)
        {
            Debug.WriteLine($"🚀[{Timestamp}] TransmitPacket: Sending {payload.Length} bytes...");

            //GetIrqStatus(out _, prompt: "🕒 ");

            // Standby with crystal oscillator ON
            spiWriteCommand(RadioCommands.SET_STANDBY, (byte)RadioStandbyModes.STDBY_XOSC, prompt:"[mode] ");

            // ✅ Write payload into FIFO (TX base is already set to 0 in SetupLoRa)
            spiWriteBuffer(0x00, payload, prompt: "[data] ");

            // Update payload length only
            spiWriteCommand(RadioCommands.SET_PACKET_PARAMS, new byte[]
            {
                (byte)(LastPreamble >> 8),      // Preamble MSB
                (byte)(LastPreamble & 0xFF),    // Preamble LSB
                0x00,                     // Explicit header
                (byte)payload.Length,     // Payload length (dynamic)
                0x01,                     // CRC on
                0x00,                     // Normal IQ
                0x00                      // Reserved
            }, prompt: "[pktParams] ");

            // Start TX (continuous timeout = 0xFFFFFF)
            spiWriteCommand(RadioCommands.SET_TX, 0xFF, 0xFF, 0xFF, prompt: "[setTX] ");

            Debug.WriteLine($"🕒 TX started, waiting for TxDone {showIRQLine}...");
            bool txdone = WaitForTxDone(8000, 1);
            //bool txdone = _txDoneEvent.WaitOne(8000, false);
            Debug.WriteLine(
                (txdone ? "✅ TX DONE!" : "⚠ TX TIMEOUT!") +
                $" {showIRQLine}: {GetIrqStatus(out byte _).ToIrqString()}");

            spiWriteCommand(RadioCommands.SET_STANDBY, (byte)RadioStandbyModes.STDBY_RC, prompt: "[mode] ");
            
            //EnableRxContinuous();
            
            Debug.WriteLine($"• [{Timestamp}] TransmitPacket: Done");
            return txdone;
        }

        private bool WaitForTxDone(int timeoutInMs = Timeout.Infinite, int pollingTimeInMs = 10)
        {
#if INTERRUPT
                return _txDoneEvent.WaitOne(timeoutInMs, false);
#else
            while (timeoutInMs > 0)
            {
                timeoutInMs -= pollingTimeInMs;
                Thread.Sleep(pollingTimeInMs);

                byte status = GetStatus(flagDebug: false);
                byte cmdStatus = (byte)((status >> 1) & 0x07);
                if (cmdStatus == 0x06)
                {
                    Debug.WriteLine($"✔️ CMD_TXDONE | {showIRQLine}");
                    return true;
                }
            }
            return false;
#endif
        }
        #endregion

        #region +++ Receiver 
        public void EnableRxContinuous()
        {
            Debug.WriteLine("\r\n📡 Enabling continuous RX...");

            spiWriteCommand(RadioCommands.SET_STANDBY, (byte)RadioStandbyModes.STDBY_XOSC, prompt: "[mode] " );

            // 4️⃣ Clear all IRQ flags
            ClearIrqStatus(RadioIrqMasks.IRQ_RADIO_ALL);

            // 3️⃣ Guard delay: allow RF frontend to settle after TX → RX switch
            Thread.Sleep(2);   // tweak between 2–10 ms depending on board/TCXO

            // 5 Start RX continuous
            spiWriteCommand(RadioCommands.SET_RX, 0xFF, 0xFF, 0xFF, prompt: "[setRX] ");

            // 4️⃣ Clear all IRQ flags
            ClearIrqStatus(RadioIrqMasks.IRQ_RADIO_ALL);

            // Radio status
            GetIrqStatus(out _, "• ");

            Debug.WriteLine($"✅ Continuous RX enabled\r\n");
        }

        public void DisableRadioReception()
        {
            // Force standby mode (RC oscillator)
            spiWriteCommand(RadioCommands.SET_STANDBY, 0x00);

            // Disable all IRQ sources
            spiWriteCommand(RadioCommands.SET_DIO_IRQ_PARAMS, new byte[]
            {
                0x00, 0x00, // IRQ mask: disable all events
                0x00, 0x00, // DIO1: mapped to all IRQ sources
                0x00, 0x00, // DIO2 unused
                0x00, 0x00  // DIO3 unused
            });

            // Clear all IRQs
            ClearIrqStatus(RadioIrqMasks.IRQ_RADIO_ALL);

            Debug.WriteLine("📴 Radio fully disabled: standby + no IRQ mask.");
        }
       
        private byte GetRxBufferStatus(out byte payloadLength, out byte fifoStartPointer, out byte endOfData)
        {
            payloadLength = 0;
            fifoStartPointer = 0;
            endOfData = 0;

            byte[] resp = spiReadCommand(RadioCommands.GET_RX_BUFFER_STATUS, 3, prompt: "[bufStat] ");
            byte chipStatus = resp[1];
            fifoStartPointer = resp[3];
            if (RadioCmdStatus.SUCCESS.Eq(chipStatus))
            {
                // get the payloadLength from the payload frame and based on this read the EOD byte
                byte frontByte = spiReadBuffer(fifoStartPointer, prompt: "[len] ");
                endOfData = spiReadBuffer((byte)(fifoStartPointer + frontByte + 1), prompt: "[eod] ");
                payloadLength = frontByte;
                //payloadLength = (byte)(endOfData == 0x00 ? frontByte : 0x00);
            }
            return chipStatus;
        }

        private PacketStatus GetPacketStatus()
        {
            // SX1262: 0x14 + 4×NOP → returns RFU + Status + 3 measurements 
            byte[] resp = spiReadCommand(RadioCommands.GET_PACKET_STATUS, 4, prompt: "[pktStat] ");

            return new PacketStatus
            {
                ChipStatus = resp[1],
                PacketRssi = (sbyte)(-resp[2] / 2),  // 0.5 dB steps
                Snr = ((sbyte)resp[3]) / 4.0,        // 0.25 dB steps
                SignalRssi = (sbyte)(-resp[4] / 2)   // 0.5 dB steps
            };
        }

        public byte GetStatus(bool flagDebug = true)
        {
            byte status = spiReadCommand(RadioCommands.GET_STATUS, flagDebug: flagDebug, prompt: "[stat] ");
            return (byte)(status & 0x7E);
        }

        public bool IsReadyForTX()
        {
            byte stat = GetStatus(false);
            bool retVal = stat == RX_RFU || stat == STBY_RC_RFU;
            Debug.WriteLine($"Status[0x{stat:X2}]({stat.ToText()}), ReadyForTX={retVal}");
            return retVal;
        }
        #endregion

        #region +++ SPI Level 

        #region SPI R/W Command
        #region SPI_Helpers
        private void WaitOnBusy()
        {
            if (_busyLine != null)
                while (_busyLine.Read() == PinValue.High)
                    Thread.Sleep(1);
        }
        #endregion

        public byte spiReadCommand(RadioCommands cmd, bool flagDebug = true, string prompt = "")
        {
            return spiReadCommand(cmd, 1, flagDebug, prompt)[0];
        }

        public byte[] spiReadCommand(RadioCommands cmd, int numberOfBytes, bool flagDebug = true, string prompt = "")
        {
            // ensure BUSY is low (state meachine ready)
            WaitOnBusy();

            byte[] wrbuffer = new byte[1 + numberOfBytes];
            byte[] rdbuffer = new byte[1 + numberOfBytes];
            wrbuffer[0] = (byte)cmd;
            base.TransferFullDuplex(new SpanByte(wrbuffer), new SpanByte(rdbuffer));

#if DEBUG
            if (flagDebug)
            {
                Debug.WriteLine($"{prompt}SPI Read Command: {cmd.ToText()} | Result[..{numberOfBytes}]: {BitConverter.ToString(rdbuffer, 1)}");
            }
#endif
            return rdbuffer;
        }

        // generic purpose
        public void spiWriteCommand(RadioCommands cmd, byte[] data = null, bool flagDebug = true, string prompt = "")
        {
            // ensure BUSY is low (state meachine ready)
            WaitOnBusy();

            var length = data == null ? 1 : data.Length + 1;
            byte[] outbuffer = new byte[length];
            outbuffer[0] = (byte)cmd;
            data?.CopyTo(outbuffer, 1);
#if DEBUG
            if (flagDebug)
            {
                string payloadHex = data != null ? BitConverter.ToString(data) : "(no data)";
                //Debug.WriteLine($"SPI Write Command: 0x{(byte)cmd:X2} | Data: {payloadHex}");
                Debug.WriteLine($"{prompt}SPI Write Command: {cmd.ToText()} | Data: {payloadHex}");
            }
#endif
            base.Write(new SpanByte(outbuffer));
        }

        public void spiWriteCommand(RadioCommands cmd, byte data0, byte data1, byte data2, byte data3, bool flagDebug = true, string prompt = "")
        {
            // ensure BUSY is low (state meachine ready)
            WaitOnBusy();
            spiWrite(new byte[] { (byte)cmd, data0, data1, data2, data3 }, flagDebug, prompt);
        }

        public void spiWriteCommand(RadioCommands cmd, byte data0, byte data1, byte data2, bool flagDebug = true, string prompt = "")
        {
            // ensure BUSY is low (state meachine ready)
            WaitOnBusy();
            spiWrite(new byte[] { (byte)cmd, data0, data1, data2 }, flagDebug, prompt);
        }

        public void spiWriteCommand(RadioCommands cmd, byte data0, byte data1, bool flagDebug = true, string prompt = "")
        {
            // ensure BUSY is low (state meachine ready)
            WaitOnBusy();
            spiWrite(new byte[] { (byte)cmd, data0, data1 }, flagDebug, prompt);
        }

        public void spiWriteCommand(RadioCommands cmd, byte data, bool flagDebug = true, string prompt = "")
        {
            // ensure BUSY is low (state meachine ready)
            WaitOnBusy();
            spiWrite(new byte[] { (byte)cmd, data }, flagDebug, prompt);
        }

        public void spiWrite(byte[] data, bool flagDebug = true, string prompt = "")
        {
            WaitOnBusy();
#if DEBUG
            if (flagDebug)
            {
                RadioCommands cmd = (RadioCommands)data[0];
                string payloadHex = data != null && data.Length > 1 ? BitConverter.ToString(data, 1) : "(no data)";
                //Debug.WriteLine($"SPI Write Command: 0x{data[0]:X2} | Data: {payloadHex}");
                Debug.WriteLine($"{prompt}SPI Write Command: {cmd.ToText()} | Data: {payloadHex}");
            }
#endif
            base.Write(new SpanByte(data));
        }
        #endregion

        #region SPI R/W Register
        // sx1262 doc:  Table 13-25: ReadRegister SPI Transaction
        public byte[] spiReadRegister(ushort address, ushort len = 1, bool flagDebug = true, string prompt = "")
        {
            WaitOnBusy();

            byte[] buffer = new byte[len + 4];
            buffer[0] = (byte)RadioCommands.READ_REGISTER; // 0x1D; 
            buffer[1] = (byte)((address & 0xFF00) >> 8);
            buffer[2] = (byte)(address & 0x00FF);
            var output = new SpanByte(buffer);
            base.TransferFullDuplex(new SpanByte(buffer), output);
            byte[] result = output.Slice(4).ToArray();
#if DEBUG
            if (flagDebug)
            {
                Debug.WriteLine($"{prompt}SPI Read Register: Addr=0x{address:X4} | Len={len} | Data: {BitConverter.ToString(result)}");
            }
#endif
            return result;
        }
        public void spiWriteRegister(ushort address, byte data, bool flagDebug = true, string prompt = "")
        {
            spiWriteRegister(address, new byte[] { data }, flagDebug, prompt);
        }
        public void spiWriteRegister(ushort address, byte[] data = null, bool flagDebug = true, string prompt = "")
        {
            //std::lock_guard<std::mutex> lg(IOLock);
            WaitOnBusy();

            var length = data == null ? 3 : data.Length + 3;
            byte[] outbuffer = new byte[length];
            outbuffer[0] = (byte)RadioCommands.WRITE_REGISTER;   // 0x0D; // CMD_WRITE_REGISTER;
            outbuffer[1] = (byte)((address & 0xFF00) >> 8);
            outbuffer[2] = (byte)(address & 0x00FF);
            data?.CopyTo(outbuffer, 3);
#if DEBUG
            if (flagDebug)
            {
                string dataHex = data != null ? BitConverter.ToString(data) : "(no data)";
                Debug.WriteLine($"{prompt}SPI Write Register: Addr=0x{address:X4} | Data: {dataHex}");
            }
#endif
            base.Write(new SpanByte(outbuffer));
            //base.TransferFullDuplex(new SpanByte(outbuffer), new SpanByte(outbuffer));
        }
        #endregion

        #region SPI R/W Buffer
        private void spiWriteBuffer(byte offset, byte[] data, bool flagDebug = true, string prompt = "")
        {
            WaitOnBusy();

            byte[] buffer = new byte[2 + data.Length];
            buffer[0] = (byte)RadioCommands.WRITE_BUFFER;   // 0x80 FIFO write opcode
            buffer[1] = offset;                             // FIFO offset
            Array.Copy(data, 0, buffer, 2, data.Length);
            base.Write(buffer);
#if DEBUG
            if (flagDebug)
            {
                Debug.WriteLine($"{prompt}SPI Write Buffer: Offset=0x{offset:X2} | Data: {BitConverter.ToString(data)}");
            }
#endif
        }

        private byte spiReadBuffer(byte offset, bool flagDebug = true, string prompt = "")
        {
            return spiReadBuffer(offset, 1, flagDebug, prompt)[0];
        }

        private byte[] spiReadBuffer(byte offset, int length, bool flagDebug = true, string prompt = "")
        {
            WaitOnBusy();

            // SX1262 "ReadBuffer" command: 0x1E, followed by offset, dummy byte, then read data
            byte[] tx = new byte[3 + length];
            byte[] rx = new byte[3 + length];

            tx[0] = (byte)RadioCommands.READ_BUFFER; //  0x1E  ReadBuffer command
            tx[1] = offset;     // FIFO offset
            tx[2] = 0x00;       // Dummy byte (required per datasheet)

            base.TransferFullDuplex(new SpanByte(tx), new SpanByte(rx));

            // Extract received payload (skip 3 command bytes)
            byte[] payload = new byte[length];
            Array.Copy(rx, 3, payload, 0, length);
#if DEBUG
            if (flagDebug)
            {
                Debug.WriteLine($"{prompt}SPI Read Buffer: Offset=0x{offset:X2} | Length={length} | Data: {BitConverter.ToString(payload)}");
            }
#endif
            return payload;
        }

        #endregion

        #region Basic methods
        public virtual void Reset()
        {
            Debug.WriteLine("[RESET] Resetting SX126x...");

            // clean-up state
            if (!string.IsNullOrEmpty(LastSetupLoRa) && _ioctrl != null && _irqLine != null)
            {
                LastSetupLoRa = string.Empty;
                _ioctrl.UnregisterCallbackForPinValueChangedEvent(_irqLine.PinNumber, OnDio1IrqHandler);
            }

            if (_rstLine != null)
            {
                _rstLine.Write(PinValue.Low);
                Thread.Sleep(20);
                _rstLine.Write(PinValue.High);
                Thread.Sleep(20);
            }
            else
            {
                Debug.WriteLine("[RESET] No hardware reset pin, using soft reset...");

                spiWriteCommand(RadioCommands.SET_STANDBY, new byte[] { 0x00 }, prompt: "[mode] ");
                spiWriteCommand(RadioCommands.CLEAR_IRQ_STATUS, new byte[] { 0xFF, 0xFF }, prompt: "[clrIrq] ");
                spiWriteCommand(RadioCommands.RESET_STATS, new byte[] { 0x00, 0x00, 0x00 }, prompt: "[rstStat] ");
            }

            WaitOnBusy();
            spiWriteCommand(RadioCommands.CLEAR_IRQ_STATUS, new byte[] { 0xFF, 0xFF }, prompt: "[clrIrq] ");

            // Reset buffer base addresses (critical!)
            spiWriteCommand(RadioCommands.SET_BUFFER_BASE_ADDRESS, new byte[] { 0x00, 0x00 }, prompt: "[baseAddr] ");

            Debug.WriteLine("[RESET] Done.");
        }

        public virtual bool CheckDevice()
        {
            //check there is a device out there, writes a register and reads back
            Debug.WriteLine("[CHECK] Check device writes a register and reads back ...");
            byte Regdata1, Regdata2;
            Regdata1 = spiReadRegister(0x88e)[0];           //low byte of frequency setting
            spiWriteRegister(0x88e, new byte[] { (byte)(Regdata1 + 1) });
            Regdata2 = spiReadRegister(0x88e)[0];                 //read changed value back
            spiWriteRegister(0x88e, Regdata1);              //restore register to original value
            bool result = Regdata2 == (Regdata1 + 1);
            Debug.WriteLine($"[CHECK] Done - {result}");
            return result;
        }
        public ushort GetIrqStatus(out byte chipStatus, string prompt = "", int waitForMs = 0 )
        {
            if(waitForMs > 0)
                Thread.Sleep(waitForMs);
            byte[] rx = spiReadCommand(RadioCommands.GET_IRQ_STATUS, 3, false);
            chipStatus = (byte)(rx[1] & 0x7E); // Byte 1 is the "status" byte from Table 13-30
            ushort irq = (ushort)(((rx[2] << 8) | rx[3]) & 0x03FF); // Table 13-29: IRQ Registers
            if (string.IsNullOrEmpty(prompt) && irq == (ushort)RadioIrqMasks.IRQ_RADIO_NONE) return irq;     // to avoid lot of logs
            Debug.WriteLine($"{prompt}GetIrqStatus: Status[0x{chipStatus:X2}]({chipStatus.ToText()}) | {irq.ToIrqString()} | {showIRQLine}");
            return irq;
        }
        public void ClearIrqStatus(RadioIrqMasks irqmask = RadioIrqMasks.IRQ_RADIO_ALL)
        {
            byte[] buf = new byte[] { (byte)(((ushort)irqmask >> 8) & 0x00FF), (byte)((ushort)irqmask & 0x00FF) };
            spiWriteCommand(RadioCommands.CLEAR_IRQ_STATUS, buf, prompt:"[clrIrq] " );
            Debug.WriteLine($"ClearIrqStatus: {irqmask.ToIrqString()} | {showIRQLine}");
        }

        public void SetRfFrequency(uint freqHz)
        {
            uint freqReg = (uint)((double)freqHz / (32_000_000.0 / (1 << 25)));

            byte[] buf = new byte[4] {
                (byte)((freqReg >> 24) & 0xFF),
                (byte)((freqReg >> 16) & 0xFF),
                (byte)((freqReg >> 8) & 0xFF),
                (byte)(freqReg & 0xFF)
            };
            spiWriteCommand(RadioCommands.SET_RF_FREQUENCY, buf, prompt:"[setHz] ");
        }
        public void SetSyncWord(ushort syncWord)
        {
            spiWriteRegister(REG_LR_SYNCWORD, (byte)((syncWord >> 8) & 0xFF));
            spiWriteRegister(REG_LR_SYNCWORD + 1, (byte)(syncWord & 0xFF));
        }
        public ushort GetSyncWord()
        {
            byte[] data = spiReadRegister(REG_LR_SYNCWORD, 2, prompt: "[synchWord] ");
            return (ushort)((data[0] << 8) | data[1]);
        }
        #endregion

        #endregion
       
    }

    #region Meshtastic Classes
    public static class MeshtasticPacketBuilder
    {
        private static uint packetCounter = 0;

        public static byte[] BuildTextPacket(string text, uint fromNodeId, uint toNodeId = 0xFFFFFFFF, bool wantAck = false, byte hopLimit = 0)
        {
            using var ms = new MemoryStream();

            // 1: packet_type = TEXT
            ProtobufHelper.WriteKey(ms, 1, 0);
            ProtobufHelper.WriteVarint(ms, (uint)PacketType.TEXT);

            // 2: from node ID
            ProtobufHelper.WriteKey(ms, 2, 0);
            ProtobufHelper.WriteVarint(ms, fromNodeId);

            // 3: to node ID
            ProtobufHelper.WriteKey(ms, 3, 0);
            ProtobufHelper.WriteVarint(ms, toNodeId);

            // 4: id (incrementing counter)
            ProtobufHelper.WriteKey(ms, 4, 0);
            ProtobufHelper.WriteVarint(ms, ++packetCounter);

            // 5: flags (WantAck, hopLimit)
            uint flags = 0;
            if (wantAck) flags |= 0x1;
            flags |= ((uint)hopLimit & 0xFF) << 1;
            ProtobufHelper.WriteKey(ms, 5, 0);
            ProtobufHelper.WriteVarint(ms, flags);

            // 6: payload (the text message)
            ProtobufHelper.WriteKey(ms, 6, 2);
            var payloadBytes = Encoding.UTF8.GetBytes(text);
            ProtobufHelper.WriteBytes(ms, payloadBytes);

            return ms.ToArray();
        }
    }

    public static class ProtobufHelper
    {
        public static void WriteKey(Stream stream, uint fieldNumber, int wireType)
        {
            uint key = (fieldNumber << 3) | (uint)wireType;
            WriteVarint(stream, key);
        }

        public static void WriteVarint(Stream stream, uint value)
        {
            while (value > 127)
            {
                stream.WriteByte((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }
            stream.WriteByte((byte)value);
        }

        public static void WriteBytes(Stream stream, byte[] bytes)
        {
            WriteVarint(stream, (uint)bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    public enum PacketType : uint
    {
        TEXT = 0,
        POSITION = 1,
        DATA = 2,
        CONTROL = 3,
        ROUTER = 4,
        CONFIG = 5,
        ACK = 6,
        // Add more if needed according to your firmware version
    }
    #endregion

    #region Extensions
    public static class RadioCmdStatusExtension
    {
        public const byte DefaultStatusMask = 0x07;
        public const byte DefaultStatusShift = 1;

        // Fastest path - avoids int math, easy for inlining
        public static bool Eq(this RadioCmdStatus expected, byte rawStatus)
        {
            byte shifted = (byte)(rawStatus >> DefaultStatusShift);
            byte extracted = (byte)(shifted & DefaultStatusMask);
            return extracted == (byte)expected;
        }

        // Flexible path for custom masks/shifts
        public static bool EqCustom(this RadioCmdStatus expected, byte rawStatus, byte statusMask, byte statusShift)
        {
            byte shifted = (byte)(rawStatus >> statusShift);
            byte extracted = (byte)(shifted & statusMask);
            return extracted == (byte)expected;
        }
    }

    public static class RadioIrqMasksExtensions
    {
        public static bool Has(this ushort rawValue, RadioIrqMasks mask)
            => (rawValue & (ushort)mask) != 0;

        public static bool HasAll(this ushort rawValue, RadioIrqMasks mask)
            => (rawValue & (ushort)mask) == (ushort)mask;

        public static bool IsExact(this ushort rawValue, RadioIrqMasks mask)
            => rawValue == (ushort)mask;

        public static string ToIrqString(this RadioIrqMasks mask)
        {
            return ((ushort)mask).ToIrqString();
        }
        public static string ToIrqString(this ushort irqStatus)
        {
            if (irqStatus == (ushort)RadioIrqMasks.IRQ_RADIO_NONE)
                return "IRQ_NONE";
            if (irqStatus == (ushort)RadioIrqMasks.IRQ_RADIO_ALL)
                return "IRQ_ALL";

            var sb = new StringBuilder($"IRQ[0x{irqStatus:X4}](");

            void AppendFlag(RadioIrqMasks mask, string name)
            {
                if ((irqStatus & (ushort)mask) != 0)
                    sb.Append(name).Append(" | ");
            }

            AppendFlag(RadioIrqMasks.IRQ_TX_DONE, "IRQ_TX_DONE");
            AppendFlag(RadioIrqMasks.IRQ_RX_DONE, "IRQ_RX_DONE");
            AppendFlag(RadioIrqMasks.IRQ_PREAMBLE_DETECTED, "IRQ_PREAMBLE_DETECTED");
            AppendFlag(RadioIrqMasks.IRQ_SYNCWORD_VALID, "IRQ_SYNCWORD_VALID");
            AppendFlag(RadioIrqMasks.IRQ_HEADER_VALID, "IRQ_HEADER_VALID");
            AppendFlag(RadioIrqMasks.IRQ_HEADER_ERROR, "IRQ_HEADER_ERROR");
            AppendFlag(RadioIrqMasks.IRQ_CRC_ERROR, "IRQ_CRC_ERROR");
            AppendFlag(RadioIrqMasks.IRQ_CAD_DONE, "IRQ_CAD_DONE");
            AppendFlag(RadioIrqMasks.IRQ_CAD_ACTIVITY_DETECTED, "IRQ_CAD_ACTIVITY_DETECTED");
            AppendFlag(RadioIrqMasks.IRQ_RX_TX_TIMEOUT, "IRQ_RX_TX_TIMEOUT");

            return sb.ToString().TrimEnd(new char[] { '|', ' ' }) + ")";
        }
    }

    public static class RadioStatusExtensions
    {
        private static readonly string[] ChipModes = {
        "Unused", "RFU", "STBY_RC", "STBY_XOSC",
        "FS", "RX", "TX", "RESERVED"
        };

        private static readonly string[] CmdStatuses = {
        "Reserved", "RFU", "CMD_SUCCESS", "CMD_TIMEOUT",
        "CMD_ERROR", "CMD_FAILURE", "CMD_TXDONE", "Undefined"
        };

        public static string ToText(this byte status, string prompt = null)
        {
            // Extract fields using bit masks and shifts
            int chipMode = (status >> 4) & 0x07;
            int cmdStatus = (status >> 1) & 0x07;

            string modeStr = chipMode < ChipModes.Length ? ChipModes[chipMode] : "UNDEFINED";
            string cmdStr = cmdStatus < CmdStatuses.Length ? CmdStatuses[cmdStatus] : "CMD_UNDEFINED";

            return string.IsNullOrEmpty(prompt)
                ? $"{modeStr}/{cmdStr}"
                : $"{prompt}[0x{status:X2}]({modeStr}/{cmdStr})";
        }
    }

    public static class RadioCommandsExtensions
    {
        //public static string ToText(this RadioCommands cmd)
        //{
        //    switch (cmd)
        //    {
        //        case RadioCommands.CLR_IRQSTATUS: return "CLR_IRQSTATUS";
        //        case RadioCommands.GET_IRQSTATUS: return "GET_IRQSTATUS";
        //        case RadioCommands.GET_PACKETSTATUS: return "GET_PACKETSTATUS";
        //        case RadioCommands.GET_RXBUFFERSTATUS: return "GET_RXBUFFERSTATUS";
        //        case RadioCommands.READ_BUFFER: return "READ_BUFFER";
        //        case RadioCommands.SET_RX: return "SET_RX";
        //        case RadioCommands.SET_DIOIRQPARAMS: return "SET_DIOIRQPARAMS";
        //        case RadioCommands.SET_PACKETPARAMS: return "SET_PACKETPARAMS";
        //        case RadioCommands.SET_BUFFERBASEADDRESS: return "SET_BUFFERBASEADDRESS";
        //        case RadioCommands.SET_STANDBY: return "SET_STANDBY";
        //        default: return $"0x{(byte)cmd:X2}";
        //    }
        //}
        public static string ToText(this RadioCommands cmd)
        {
            return $"0x{(byte)cmd:X2}";
        }
    }

   
    #endregion
}
