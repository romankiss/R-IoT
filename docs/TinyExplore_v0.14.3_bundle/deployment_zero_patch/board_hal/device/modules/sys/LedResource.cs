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
        private readonly object ledSync = new object();
        private GpioPin led;
        private int ledPin = -1;
        private string ledActive = "HIGH";
        private void WriteLed(bool active, int delay = 0)
        {
            lock (ledSync)
            {
                if (led == null) throw new InvalidOperationException("ENOTOPEN");
                led.Write(active == (ledActive == "HIGH") ? PinValue.High : PinValue.Low);
            }
            if (delay > 0) Thread.Sleep(delay);
        }
        private string LedHandler(string cmd, int msgId, string[] args)
        {
            if (args == null || args.Length == 0) return Error(msgId, "EARGS");
            string operation = args[0].ToUpper();
            try
            {
                if (operation == "ON" || operation == "OFF" || operation == "TOGGLE" || operation == "BLINK")
                {
                    if (led == null) return Error(msgId, "ENOTOPEN");
                    if (operation == "BLINK")
                    {
                        int count, phase = 25;
                        if (args.Length < 2 || args.Length > 3) return Error(msgId, "EARGS");
                        if (!ParseDecimal(args[1], out count) || (args.Length == 3 && !ParseDecimal(args[2], out phase)) || count < 1 || count > 5 || phase < 10 || phase > 100)
                            return Error(msgId, "ERANGE");
                        for (int i = 0; i < count; i++) { WriteLed(true, phase); WriteLed(false, phase); }
                        return Ok(msgId, "led,blink,count=" + count + ",phaseMs=" + phase);
                    }
                    if (args.Length != 1) return Error(msgId, "EARGS");
                    if (operation == "TOGGLE")
                    { lock (ledSync) { if (led == null) return Error(msgId, "ENOTOPEN"); led.Toggle(); } }
                    else WriteLed(operation == "ON");
                    return Ok(msgId, "led," + operation.ToLower());
                }
                lock (ledSync)
                {
                    if (operation == "OPEN")
                    {
                        if (led != null) return Ok(msgId, "led,open,already");
                        if (args.Length != 3) return Error(msgId, "EARGS");
                        int pin; string active = args[2].ToUpper();
                        if (!ParseDecimal(args[1], out pin) || pin < 0 || (active != "HIGH" && active != "LOW")) return Error(msgId, "ECONFIG");
                        GpioPin acquired = null;
                        try { acquired = ioctrl.OpenPin(pin, PinMode.Output); acquired.Write(active == "HIGH" ? PinValue.Low : PinValue.High); led = acquired; ledPin = pin; ledActive = active; }
                        catch { if (acquired != null) ioctrl.ClosePin(pin); throw; }
                        return Ok(msgId, "led,open");
                    }
                    if (operation == "CLOSE")
                    {
                        if (args.Length != 1) return Error(msgId, "EARGS");
                        if (led == null) return Ok(msgId, "led,close,already");
                        try { led.Write(ledActive == "HIGH" ? PinValue.Low : PinValue.High); }
                        finally { try { ioctrl.ClosePin(ledPin); } finally { led = null; } }
                        return Ok(msgId, "led,close");
                    }
                    if (operation == "INFO")
                    {
                        if (args.Length != 1) return Error(msgId, "EARGS");
                        return MessageFormat.BuildData(AtGroup, msgId, "led,opened=" + (led != null) + ",pin=" + ledPin.ToString() + ",active=" + ledActive);
                    }
                }
                return Error(msgId, "EOPERATION");
            }
            catch (Exception ex) { return Error(msgId, ex.Message == "ENOTOPEN" ? "ENOTOPEN" : "EPLATFORM:" + ex.Message); }
        }
    }
}
