#Sets up a device that provides a command to obtain the current time
import time
from device_client import DeviceClient, Logger

def command_handler(command):
    if command == "time":
        return str(time.time())
    return None

def think_handler():
    print("think")
    return 30

logger = Logger()
device = DeviceClient(logger)
device.set_command_handler(command_handler, ["time"])
device.set_think_handler(think_handler)
device.start('192.168.1.64', 21008)
