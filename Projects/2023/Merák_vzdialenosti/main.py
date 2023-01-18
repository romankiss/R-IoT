from hcsr04 import HCSR04
import neopixel
import time
import machine

np = neopixel.NeoPixel(machine.Pin(2), 1)
np[0] = (0, 255, 0) # RGB  set green to max brightness
np.write()

sensor = HCSR04(trigger_pin=5, echo_pin=4)
distance = 0

time.sleep(1)

while True:
    
    distance = sensor.distance_cm()
    
    if distance < 10:
        np[0] = (255, 0, 0)#ak je predmet bližšie ako 10cm rozsvieti sa červená
        np.write()
    else:
        np[0] = (0, 255, 0)#ak je predmet 10cm a viac od snímača rozsvieti sa zelená
        np.write()
        
    print(distance)#meranie sa vykonáva každú polsekundu a vzdialenosť sa vypisuje cez Tx do konzoly
    time.sleep_ms(500)
        




