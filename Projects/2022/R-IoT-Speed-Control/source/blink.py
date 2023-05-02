import math
import machine
import time
# Define the period and amplitude of the sine wave
period = 2 * math.pi  # in radians
amplitude = 1.0

# Set up the LED pin
led_pin = machine.Pin("LED", machine.Pin.OUT)

# Loop forever
while True:
    for i in range(100):
        # Calculate the sine value for the current step
        sine_value = amplitude * math.sin(period * i / 140)

        # Output the sine value to the REPL console
        print("{:.3f}".format(sine_value))

        led_pin.toggle()  # Turn the LED on
        time.sleep(sine_value)  # Wait for 0.5 second

    