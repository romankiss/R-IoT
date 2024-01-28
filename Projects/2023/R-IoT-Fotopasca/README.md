


  <h1>Fotopasca</h1>
    <p>Po zistení pohybu sa zoberie snímka, ktorá sa buď vyhodnotí na mieste, alebo bude posielaná správcovy.</p>
    <ul>
      <li>Žiadne spojenie - iba lokálne ukladanie snímok na napr. SD kartu</li>
      <li>Spojenie cez LoRa / Long Dist. WiFi</li>
    </ul>

    
## Dosavadný pokrok:    
![ESP32 a PIR pohyb. senzor](https://github.com/romankiss/R-IoT/assets/59760649/923af352-fe12-4d16-89da-fdbeb8ebde39)    
(prepojenie pinov a komponenty sú len ilustračné, výsledky sa môžu líšiť)

## Do budúcna
Bolo by fajn dosiahnuť vysokú odolnosť voči výpadkom v tomto systéme.     
Eliminovať centralizované úložisko dát a tím aj možnosť jeho zlyhania by mohla pomôcť technológia IPFS (P2P decentral. protokol na ukladanie dát a ich adresovanie pomocov ich hashov).  
Vytvorenie alternatívnych trás pre pripojenie do internetu bude už ťažšie dosiahnuteľné. Do úvahy prichádzajú pripojenia typu LoRa alebo LongRangeWiFi(ak také niečo vôbec existuje).
