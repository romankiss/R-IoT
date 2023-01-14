import machine, neopixel, time

np = neopixel.NeoPixel(machine.Pin(3), 1)

sleep = 0
r = 255
g = 0
b = 0

np[0] = (r, g, b)
np.write()

for i in range(5):

    if r == 255 and g == 255 and b == 0:
        while r != 0:
            r -= 1
            np[0]=(r,g,b)
            np.write()
            time.sleep_ms(sleep)
    
    if r == 0 and g == 255 and b == 0:
        while b != 255:
            b += 1
            np[0]=(r,g,b)
            np.write()
            time.sleep_ms(sleep)
            
    
    if r == 0 and g == 255 and b == 255:
        while g != 0:
            g -= 1
            np[0]=(r,g,b)
            np.write()
            time.sleep_ms(sleep)
            
    if r == 0 and g == 0 and b == 255:
        while r != 255:
            r += 1
            np[0]=(r,g,b)
            np.write()
            time.sleep_ms(sleep)
            
    if r == 255 and b == 255 and g == 0:
        while b != 0:
            b -= 1
            np[0]=(r,g,b)
            np.write()
            time.sleep_ms(sleep)
            
    if r == 255 and g == 0 and b == 0:
        while g != 255:
            g += 1
            np[0]=(r,g,b)
            np.write()
            time.sleep_ms(sleep)
            
r = 0
g = 0
b = 0

np[0] = (r, g, b)
np.write()
            