#Lora Read packet via RYLR998 module using AT commands
function readlora {
    $counter = 0
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
    #$port.Write("AT+BAND=868500000,M" + "`r`n")
    #$port.ReadLine()
    #$port.Write("AT+ADDRESS=2000"+ "`r`n")     
    #$port.ReadLine()

    do {
        try{
            $line = $port.ReadLine()
            Write-Host "$($line)" 
        }catch{
            Write-Host "Exception occured when tried to read from port, probably there are no new messages."
        }                  
    }
    while ($port.IsOpen)
    $port.Close()
    $port.Dispose()
}

 #   $port.Write("AT+SEND=1201,5,12345" + "`r`n")
 #   $port.ReadLine()

# $port.Write("AT+SEND=0,5,HELLO`r`n")
 #$port.ReadLine()
