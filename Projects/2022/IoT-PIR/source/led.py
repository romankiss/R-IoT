from machine import *
import time
import neopixel
import random
import machine

button1 = Pin(22, Pin.IN, Pin.PULL_UP)

np = neopixel.NeoPixel(machine.Pin(27), 1)

def dot(i):
    j = 0
    while j != i:
        np[0] = (0, 0, 128)
        np.write()
        time.sleep(0.1)
        np[0] = (0, 0, 0)
        np.write()
        time.sleep(0.33)
        j += 1
    
def dash(i):
    j = 0
    while j != i:
        np[0] = (0, 0, 128)
        np.write()
        time.sleep(0.5)
        np[0] = (0, 0, 0)
        np.write()
        time.sleep(0.33)
        j += 1



dot(4)
time.sleep(1)
dot(1)
time.sleep(1)
dot(1)
dash(3)
time.sleep(1)
dot(1)
dash(3)
time.sleep(1)
dash(3)
time.sleep(1)
