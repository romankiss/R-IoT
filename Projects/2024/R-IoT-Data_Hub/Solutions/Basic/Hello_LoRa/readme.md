# This example demonstrates the "Hello World!" program for the REYAX RYLR998 LoRa module
[This](https://reyax.com//upload/products_download/download_file/LoRa_AT_Command_RYLR998_RYLR498_EN.pdf) LoRa module is really tiny and communicates (with the "master" device) via standard AT commands.  
Using [this](https://shop.m5stack.com/products/m5stamp-isp-serial-programmer-module-ch9102) programmer/adapter it is quite easy to start with a simple LoRa demo via connecting it to a standard USB port.  
At the end of this demo we should be able to use this module to send or receive data via the LoRa wireless protocol.
## Pinout
Being aware of the proper connecting and wiring for the module is crucial. The module uses 3,3V logic and power, is not protected from higher or reverse voltege(exchanged Vdd with Gnd) so a mistake will destroy the board.  
Tx (TxD) pin is connected to the Rx (RxD) from the USB-programmer, Rx from the module is connected to Tx on the programmer/adapter/"master" device.  
If using the programmer mentioned above, connecting to a computer can be done with a USB-C to USB-A cable.
## Example
After the module is properly connected, we can start by configuring it. To send the AT commands we will use the scripting abilities of the Windows Powershell ISE program.  
In the upper text editor we will write our script. To execute it, the "Run selection" button is the best option - runs and executes the currently selected code.  
You can save your script for later.  
 ```
$COM = [System.IO.Ports.SerialPort]::getportnames() 
    Write-Host $("Device should be connected at port: $COM")
    $port= new-Object System.IO.Ports.SerialPort $COM,115200,None,8,one
    $port.ReadTimeout = 5000
    $port.Open()
    $port.Write("AT" + "`r`n")
    $port.ReadLine()
    #Attention! moving-disconnecting the contact on the pins will force the device to reset - can be seen when there are no commands send, but the blue led on the adaper blinks. After each reset, there will be a "+READY" response generated - be aware of reading the newest line from the buffer(?)
    $port.Write("AT+RESET" + "`r`n")
    #after the reset command, there must be 2 reads, to get the full 2 responses, else later in the program the read will be reading the second newest, not the newest line from the buffer(?)
    $port.ReadLine()
    $port.ReadLine()
    $port.Write("AT+BAND=868500000,M" + "`r`n")
    $port.ReadLine()
    $port.Write("AT+ADDRESS=<your adress from the supported range (see manual)>"+ "`r`n")     
    $port.ReadLine()
 ```
The above code snipped should be used to configure the module, do not reconfigure frequently to avoid potentional damage.  
To check the current config or to config the NETID in which you will comunicate see the module manual.  
Next time when you connect, comment out the last 4 lines to avoid config again (frequent reconfig is not good for the module).  
To safely close the connection between the PC and the module the following code is used:
 ```
    $port.Close()
    $port.Dispose()
 ```
Here is an example of sending a broadcas message (to the whole network you are currently in) (BC address = 0):
 ```
 $port.Write("AT+SEND=0,5,HELLO`r`n")
 $port.ReadLine()
 ```
To see if we received any messages use: 
 ```
$line = $port.ReadLine()
Write-Host "$($line)"
 ```
Be aware that sometimes the buffer we read from can contain older messages or responses from the module. Use the above code to view the lines writen to the buffer.  
It will read the oldest unread message.  


