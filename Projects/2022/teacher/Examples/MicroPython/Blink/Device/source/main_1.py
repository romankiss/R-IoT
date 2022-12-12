import machine
from machine import *
import neopixel
import random
import time


# AtomLite 
button_Pin = 39	
neo_Pin = 27	    # for HAT pin 25	

"""
# RPi Pico
button_Pin = xx	
neo_Pin = yy	
"""

timer = Timer(1)
button1 = Pin(button_Pin, Pin.IN, Pin.PULL_UP)
np = neopixel.NeoPixel(machine.Pin(neo_Pin), 1)
np[0] = (0, 128, 0) # RGB  set green to 1/2 brightness
np.write()

def doit(t):
    print("Button has been pressed")
    np[0] = (random.randrange(100),random.randrange(100),random.randrange(100))
    np.write()   

button1.irq(trigger=Pin.IRQ_FALLING, handler=doit)
timer.init(freq=1.5, mode=Timer.PERIODIC, callback=lambda t:doit(t))
