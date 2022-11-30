
# Raspberry Pi Lora Gateway

Ako náš LoRa gateway sme použili Raspberry Pi 3, na ktorý sme nasadili LoRa GPS Hat od spoločnosti Dragino. Operačný systém nainštalovaný na Raspberry Pi je Raspbian OS. Do Raspbianu sme museli nainštalovať packet forwarder pre LoRa sieť.

## Jednotlivé kroky vytvárania gateway

#### 1. Nasadenie LoRa Hat na Raspberry Pi

Ako prvé sme na GPIO header raspberry nasadili náš LoRa Hat. Následne sme našróbovali anténu potrebnú pre LoRa sieť.

![Raspberry](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Raspberry.png)

#### 2. Inštalácia OS na Raspberry Pi

Na raspberry sme museli najprv nainštalovať operačný systém. Zvolili sme si ako operačný systém Raspbian, pretože je pre nás veľmi výhodný. 
Ako prvé sme si museli zo stránky raspberry stiahnuť Raspberry Pi Imager.
Ten nám slúži aby sme mohli nainštalovať OS na našu SD kartu.
SD kartu sme vložili do PC a spustili Raspberry Pi Imager. 
Vybrali sme vhodný OS a nahrali ho na SD kartu. Následne sme vložili SD kartu do raspbery a zapli sme ho.
Dokončili sme inštaláciu už na samotnom raspberry. 

#### 3. Inštalácia packet forwarderu 

Jednotlivé kroky inštalácie:

*Naklonovanie repozitáru gateway softvéru:*

Do raspberry zadáme príkaz ako root: *git clone https://github.com/dragino/dual_chan_pkt_fwd*

![Clone](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Clone.png)
 
*Povolíme SPI:*

Zadáme príkaz na otvorenie nastavení raspberry: *sudo raspi-config*

![Raspi-config](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Raspi-config.png)

*Vyberieme možnosť: Možnosti rozhrania*

![Interface_options](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Interface_options.png)

*Vyberieme SPI:*

![SPI](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/SPI.png)

*Povolíme rozhranie SPI:*

![Enable](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Enable.png)

Následne reštartujeme raspberry pomocou príkazu: *sudo shutdown -r now*

*Inštalácia wiringPi:*

Nainštalujeme wiringPi pomocou príkazu: *sudo apt-get install wiringpi*

![wiringPi](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/wiringPi.png)

*Konfigurácia dual channel packet forwared kódu:*

Prejdeme do adresára v ktorom máme uložený packet forwarder pomocou príkazu: *cd dual_chan_pkt_fwd*.
Spustíme si editor nano v ktorom upravíme parametre ako frekvenciu na ktorej chceme vysielať, server na ktorý chceme vysielať atď. Do editora sa dostaneme príkazom: *nano global_conf.json*

![global_conf_json](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/global_config_json.png)

*Spustenie softvéru gateway:*

Na záver už len spustíme samotný gateway softvér príkazom: *sudo ./dual_chan_pkt_fwd*

![Launch](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Launch.png)


## Dodatočné informácie k jednotlivým krokom

Viac informácií o [LoRa-GPS-Hat](https://www.dragino.com/products/lora/item/106-lora-gps-hat.html)

Viac informácií o [Raspberry-Pi](https://www.raspberrypi.com/documentation/)











