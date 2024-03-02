
<h2>16x8 LED Matrix SimpleHT16K33 Driver</h2>

 <img  width="35%" src="https://github.com/romankiss/R-IoT/assets/30365471/bd4ec62f-9547-4d50-8851-4d251419afa4">

 
https://github.com/romankiss/R-IoT/assets/30365471/1dfcdfa0-eb1b-46b6-97a7-d97cd57363f4



<br></br>

<h3>Tool for creating a Font8x8 pixels</h3>

<li><a href="https://xantorohara.github.io/led-matrix-editor/#0000000000a000e0">LED Matrix Editor</a></li>

<h4>Example of pixels for '3' used in the driver</h4>

![image](https://github.com/romankiss/R-IoT/assets/30365471/2bd01149-aafe-4552-b894-1e6a46684bb2)






<h2>Usage of the SimpleHT16K33 driver</h2>

       static SimpleHT16K33 ht16k33 = null;

<h3>Initialize driver</h3>

        #region HT16K33  
        I2cDevice i2c_ht16k33 = new(new I2cConnectionSettings(1, 0x70)); // Grove connector
        res = i2c_ht16k33.WriteByte(0x07);
        if (res.Status == I2cTransferStatus.FullTransfer)
        {
            ht16k33 = new SimpleHT16K33(i2c_ht16k33);
            ht16k33.Init();
            ht16k33.SetBrightness(10);
            ht16k33.SetBlinkRate();
            ht16k33.Clear();
            ht16k33.ShowMessage("Sensors=OK");
            ht16k33.ShowAndCirculateMessageAsync("wifi", 250);
        }          
        #endregion



<h3>Demo of the above code</h3>
        

https://github.com/romankiss/R-IoT/assets/30365471/0bbf4594-9123-4d63-ae73-36d8e3e9dcb5


<br></br>
<h3>HT16K33 wired to M5CapsuleS3 Controller as a PnP IoT Device connected to Azure IoT Central</h3>

 https://github.com/romankiss/R-IoT/assets/30365471/788edb9b-9fc3-4c75-a8cb-8053cc2f3ab7 

<br></br>
<h3>HT16K33 wired to Atom Lite Controller as a PnP IoT Device connected to Azure IoT Central</h3>


https://github.com/romankiss/R-IoT/assets/30365471/dd19294e-efe6-48c1-9ae1-cd9f13b3eb61




