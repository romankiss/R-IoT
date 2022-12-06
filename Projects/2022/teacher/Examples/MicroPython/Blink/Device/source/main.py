from machine import *
import time
import neopixel
import random

button1 = Pin(22, Pin.IN, Pin.PULL_UP)
---
Pokus
---
np = neopixel.NeoPixel(machine.Pin(27), 1)
np[0] = (0, 128, 0) # RGB  set to red, 1/2 brightness
np.write()

def doit(t):
    print("Button has been pressed!")
    np[0] = (random.randrange(64),random.randrange(255),random.randrange(255))  
    np.write()   

button1.irq(trigger=Pin.IRQ_FALLING, handler=doit)
