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


$req1 = 0xC1, 0x00, 0x09
$port.BaseStream.Write($req1, 0, $req1.Count)
$port.BaseStream.Flush()
Start-Sleep -Milliseconds 500  
$rsp = [System.Byte[]]::CreateInstance([System.Byte], 12)
$port.Read($rsp, 0, $rsp.Length)
Start-Sleep -Milliseconds 100 
Write-Host "Response:"  (($rsp|ForEach-Object ToString X2) -join ' ') -foreground black -BackgroundColor white




#read device info product
$req1 =  0xC1, 0x80, 0x07 #0xC1, 0x00, 0x09 ...............c1=read/response, next byte says the register location where to start(0 to 8th (9B total)), the next num says how much bytes to operate with from the start byte (0X00, 0X09 means start prom the firs byte and operate with all nine bytes), Byte and bit meanings are decribed in the manual on pages 15-17
$port.BaseStream.Write($req1, 0, $req1.Count)
$port.BaseStream.Flush()
Start-Sleep -Milliseconds 500  
$rsp = [System.Byte[]]::CreateInstance([System.Byte], 12)
$port.Read($rsp, 0, $rsp.Length)
Start-Sleep -Milliseconds 100 
Write-Host "Response:"  (($rsp|ForEach-Object ToString X2) -join ' ') -foreground black -BackgroundColor white

#closing and disposing
$port.Close()
$port.Dispose()







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
#      set   strtB #B    AddH  addL  NetID x1    x2    chan  other cryptHcryptL 
$req = 0xC0, 0x00, 0x09, 0x12, 0x34, 0x12, 0xE2, 0x00, 0x12, 0xC0, 0x00, 0x00     # addr: 0x0901, baudrate: 115200, netid: 0x00, P2P, 868.125Mhz  wrong order!!!!!!
#  x1: baudrate for uart, serial parity bit, air data rate. x2: subpacketing, rssi, transmitting power    ----more data is coded inside these one Byte(eg. baudrate takes 3 bits from this B)
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






# ***************** Normal mode M1=0 M0=0 *****************************************************************
$COM_Array = [System.IO.Ports.SerialPort]::getportnames()
$COM = $COM_Array[$COM_Array.Length - 1]
Write-Host "Port:" $COM
$port= new-Object System.IO.Ports.SerialPort $COM,115200,None,8,one
$port.ReadTimeout = 35000
#$port.Encoding = [System.Text.Encoding]::UTF8
$port.Open()


#0C-22-12-C1-00-0A-04-44-41-54-41-F0 maybe

#               [DestAddr (BC),   NetId], Payload(0xC1, SourceId, Len, data ...)   
[Byte[]] $req1 = 0xFF, 0xFF, 0x12,    0xAA, 0xBB #0xC1, 0x00, 0x0A, 0x05, 0x11, 0x22, 0x33, 0xAB, 0xBA, 0x55
#$req1 = 0xC0, 0xC1, 0xC2, 0xC3, 0x02, 0x01
$port.Write($req1, 0, $req1.Count)
$port.ReadExisting()
$port.BaseStream.Flush()
Start-Sleep -Milliseconds 500 
#$rsp = ""
#$port.Read($rsp, 0, $rsp.Length)
#Write-Host $rsp
$pcktCnt = 0
$rsp = [System.Byte[]]::CreateInstance([System.Byte], 9)
while(1){
    $port.BaseStream.Flush()
    #$rsp = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}
    $port.Read($rsp, 0, 9)
    Write-Host $port.BytesToRead
    $pcktCnt = $pcktCnt + 1
    Write-Host "RCV: " $rsp "No.:" $pcktCnt
    Start-Sleep -Milliseconds 100
}


# closing and disposing
$port.Close()
$port.Dispose()
