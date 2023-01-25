import machine, neopixel, time

np = neopixel.NeoPixel(machine.Pin(3), 1)
sleep_ms = 200
brightness = 0 #zacinajuca sytost
fade = 5 #milisekundy

for pocet in range (2):
    np[0] = (255, 0, 0) 
    np.write()
    time.sleep_ms(sleep_ms)
    np[0] = (0, 255, 0)
    np.write()
    time.sleep_ms(sleep_ms)
    np[0] = (0, 0, 255)
    np.write()
    time.sleep_ms(sleep_ms)

np[0] = (0,0,0)
np.write()
time.sleep(0.5)

for i in range(255 - brightness):
    brightness += 1
    np[0] = (0+brightness,0+brightness,0+brightness)
    np.write()
    time.sleep_ms(fade)

brightness = 0
np[0] = (0,0,0)
np.write()
