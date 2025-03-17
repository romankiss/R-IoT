
<h3>Notes</h3>
1. Treba doplnit v dokumentacii (Formát LoRa správy), ze sprava (payload) je ukonceny CRLF (0x0D, 0x0A) 
</br>
 example:       <b2> #123T25.55H48.12P998D10\r\n </b2>     
</br>
</br>
2. Vysielanie telemetry data ma byt kazdu secundu, takze
</br>
const int pub_period = <b>1000</b>;     // miliseconds
</br>
</br>
pre lepsiu spolahlivost pri tejto frekvencie vysielania je lepsie pouzit SendAsync prikaz, takze
</br>
lora.<b>SendAsync</b>(address: BroadcastAddress, data: payload);
</br>
</br>
Pre zistenie kolko je free memory a spustenie GC (garbage collection) sa pouziva call
</br>
Memory.Run(true)
</br>
Pre zistenie kolko je len free memory sa pouziva call
</br>
Memory.Run(false)
</br>
takze je dobre mat prehlad o poziti nF memory, ci nie je nejaky leak, atd. na zaciatku Fire a pri jeho ukonceni.
