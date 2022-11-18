
# Raspberry Pi Lora Gateway

Ako náš LoRa gateway sme použili Raspberry Pi 3, na ktorý sme nasadili LoRa GPS Hat od spoločnosti Dragino. Operačný systém nainštalovaný na Raspberry Pi je Raspbian OS. Do Raspbianu sme museli nainštalovať packet forwarder pre LoRa sieť.

## Jednotlivé kroky vytvárania gateway

#### 1. Nasadenie LoRa Hat na Raspberry Pi

Ako prvé sme na GPIO header raspberry náš LoRa Hat. Následne sme našróbovali anténu potrebnú pre LoRa sieť.

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

**Naklonovanie repozitáru gateway softvéru:**

Do raspberry zadáme príkaz ako root: git clone https://github.com/dragino/dual_chan_pkt_fwd
 
**Povolíme SPI:**
**Inštalácia wiringPi
**Konfigurácia dual channel gateway kódu**
**Spustenie gateway aplikácie**






## Dodatočné informácie k jednotlivým krokom

Viac informácií o [LoRa-GPS-Hat](https://www.dragino.com/products/lora/item/106-lora-gps-hat.html)

Viac informácií o [Raspberry-Pi](https://www.raspberrypi.com/documentation/)











