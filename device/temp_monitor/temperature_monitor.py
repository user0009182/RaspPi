#implements a device that measures temperature and humidty
#provides a single command get_reading that returns a temperature (C) and humidity (%) tuple

from device_client import DeviceClient,Logger
from dht11 import Dht11
import time

buffer_size=100
buffer_index = 0
reading_buffer = [None] * buffer_size

def command_handler(command):
    if command == "get_reading":
        reading = dht11.get_temperature_humidity()
        print(reading)
        if reading == None:
            return "error"
        else:
            (temp,humidity) = reading
            return "{},{}".format(temp, humidity)
    if command == "get_readings":
        text=""
        for i in range(0, buffer_size):
            index=(buffer_index-1-i) % buffer_size
            reading=reading_buffer[index]
            if reading == None:
                text+="N,"
            else:
                if len(reading) == 1:
                    text+="{},".format(reading[0])
                else:
                    text+="{}-{}-{},".format(reading[0],reading[1],reading[2])
        return text
    return None
 
def think_handler():
    global buffer_index
    print("making scheduled reading")
    reading = dht11.get_temperature_humidity()
    if reading == None:
        reading_buffer[buffer_index] = [time.time()]
    else:
        (temp,humidity) = reading
        reading_buffer[buffer_index] = [time.time(),temp,humidity]
    print(reading_buffer[buffer_index])
    buffer_index = buffer_index + 1
    buffer_index = buffer_index % buffer_size
    return 60 #approx number of seconds until next think

def start_temperature_monitor():
    global dht11
    dht11 = Dht11(15)
    client = DeviceClient(Logger())
    client.set_command_handler(command_handler, ['get_reading', 'get_readings'])
    client.set_think_handler(think_handler)
    client.start('192.168.1.64', 21008)

if __name__ == '__main__':
    start_temperature_monitor()