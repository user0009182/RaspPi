#Sets up a device that provides a command to obtain the current time
import time
from device_client import DeviceClient, Logger

#define a command handler. This will be invoked when the device receives a command from the hub
#the handler is passed a single string parameter containing the command
def command_handler(command):
    if command == "time":
        #returning a string will result in this string being returned in the response to the hub
        return str(time.time())
    #returning None indicates that the command is unknown and wasn't processed
    return None

#a think handler is optional and allows the device to perform periodic processing independently of command handling
def think_handler():
    print("think")
    #return the number of seconds for the think handler to be called again
    return 30

logger = Logger()
device = DeviceClient(logger)
#The second parameter is an array of strings, the name of supported commands
#The device can be queried for which commands it supports. The response will include this list.
device.set_command_handler(command_handler, ["time"])
device.set_think_handler(think_handler)
#connects the device to a hub running on the given address and port
device.start('192.168.1.64', 21008)
