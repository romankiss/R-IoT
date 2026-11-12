// Trial replacement: atoms3r_cam_hal_v1 - UNVERIFIED
// CodeCraft generated artifact
// Source: CodeCraft_package1_sys_empty_lora@3.1.5.14
// Contract: CodeCraft_package1_sys_empty_lora@3.1.5.14:device/modules/lora/CodeCraft_lora_module.json#
// Target: nanoframework / C#
// Copyright © 2026 Roman Kiss, rkiss@pathcom.com
// Generated with assistance from ChatGPT and CodeCraft.
// Do not edit manually; update Model-First metadata and regenerate.
using CodeCraft.Common;
using NFAppLib;
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Net;
namespace CodeCraft.Modules.Lora
{
    public partial class LoraModule : VirtualModule
    {
        public readonly struct Topics
        {
            public const string Create = "/ATTACH";
            public const string Open = "/OPEN/DONE";
            public const string Close = "/CLOSE/DONE";
            public const string Setup = "/SETUP/DONE";
            public const string Reset = "/RESET/DONE";
            public const string RX = "/QRX/PACK";
        }
        private LoraModule(string suffix) : base("Lora", "LORA" + suffix) { }
        public static LoraModule Create(string suffix, int msgId = 0)
        {
            LoraModule module = new LoraModule(suffix);
            module.Initialize(); module.Start();
            module.EmitEvent(Topics.Create, msgId, module.Name, module.AtGroup, "PUSH");
            return module;
        }
        protected override void RegisterHandlers()
        {
            RegisterHandler("ECHO", EchoHandler);
            RegisterHandler("INFO", InfoHandler);
            RegisterHandler("LOOPBACK", LoopbackHandler);
            RegisterHandler("OPEN", OpenHandler);
            RegisterHandler("CLOSE", CloseHandler);
            RegisterHandler("SETUP", SetupHandler);
            RegisterHandler("RESET", ResetHandler);
            RegisterHandler("SEND", SendHandler);
            RegisterHandler("SEND_ACK", SendAckHandler);
        }
        protected override void DefaultEventHandler(string topic, int msgId, string filter, string[] args)
        {
            TraceLog(AtGroup, "handler_event", msgId, "topic=" + topic + " filter=" + FilterForLog(filter) + " args=" + args.Join());
        }
        private string EchoHandler(string cmd, int msgId, string[] args)
        {
            string text = args.Join(); EmitEvent("/ECHO/DONE", msgId, text);
            return MessageFormat.BuildData(AtGroup, msgId, text);
        }
        private string LoopbackHandler(string cmd, int msgId, string[] args)
        {
            string[] results = Publish(AtGroup, "ECHO", msgId, "LOOPBACK_TEST"); bool passed = false;
            if (results != null)
                for (int i = 0; i < results.Length; i++)
                    if (results[i] != null && results[i].IndexOf("LOOPBACK_TEST") >= 0) passed = true;
            return MessageFormat.BuildData(AtGroup, msgId, passed ? "OK" : "FAIL");
        }
        private string BaseInfo()
        {
            return "name=" + Name + ",atGroup=" + AtGroup + ",enabled=" + IsEnabled + ",rxCount=" + RxCount;
        }
        private string Ok(int msgId, string text) { return MessageFormat.BuildOk(AtGroup, msgId, text); }
        private string Error(int msgId, string code) { return MessageFormat.BuildError(AtGroup, msgId, code); }
        private SX1262 lora = null;
        private GpioController ioctrl = new GpioController();
        private EventDrivenQueue rxQueue = null;
        private EventDrivenQueue txQueue = null;
        const ushort loraBroadcastAddress = 0xFFFF;
        ushort loraAddress = 0;
        private string InfoHandler(string cmd, int msgId, string[] args)
        {
            string status = BaseInfo() + ",address=0x" + loraAddress.ToString("X4") + ",radio=" + (lora == null ? "closed" : "open") +
                ",config=[" + (lora == null ? "not-opened-radio" : lora.LastSetupLoRa) + "]";
            EmitEvent("/INFO", msgId, status); return MessageFormat.BuildData(AtGroup, msgId, status);
        }
        private static bool Integer(string text, out int value)
        {
            value = 0; if (string.IsNullOrEmpty(text)) return false;
            bool negative = text[0] == '-'; int i = negative ? 1 : 0;
            if (i == text.Length) return false;
            for (; i < text.Length; i++)
            {
                int digit = text[i] - '0';
                if (digit < 0 || digit > 9 || value > (int.MaxValue - digit) / 10) return false;
                value = value * 10 + digit;
            }
            if (negative) value = -value; return true;
        }
        private static bool Unsigned(string text, out uint value)
        {
            value = 0; if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < text.Length; i++)
            {
                int digit = text[i] - '0';
                if (digit < 0 || digit > 9 || value > (uint.MaxValue - (uint)digit) / 10) return false;
                value = value * 10 + (uint)digit;
            }
            return true;
        }
        private void CleanupRadio()
        {
            try
            {
                if (lora != null) lora.OnPacketReceived -= Lora_OnPacketReceived;
                if (rxQueue != null) rxQueue.OnGetItem -= RxQueue_OnGetItem;
                if (txQueue != null) txQueue.OnGetItem -= TxQueue_OnGetItem;
                if (lora != null) lora.Reset();
            }
            finally
            {
                try { if (txQueue != null) txQueue.Dispose(); }
                finally
                {
                    txQueue = null;
                    try { if (rxQueue != null) rxQueue.Dispose(); }
                    finally
                    {
                        rxQueue = null;
                        try { if (lora != null) lora.Dispose(); }
                        finally { lora = null; loraAddress = 0; }
                    }
                }
            }
        }
        private string OpenHandler(string cmd, int msgId, string[] args)
        {
            if (lora != null) return Error(msgId, "EOPENED");
            string arity = "EARGS:OPEN,busId,cs,busy,rst,irq,address,clockFrequency,mode,dataFlow,chipSelectActiveState,configuration";
            if (args == null || args.Length != 11) return Error(msgId, arity);
            try
            {
                int bus, cs, busy, reset, irq, clock; ushort address;
                if (!Integer(args[0], out bus) || !Integer(args[1], out cs) || !Integer(args[2], out busy) ||
                    !Integer(args[3], out reset) || !Integer(args[4], out irq) || !args[5].TryParseLoraAddress(out address) ||
                    !Integer(args[6], out clock) || bus < 0 || cs < 0 || busy < 0 || reset < -1 || irq < -1 || clock <= 0) return Error(msgId, arity);
                SpiMode mode;
                switch (args[7])
                {
                    case "Mode0": mode = SpiMode.Mode0; break;
                    case "Mode1": mode = SpiMode.Mode1; break;
                    case "Mode2": mode = SpiMode.Mode2; break;
                    case "Mode3": mode = SpiMode.Mode3; break;
                    default: return Error(msgId, arity);
                }
                DataFlow flow;
                if (args[8] == "MsbFirst") flow = DataFlow.MsbFirst;
                else if (args[8] == "LsbFirst") flow = DataFlow.LsbFirst;
                else return Error(msgId, arity);
                string active = args[9].ToLower(); if (active != "true" && active != "false") return Error(msgId, arity);
                SpiBusConfiguration configuration;
                switch (args[10])
                {
                    case "FullDuplex": configuration = SpiBusConfiguration.FullDuplex; break;
                    case "HalfDuplex": configuration = SpiBusConfiguration.HalfDuplex; break;
                    case "Simplex": configuration = SpiBusConfiguration.Simplex; break;
                    default: return Error(msgId, arity);
                }
                SpiConnectionSettings settings = new SpiConnectionSettings(bus, cs)
                { ClockFrequency = clock, Mode = mode, DataFlow = flow, ChipSelectLineActiveState = active == "true", Configuration = configuration };
                lora = new SX1262(settings, ioctrl, busy, reset, irq, address);
                if (lora == null) return Error(msgId, "EOPEN:Can't open driver");
                loraAddress = address;
                rxQueue = new EventDrivenQueue("LoRa-QRX", EventDrivenQueue.QueueMode.AutoDrain);
                txQueue = new EventDrivenQueue("LoRa-QTX", EventDrivenQueue.QueueMode.Manual);
                lora.OnPacketReceived += Lora_OnPacketReceived;
                rxQueue.OnGetItem += RxQueue_OnGetItem;
                txQueue.OnGetItem += TxQueue_OnGetItem;
                EmitEvent(Topics.Open, msgId); return Ok(msgId, "DriverOpened");
            }
            catch (Exception ex)
            {
                TraceLog(AtGroup, "open", msgId, ex.Message);
                try { CleanupRadio(); } catch (Exception cleanup) { TraceLog(AtGroup, "open", msgId, cleanup.Message); }
                return Error(msgId, "EOPEN:" + ex.Message);
            }
        }
        private string CloseHandler(string cmd, int msgId, string[] args)
        {
            if (lora == null) return Error(msgId, "ENOTOPEN");
            try { CleanupRadio(); EmitEvent(Topics.Close, msgId); return Ok(msgId, "DriverClosed"); }
            catch (Exception ex) { TraceLog(AtGroup, "close", msgId, ex.Message); return Error(msgId, "ECLOSE:" + ex.Message); }
        }
        private string SetupHandler(string cmd, int msgId, string[] args)
        {
            if (lora == null) return Error(msgId, "ENOTOPEN");
            string arity = "EARGS:SETUP,frequency,offset,sf,bw,cr,syncWord(0x..),preamble,irqMode";
            if (args == null || args.Length != 8) return Error(msgId, arity);
            try
            {
                uint frequency; int offset, sf, bw, cr, preamble, irqMode; ushort syncWord;
                if (!Unsigned(args[0], out frequency) || !Integer(args[1], out offset) || !Integer(args[2], out sf) ||
                    !Integer(args[3], out bw) || !Integer(args[4], out cr) || !args[5].TryParseLoraAddress(out syncWord) ||
                    !Integer(args[6], out preamble) || !Integer(args[7], out irqMode) || sf < 0 || sf > 255 || bw < 0 || bw > 255 ||
                    cr < 0 || cr > 255 || preamble < 0 || preamble > 65535 || irqMode < 0 || irqMode > 255) return Error(msgId, arity);
                lora.SetupLoRa(frequency, offset, (byte)sf, (byte)bw, (byte)cr, syncWord, (ushort)preamble, (byte)irqMode);
                EmitEvent(Topics.Setup, msgId, lora.LastSetupLoRa); return Ok(msgId, "DriverSetup");
            }
            catch (Exception ex) { TraceLog(AtGroup, "setup", msgId, ex.Message); return Error(msgId, "ESETUP:" + ex.Message); }
        }
        private string ResetHandler(string cmd, int msgId, string[] args)
        {
            if (lora == null) return Error(msgId, "ENOTOPEN");
            try { lora.Reset(); EmitEvent(Topics.Reset, msgId); return Ok(msgId, "DriverReset"); }
            catch (Exception ex) { TraceLog(AtGroup, "reset", msgId, ex.Message); return Error(msgId, "ERESET:" + ex.Message); }
        }
        private readonly object ackCommandSync = new object();
        protected override string DefaultAtCommandHandler(string command, int msgId, string[] args)
        {
            // All built-in subscription command mutations share SEND_ACK's ensure lock.
            lock (ackCommandSync) return base.DefaultAtCommandHandler(command, msgId, args);
        }
        private string SendAckHandler(string cmd, int msgId, string[] args)
        {
            lock (ackCommandSync)
            {
                if (lora == null) return Error(msgId, "ENOTOPEN");
                if (string.IsNullOrEmpty(lora.LastSetupLoRa)) return Error(msgId, "ENOTSETUP");
                const string arity = "EARGS:SEND_ACK,destination,payload,consumer,ackFilter";
                if (args == null || args.Length != 4) return Error(msgId, arity);
                ushort to;
                if (string.IsNullOrEmpty(args[0]) || !args[0].TryParseLoraAddress(out to) ||
                    args[1] == null || string.IsNullOrEmpty(args[2]) || string.IsNullOrEmpty(args[3]))
                    return Error(msgId, arity);
                for (int i = 0; i < args[1].Length; i++)
                    if (args[1][i] == ',' || args[1][i] < ' ' || args[1][i] == (char)127) return Error(msgId, arity);
                for (int i = 0; i < args[2].Length; i++)
                    if (args[2][i] <= ' ' || args[2][i] == (char)127 || args[2][i] == '*' ||
                        args[2][i] == '#' || args[2][i] == '|' || args[2][i] == ',') return Error(msgId, arity);
                try
                {
                    string listed = base.DefaultAtCommandHandler("SUBLIST", msgId,
                        new string[] { args[2], Topics.RX, "*" });
                    string prefix = MessageFormat.BuildOk(AtGroup, msgId,
                        "sub_list," + args[2] + "," + Topics.RX + ",*,count=");
                    if (listed == null || listed.IndexOf(prefix) != 0) return Error(msgId, "ESUB:" + (listed ?? "No response"));
                    int end = listed.IndexOf(" [", prefix.Length);
                    int count = 0;
                    if (end <= prefix.Length) return Error(msgId, "ESUB:Invalid SUBLIST response");
                    for (int i = prefix.Length; i < end; i++)
                    {
                        char c = listed[i];
                        if (c < '0' || c > '9' || (count > 214748364 || (count == 214748364 && c > '7'))) return Error(msgId, "ESUB:Invalid SUBLIST count");
                        count = count * 10 + c - '0';
                    }
                    if (count == 0)
                    {
                        string created = base.DefaultAtCommandHandler("SUB", msgId,
                            new string[] { args[2], Topics.RX, args[3], "PUSH" });
                        if (created == null || created.IndexOf(MessageFormat.BuildOk(AtGroup, msgId, "sub,")) != 0)
                            return Error(msgId, "ESUB:" + (created ?? "No response"));
                    }
                }
                catch (Exception ex) { return Error(msgId, "ESUB:" + ex.Message); }
                return SendHandler("SEND", msgId, new string[] { args[0], args[1] });
            }
        }
        private string SendHandler(string cmd, int msgId, string[] args)
        {
            if (lora == null) return Error(msgId, "ENOTOPEN");
            if (string.IsNullOrEmpty(lora.LastSetupLoRa)) return Error(msgId, "ENOTSETUP");
            if (args == null || args.Length < 1) return Error(msgId, "EARGS:SEND,to(hex 0x..)[,text]");
            try
            {
                ushort to; if (!args[0].TryParseLoraAddress(out to)) return Error(msgId, "EARGS:SEND,to(hex 0x..)[,text]");
                string data = args.TextArg(1);
                txQueue.Enqueue(new TextPacket { From = loraAddress, To = to, Id = msgId, Data = data });
                txQueue.FireOneIf(lora.IsReadyForTX()); return Ok(msgId, "PacketEnqueued");
            }
            catch (Exception ex) { TraceLog(AtGroup, "send", msgId, ex.Message); return Error(msgId, "ESEND:" + ex.Message); }
        }
        private void Lora_OnPacketReceived(object source, RcvPacketArgs e)
        {
            rxQueue.Enqueue(e);
            txQueue.FireOneIf(txQueue.Count > 0 && lora.IsReadyForTX());
        }
        private void RxQueue_OnGetItem(object source, object item, int count)
        {
            try
            {
                RcvPacketArgs received = item as RcvPacketArgs;
                if (received != null)
                {
                    TextPacket textPkt = TextPacket.Create2(received.Packet);
                    if (textPkt.IsText)
                        TraceLog(AtGroup, "QueueRX", textPkt.Id, "count=" + count + " From=" + textPkt.From.ToString("X4") +
                            " To=" + textPkt.To.ToString("X4") + " Len=" + textPkt.Data.Length);
                    else TraceLog(AtGroup, "QueueRX", textPkt.Id, "Hdr: " + BitConverter.ToString(received.Packet, 0, textPkt.SizeOfHeader));
                    EmitEvent(Topics.RX, textPkt.Id, textPkt.From.ToString("X4"), textPkt.To.ToString("X4"),
                        received.RSSI.ToString(), received.SNR.ToString(), textPkt.Data);
                }
            }
            catch (Exception ex) { TraceLog(AtGroup, "queuerx", 0, ex.Message); }
        }
        private void TxQueue_OnGetItem(object source, object item, int count)
        {
            try
            {
                TextPacket packet = item as TextPacket;
                if (item is string) packet = new TextPacket { From = loraAddress, To = loraBroadcastAddress, Id = NextMsgId(), Data = (string)item };
                if (packet == null) { TraceLog(AtGroup, "queuetx", 0, "count=" + count + ", wrong item in the queue"); return; }
                byte[] buffer = packet.ToLoraArray2();
                TraceLog(AtGroup, "QueueTX", packet.Id, "count=" + count + ", Raw: " + BitConverter.ToString(buffer));
                lora.TransmitPacket(buffer);
            }
            catch (Exception ex) { TraceLog(AtGroup, "queuetx", 0, ex.Message); }
            finally
            {
                if (!txQueue.FireOneIf(count > 0 && lora.IsReadyForTX())) lora.EnableRxContinuous();
            }
        }
    }
}
