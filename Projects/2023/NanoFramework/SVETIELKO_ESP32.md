# Návod na blikanie ledkou

## Čo by už malo byť hotové:   
-ESP32 s nanoFrejmworkom   
-Vizualko so vším všudy   

## Extra knižnica:  
![scr1](https://github.com/romankiss/R-IoT/assets/59760649/5d64516f-cd6a-4db9-a8e7-6a09c368f4d5)   
pravý klik na Solution 'vaše pomenovanie'   
![scr2](https://github.com/romankiss/R-IoT/assets/59760649/bd867514-0158-4949-bf86-cb5883530a29)   
klik na Manage NuGet Packages for Solution   
![scr3](https://github.com/romankiss/R-IoT/assets/59760649/fc275163-4784-4eb8-adc9-8eee6abfc6c8)   
Browse   
Vyhľadajte : nanoFramework.AtomLite   
Klik na nanoFramework.AtomLite , napravo zaškrtnite svoj projekt a kliknite na install

//Extra Turbo skupina

## Kód:
 ```
using nanoFramework.AtomLite;
using System.Drawing;
...
while (true)
{


    AtomLite.NeoPixel.Image.SetPixel(0, 0, Color.Red);
    AtomLite.NeoPixel.Update();

    Thread.Sleep(5000);
    //vypnutie led
    AtomLite.NeoPixel.Image.Clear();
    AtomLite.NeoPixel.Update();
    //počkanie na dobu 5sec
    Thread.Sleep(5000);
}
 ```
## Obete:
-Maťova červená ledka, odpálená?(svietila -> už nesvieti), ostatné farby ok
## UwU:
![pls potešilo by](https://shop.m5stack.com/products/m5paper-esp32-development-kit-v1-1-960x540-4-7-eink-display-235-ppi)

Spolupáchateľ: Matej X

