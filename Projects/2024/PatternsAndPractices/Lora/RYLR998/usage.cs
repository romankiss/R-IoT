



#region LoRa
// Use COM2/COM3 to avoid conflict with the USB Boot port
Configuration.SetPinFunction(pinCOM2_TX, DeviceFunction.COM2_TX);
Configuration.SetPinFunction(pinCOM2_RX, DeviceFunction.COM2_RX);
lora = RYLR998.Create("COM2", loraAddress, pinLoRa_RST, ioctrl);
if (lora != null)
{
    lora.OnPacketReceived += (sender, e) =>
    {
        if (e.AddressID == 911 && e.Data.StartsWith("loopback"))
            lora.SendAsync(e.AddressID, e.Data);
    };
}
#endregion
