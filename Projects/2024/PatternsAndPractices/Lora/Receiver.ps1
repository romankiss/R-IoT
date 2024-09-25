#Lora Read packet via RYLR998 module using AT commands
function readlora {
    $counter = 0
    $COM = [System.IO.Ports.SerialPort]::getportnames()
    $port= new-Object System.IO.Ports.SerialPort $COM,115200,None,8,one
    $port.ReadTimeout = 5000
    $port.Open()
    $port.Write("AT" + "`r`n")
    $port.ReadLine()
    $port.Write("AT+RESET" + "`r`n")
    $port.ReadLine()
    #$port.Write("AT+BAND=868500000,M" + "`r`n")
    #$port.ReadLine()
    #$port.Write("AT+ADDRESS?" + "`r`n")
    #$port.ReadLine()

    do {
            
        $line = $port.ReadLine()
        Write-Host "$($line)" 
                    
    }
    while ($port.IsOpen)
    $port.Close()
    $port.Dispose()
}
