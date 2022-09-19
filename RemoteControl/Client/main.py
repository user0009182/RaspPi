import device_client
import hc_sr04
from machine import Pin
from neopixel import NeoPixel

def command_handler(command):
    if command == "get_distance":
        distance = distance_sensor.read_distance_m(1000)
        client.sendStringData(str(distance))
        return True
    if command.startswith("set_pixel"):
        parts=command.split(' ')
        pixel_num=int(parts[1])
        pixel_r=int(parts[2])
        pixel_g=int(parts[3])
        pixel_b=int(parts[4])
        neopixel[pixel_num] = (pixel_r, pixel_g, pixel_b)
        neopixel.write()
        client.sendStringData("OK")
        return True
    return False

distance_sensor = hc_sr04.Hc_sr04(15, 14)
neopixel = NeoPixel(Pin(0, Pin.OUT), 16)

client = device_client.Client()
client.command_handler = command_handler
client.extra_command_list = ['get_distance', 'set_pixel']
client.start('192.168.1.64', 21008)
