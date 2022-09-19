from neopixel import NeoPixel
from machine import Pin
import random

pin = Pin(0, Pin.OUT)
np = NeoPixel(pin, 16)

np[0] = (1, 1, 1)
r=random.randint(0, 10)
g=random.randint(0, 10)
b=random.randint(0, 10)
np[0] = (r, g, b)
np.write()
