#+++++++++++++++++++++++++++++++++++++++ E22 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#  E22-900T22U
#  E22-900T30D
#  E22-900T22S
#  broadcast address: 0xFFFF, channel: 0x12 (868.125MHz)
#++++++++++++++++++++++++++++++++++ E22 Configuration mode M1=1 M0=0 +++++++++++++++++++++++++++++++++++++++++++++++
$COM_Array = [System.IO.Ports.SerialPort]::getportnames()
$COM = $COM_Array[$COM_Array.Length - 1]
Write-Host "Port:" $COM
$port= new-Object System.IO.Ports.SerialPort $COM,9600,None,8,one
$port.ReadTimeout = 5000
#$port.Encoding = [System.Text.Encoding]::UTF8
$port.Open()

#set basic configuration with address 0x0001 ...
$req = 0xC0, 0x00, 0x09, 0x09, 0x01, 0x00, 0xE2, 0x00, 0x12, 0xC0, 0x00, 0x00     # addr: 0x0901, baudrate: 115200, netid: 0x00, P2P, 868.125Mhz  
$port.BaseStream.Write($req, 0, $req.Count)
$port.BaseStream.Flush()
Start-Sleep -Milliseconds 500  
$rsp = [System.Byte[]]::CreateInstance([System.Byte], 12)
$port.Read($rsp, 0, $rsp.Length)
Start-Sleep -Milliseconds 100 
Write-Host "Response:"  (($rsp|ForEach-Object ToString X2) -join ' ') -foreground black -BackgroundColor white

# closing and disposing
$port.Close()
$port.Dispose()
