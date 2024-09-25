#Lora Send packet via RYLR998 module using AT commands
function writelora {
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

        $data = "HELLO LORA, #" + $($counter)
        $port.Write("AT+SEND=0,$($data.Length),$($data)" + "`r`n")
        $line = $port.ReadLine()
        Write-Host "$($counter) - $($line)" 
        $counter = $counter + 1
        Start-Sleep -Milliseconds 250                
    }
    while ($port.IsOpen)
    $port.Close()
    $port.Dispose()
}
