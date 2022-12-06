# Raspberry Pi Lora Gateway

As our LoRa gateway we used a Raspberry Pi 3, on which we deployed the LoRa GPS Hat from Dragino. The operating system installed on the Raspberry Pi is Raspbian OS. We had to install a packet forwarder for the LoRa network in Raspbian.

## The individual steps of creating a gateway

#### 1. Mounting LoRa Hat on Raspberry Pi

First we mounted our LoRa Hat on the raspberry GPIO header. Then we screwed up the antenna needed for the LoRa network.

![Raspberry](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Raspberry.png)

#### 2. Installing the OS on the Raspberry Pi

On raspberry we had to install the operating system first. We chose Raspbian as the operating system because it is very convenient for us. 
First we had to download the Raspberry Pi Imager from the raspberry website.
This is used to allow us to install the OS on our SD card.
We inserted the SD card into the PC and started the Raspberry Pi Imager. 
We selected a suitable OS and loaded it onto the SD card. We then inserted the SD card into the raspbery and turned it on.
We finished the installation already on the raspberry itself. 

#### 3. Packet forwarder installation 

Installation steps:

*Cloning the gateway software repository:*

Enter the command as root in raspberry: *git clone https://github.com/dragino/dual_chan_pkt_fwd*

![Clone](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Clone.png)
 
*Enable SPI:*

Enter the command to open raspberry settings: *sudo raspi-config*

![Raspi-config](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Raspi-config.png)

*Select Interface Options*

![Interface_options](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Interface_options.png)

*Select SPI:*

![SPI](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/SPI.png)

*Enable SPI interface:*

![Enable](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Enable.png)

Then restart raspberry using the command: *sudo shutdown -r now*

*Installation of wiringPi:*

Install wiringPi using the command: *sudo apt-get install wiringpi*

![wiringPi](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/wiringPi.png)

*Configuration of dual channel packet forwared code:*

Go to the directory where we have the packet forwarder stored using the command: *cd dual_chan_pkt_fwd*.
Run the nano editor in which we edit parameters such as the frequency we want to transmit on, the server we want to transmit to, etc. To get to the editor, use the command: *nano global_conf.json*

![global_conf_json](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/global_config_json.png)

*Starting the gateway software:*

Finally, we just run the gateway software itself with the command: *sudo ./dual_chan_pkt_fwd*

![Launch](https://github.com/romankiss/R-IoT/blob/main/Projects/2022/IoT-Enviro-sensor/images/Launch.png)


## Additional information on individual steps

Read more about [LoRa-GPS-Hat](https://www.dragino.com/products/lora/item/106-lora-gps-hat.html)

Read more about [Raspberry-Pi](https://www.raspberrypi.com/documentation/)
