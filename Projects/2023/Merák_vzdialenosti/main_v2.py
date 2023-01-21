from hcsr04 import HCSR04
import neopixel
import time
import machine

sensor = HCSR04(trigger_pin=5, echo_pin=4)
np = neopixel.NeoPixel(machine.Pin(2), 1)
np[0] = (0, 0, 255)
np.write()

distance = 0

while True:
    
    distance = sensor.distance_cm()
    
    if distance < 10:
        np[0] = (255, 0, 0)
        np.write()
        
    else:
        np[0] = (0, 255, 0)
        np.write()
        
    time.sleep_ms(10)
