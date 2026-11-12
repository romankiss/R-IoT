// Trial replacement: atoms3r_cam_hal_v1 - UNVERIFIED
// Replacement revision: button_open_context_v2 - UNVERIFIED
// Preview whole-file replacement derived from the verified v3.1.5.14 pair.
// CodeCraft generated artifact
// Source: CodeCraft_package1_sys_empty_lora@3.1.5.9
// Contract: CodeCraft_package1_sys_empty_lora@3.1.5.9:device/modules/sys/CodeCraft_sys_module.json#
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
        private readonly object buttonSync = new object();
        private GpioButton button;
        private int buttonPin = -1, buttonDebounceMs = 333;
        private PinMode buttonMode = PinMode.InputPullUp;
        private int buttonContext;
        private int ctx;
        private int buttonPressCounter;
        private bool buttonHolding;
        private void CloseButton()
        {
            lock (buttonSync)
            {
                GpioButton owned = button; button = null;
                try
                {
                    if (owned != null) { owned.Press -= Button_Press; owned.Holding -= Button_Holding; owned.Dispose(); }
                }
                finally { button = null; buttonContext = 0; ctx = 0; buttonHolding = false; buttonPressCounter = 0; }
            }
        }
        private string ButtonHandler(string cmd, int msgId, string[] args)
        {
            if (args == null || args.Length == 0) return Error(msgId, "EARGS");
            string operation = args[0].ToUpper();
            try
            {
                lock (buttonSync)
                {
                    if (operation == "OPEN")
                    {
                        if (button != null) return Ok(msgId, "button,open,already");
                        if (args.Length < 2 || args.Length > 4) return Error(msgId, "EARGS");
                        int pin, debounce = 333; PinMode mode = PinMode.InputPullUp;
                        if (!ParseDecimal(args[1], out pin) || pin < 0 || (args.Length > 3 && !ParseDecimal(args[3], out debounce)) || debounce < 0) return Error(msgId, "ECONFIG");
                        if (args.Length > 2)
                        {
                            if (args[2] == "Input") mode = PinMode.Input;
                            else if (args[2] == "InputPullDown") mode = PinMode.InputPullDown;
                            else if (args[2] != "InputPullUp") return Error(msgId, "ECONFIG");
                        }
                        GpioButton acquired = null;
                        try
                        {
                            acquired = new GpioButton(buttonPin: pin, gpio: ioctrl, shouldDispose: false, pinMode: mode, debounceTime: TimeSpan.FromMilliseconds(debounce));
                            acquired.IsHoldingEnabled = true; acquired.IsDoublePressEnabled = false; acquired.Press += Button_Press; acquired.Holding += Button_Holding;
                            buttonPin = pin; buttonMode = mode; buttonDebounceMs = debounce; buttonContext = 0; buttonHolding = false; buttonPressCounter = 0; button = acquired;
                        }
                        catch
                        {
                            if (acquired != null) { acquired.Press -= Button_Press; acquired.Holding -= Button_Holding; acquired.Dispose(); }
                            throw;
                        }
                        ctx = msgId;
                        return Ok(msgId, "button,open");
                    }
                    if (operation == "CLOSE")
                    {
                        if (args.Length != 1) return Error(msgId, "EARGS");
                        if (button == null) { CloseButton(); return Ok(msgId, "button,close,already"); } CloseButton(); return Ok(msgId, "button,close");
                    }
                    if (operation == "INFO")
                    {
                        if (args.Length != 1) return Error(msgId, "EARGS");
                        return MessageFormat.BuildData(AtGroup, msgId, "button,opened=" + (button != null) + ",pin=" + buttonPin.ToString() +
                            ",mode=" + buttonMode + ",debounceMs=" + buttonDebounceMs + ",holding=" + buttonHolding);
                    }
                }
                return Error(msgId, "EOPERATION");
            }
            catch (Exception ex) { return Error(msgId, "EPLATFORM:" + ex.Message); }
        }
        private void Button_Press(object sender, EventArgs args)
        {
            lock (buttonSync)
            {
                if (button == null || sender != button) return;
                int context = buttonHolding ? buttonContext : NextMsgId();
                buttonPressCounter = buttonPressCounter == int.MaxValue ? 1 : buttonPressCounter + 1;
                EmitEvent(Topics.ButtonPress, context, buttonPressCounter.ToString(), $"ctx{ctx:X8}");
            }
        }
        private void Button_Holding(object sender, ButtonHoldingEventArgs args)
        {
            lock (buttonSync)
            {
                if (button == null || sender != button) return;
                if (args.HoldingState == ButtonHoldingState.Started)
                { if (buttonContext == 0) buttonContext = NextMsgId(); buttonHolding = true; EmitEvent(Topics.ButtonHoldingStarted, buttonContext, $"ctx{ctx:X8}"); }
                else if (args.HoldingState == ButtonHoldingState.Completed)
                { EmitEvent(Topics.ButtonHoldingCompleted, buttonContext, $"ctx{ctx:X8}"); buttonHolding = false; buttonContext = 0; }
            }
        }
    }
}
