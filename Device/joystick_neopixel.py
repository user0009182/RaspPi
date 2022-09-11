#drive neopixel ring using joystick
#the angle of the joystick is used to determine the LED to light up
#pushing the joystick further will brighten the LED
from neopixel import NeoPixel
from joystick import Joystick
from machine import Pin
import random
import utime
import math

joystick = Joystick(0,1)
pin = Pin(0, Pin.OUT)
np = NeoPixel(pin, 16)
led_prev=0
for i in range(200):
    (angle, magnitude) = joystick.readAngle()
    np[led_prev] = (0, 0, 0)
    if magnitude > 0:
        led_cur = 15 - int(math.floor(angle / (math.pi*2) * 16))
        brightness=int(magnitude * 50)
        np[led_cur] = (0,int(brightness/2),brightness)
        led_prev = led_cur
    np.write()
    utime.sleep(0.1)



