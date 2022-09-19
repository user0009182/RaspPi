from neopixel import NeoPixel
from machine import Pin
import random
import time

pin = Pin(0, Pin.OUT)
num_leds_on_ring=16
np = NeoPixel(pin, num_leds_on_ring)

led=0
#cycle round ring four times
#+ 1 so that it ends on the starting LED
iterations = num_leds_on_ring * 4 + 1
for i in range(iterations):
    #turn off the previously lit LED
    led_last=led
    np[led_last] = (0, 0, 0)

    #calculate the next LED to light
    led = i % 16

    #set the LED to a random r,g,b color
    r=random.randint(0, 50)
    g=random.randint(0, 50)
    b=random.randint(0, 50)
    np[led] = (r, g, b)
    np.write()
    #wait 0.1 seconds
    time.sleep(0.1)

np[led] = (0, 0, 0)
np.write()