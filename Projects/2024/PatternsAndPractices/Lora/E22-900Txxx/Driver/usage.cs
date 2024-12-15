// code snippet

static int loopback_counter = 0;
static E22 lora = null;
static ushort broadcastAddress = 0xFFFF;
const ushort loraAddress = 0x0901;  //55;
const byte loraNetworkId = 0x12;    // 850.125 + 18 = 868.125Mhz



Configuration.SetPinFunction(pinCOM2_TX, DeviceFunction.COM2_TX);
Configuration.SetPinFunction(pinCOM2_RX, DeviceFunction.COM2_RX);
lora = E22.Create("COM2", loraAddress: loraAddress);
if (lora != null)
{
    lora.OnPacketReceived += (sender, e) =>
    {
        Debug.WriteLine($"nFmem_LoRaRcv={Memory.Run(true)}");
        if (e.Data.StartsWith("6C6F6F706261636B"))
        {
            int counter = Interlocked.Increment(ref loopback_counter);
            lora.SendAsync(e.AddressID, $"Loopback_#{counter:D4}", loraNetworkId);
        }
        //WriteTextOnScreen($"E22: {e.AddressID:X4} {e.Data}", X: 0, Y: LCD_Y_StatusLine, fcolor: Color.GreenYellow, maxChars: LCD_NumberOfSmallChars, font: SmallFont);
    };
    sensors.Append("LoRa ");
}
