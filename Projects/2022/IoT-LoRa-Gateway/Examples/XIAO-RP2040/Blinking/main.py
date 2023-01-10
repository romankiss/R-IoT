# based on https://wiki.seeedstudio.com/XIAO-RP2040-with-MicroPython/
from machine import Pin, Timer
import machine, time, neopixel, random

timer = Timer()

#enable NeoPixel
switch = Pin(11, Pin.OUT) 
switch.value(1)

button1 = Pin(26, Pin.IN, Pin.PULL_UP) #physical: pin#0, programmatically: pin P26
ledB = Pin(25, Pin.OUT, 0) #user Led

# neopixel
np = neopixel.NeoPixel(machine.Pin(12), 1) #built-in Neo
np[0] = (0, 255, 0) 
np.write()

#callback function
def doit(t):
    np[0] = (random.randrange(100),random.randrange(100),random.randrange(100))
    np.write()
    ledB.value(not ledB.value())
 
#callback function
def switcher(t):
    print("Button has been pressed")
    switch.value(not switch.value())

#event handlers
timer.init(freq=1, mode=Timer.PERIODIC, callback=doit)  #change RGB colors at each seconds
button1.irq(trigger=Pin.IRQ_FALLING, handler=switcher)  #enable/disable NeoPixel
