#implements a device that measures temperature and humidty
#provides a single command get_reading that returns a temperature (C) and humidity (%) tuple

import device_client
from dht11 import Dht11
from machine import Pin
from neopixel import NeoPixel

def command_handler(command):
    if command == "get_reading":
        reading = dht11.get_temperature_humidity()
        print(reading)
        if reading == None:
            client.sendStringData("error")
        else:
            (temp,humidity) = reading
            client.sendStringData("{},{}".format(temp, humidity))
        return True
    return False

dht11 = Dht11(15)
client = device_client.Client()
client.command_handler = command_handler
client.extra_command_list = ['get_reading']
client.start('192.168.1.64', 21008)
