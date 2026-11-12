// Trial replacement: atoms3r_cam_hal_v1 - UNVERIFIED
// CodeCraft generated artifact
// Source: CodeCraft_package1_sys_empty_lora@3.1.5.7
// Contract: CodeCraft_package1_sys_empty_lora@3.1.5.7:device/modules/sys/CodeCraft_sys_module.json#
// Target: nanoframework / C#
// Copyright © 2026 Roman Kiss, rkiss@pathcom.com
// Generated with assistance from ChatGPT and CodeCraft.
// Do not edit manually; update Model-First metadata and regenerate.
using CodeCraft.Common;
using System;
using System.Threading;
using System.IO;
using System.Text;
using System.Collections;
using System.Device.Gpio;
using System.Drawing;
using Iot.Device.Ws28xx.Esp32;
using Iot.Device.Button;
namespace CodeCraft.Modules.Sys
{
    public partial class SysModule : VirtualModule
    {
        private readonly object neoSync = new object();
        private Ws2808 neo;
        private GpioPin neoPowerPin;
        private int neoData = -1, neoCount = 1, neoPower = -1;
        private string neoOrder = "RGB", neoPowerActive = "HIGH";
        private static bool ParseColor(string text, out uint value)
        {
            value = 0; if (text == null) return false;
            text = text.ToUpper();
            switch (text)
            {
                case "BLACK": return true;
                case "RED": value = 0xFF0000; return true;
                case "GREEN": value = 0x00FF00; return true;
                case "BLUE": value = 0x0000FF; return true;
                case "WHITE": value = 0xFFFFFF; return true;
                case "YELLOW": value = 0xFFFF00; return true;
                case "CYAN": value = 0x00FFFF; return true;
                case "MAGENTA": value = 0xFF00FF; return true;
                case "ORANGE": value = 0xFFA500; return true;
                case "PURPLE": value = 0x800080; return true;
            }
            if (text.Length == 10 && text.Substring(0, 2) == "0X")
            {
                for (int i = 2; i < 10; i++)
                {
                    char c = text[i]; int digit = c >= '0' && c <= '9' ? c - '0' : c >= 'A' && c <= 'F' ? c - 'A' + 10 : -1;
                    if (digit < 0) return false; value = (value << 4) | (uint)digit;
                }
                return (value & 0xFF000000) == 0;
            }
            string[] parts = text.Split(':'); int r, g, b;
            if (parts.Length != 3 || !ParseDecimal(parts[0], out r) || !ParseDecimal(parts[1], out g) || !ParseDecimal(parts[2], out b) ||
                r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255) return false;
            value = ((uint)r << 16) | ((uint)g << 8) | (uint)b; return true;
        }
        private void WriteNeo(uint value, int delay = 0)
        {
            lock (neoSync)
            {
                if (neo == null) throw new InvalidOperationException("ENOTOPEN");
                int r = (byte)(value >> 16), g = (byte)(value >> 8), b = (byte)value;
                Color color = neoOrder == "GRB" ? Color.FromArgb(g, r, b) : neoOrder == "BRG" ? Color.FromArgb(b, r, g) : Color.FromArgb(r, g, b);
                neo.Image.SetPixel(0, 0, color); neo.Update();
            }
            if (delay > 0) Thread.Sleep(delay);
        }
        private void CloseNeo()
        {
            lock (neoSync)
            {
                try
                {
                    if (neo != null)
                        try { neo.Image.SetPixel(0, 0, Color.FromArgb(0, 0, 0)); neo.Update(); }
                        finally { neo.Dispose(); }
                }
                finally
                {
                    neo = null;
                    try
                    {
                        if (neoPowerPin != null)
                            try { neoPowerPin.Write(neoPowerActive == "HIGH" ? PinValue.Low : PinValue.High); }
                            finally { ioctrl.ClosePin(neoPower); }
                    }
                    finally { neoPowerPin = null; /* Preserve the last successful OPEN binding for INFO. */ }
                }
            }
        }
        private string NeoHandler(string cmd, int msgId, string[] args)
        {
            if (args == null || args.Length == 0) return Error(msgId, "EARGS");
            string operation = args[0].ToUpper();
            try
            {
                if (operation == "SET" || operation == "OFF" || operation == "BLINK")
                {
                    if (neo == null) return Error(msgId, "ENOTOPEN");
                    if (operation == "OFF")
                    {
                        if (args.Length != 1) return Error(msgId, "EARGS"); WriteNeo(0); return Ok(msgId, "neo,off");
                    }
                    uint color;
                    if (operation == "SET")
                    {
                        if (args.Length != 2) return Error(msgId, "EARGS");
                        if (!ParseColor(args[1], out color)) return Error(msgId, "ECOLOR");
                        WriteNeo(color); return Ok(msgId, "neo,set,0x" + color.ToString("X8"));
                    }
                    if (args.Length < 3 || args.Length > 4) return Error(msgId, "EARGS");
                    if (!ParseColor(args[1], out color)) return Error(msgId, "ECOLOR");
                    int count, phase = 25;
                    if (!ParseDecimal(args[2], out count) || (args.Length == 4 && !ParseDecimal(args[3], out phase)) ||
                        count < 1 || count > 5 || phase < 10 || phase > 100) return Error(msgId, "ERANGE");
                    for (int i = 0; i < count; i++) { WriteNeo(color, phase); WriteNeo(0, phase); }
                    return Ok(msgId, "neo,blink,count=" + count + ",phaseMs=" + phase);
                }
                lock (neoSync)
                {
                    if (operation == "OPEN")
                    {
                        if (neo != null) return Ok(msgId, "neo,open,already");
                        if (args.Length < 2 || args.Length > 6) return Error(msgId, "EARGS");
                        int data, count = 1, power = -1;
                        string order = args.Length > 3 ? args[3].ToUpper() : "RGB", active = args.Length > 5 ? args[5].ToUpper() : "HIGH";
                        if (!ParseDecimal(args[1], out data) || data < 0 || (args.Length > 2 && !ParseDecimal(args[2], out count)) || count < 1 ||
                            (args.Length > 4 && !ParseDecimal(args[4], out power)) || power < -1 || power == data ||
                            (order != "RGB" && order != "GRB" && order != "BRG") || (active != "HIGH" && active != "LOW")) return Error(msgId, "ECONFIG");
                        GpioPin acquiredPower = null; Ws2808 acquiredNeo = null;
                        try
                        {
                            if (power >= 0) { acquiredPower = ioctrl.OpenPin(power, PinMode.Output); acquiredPower.Write(active == "HIGH" ? PinValue.High : PinValue.Low); }
                            acquiredNeo = new Ws2808(data, count);
                            neoData = data; neoCount = count; neoPower = power; neoOrder = order; neoPowerActive = active;
                            neoPowerPin = acquiredPower; neo = acquiredNeo;
                        }
                        catch
                        {
                            try { if (acquiredNeo != null) acquiredNeo.Dispose(); }
                            finally
                            {
                                if (acquiredPower != null)
                                    try { acquiredPower.Write(active == "HIGH" ? PinValue.Low : PinValue.High); }
                                    finally { ioctrl.ClosePin(power); }
                            }
                            throw;
                        }
                        return Ok(msgId, "neo,open");
                    }
                    if (operation == "CLOSE")
                    {
                        if (args.Length != 1) return Error(msgId, "EARGS");
                        if (neo == null) return Ok(msgId, "neo,close,already"); CloseNeo(); return Ok(msgId, "neo,close");
                    }
                    if (operation == "INFO")
                    {
                        if (args.Length != 1) return Error(msgId, "EARGS");
                        return MessageFormat.BuildData(AtGroup, msgId, "neo,opened=" + (neo != null) + ",data=" + neoData + ",count=" + neoCount +
                            ",power=" + neoPower.ToString() + ",order=" + neoOrder + ",powerActive=" + neoPowerActive);
                    }
                }
                return Error(msgId, "EOPERATION");
            }
            catch (Exception ex) { return Error(msgId, ex.Message == "ENOTOPEN" ? "ENOTOPEN" : "EPLATFORM:" + ex.Message); }
        }
    }
}
