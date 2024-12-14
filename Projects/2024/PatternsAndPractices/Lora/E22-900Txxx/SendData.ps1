# ***************** Normal mode M1=0 M0=0 *****************************************************************
$COM_Array = [System.IO.Ports.SerialPort]::getportnames()
$COM = $COM_Array[$COM_Array.Length - 1]
Write-Host "Port:" $COM
$port= new-Object System.IO.Ports.SerialPort $COM,115200,None,8,one
$port.ReadTimeout = 35000
#$port.Encoding = [System.Text.Encoding]::UTF8
$port.Open()

#               [DestAddr,   NetId], Payload(0xC1, SourceId, Len, data ...)   
[Byte[]] $req1 = 0x09, 0x01, 0x12,   0xC1, 0x09, 0x01, 0x05, 0x11, 0x22, 0x33, 0xAB, 0xBA, 0x55
#$req1 = 0xC0, 0xC1, 0xC2, 0xC3, 0x02, 0x01
$port.Write($req1, 0, $req1.Count)
$port.ReadExisting()
$port.BaseStream.Flush()
Start-Sleep -Milliseconds 500 



# closing and disposing
$port.Close()
$port.Dispose()
