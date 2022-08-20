import device_client
import hc_sr04

def command_handler(command):
    if command == "get_dist":
        distance = hc_sr04.read_distance_m(pinEcho, pinTrig)
        client.sendStringData(str(distance))
        return True
    return False

(pinEcho, pinTrig) = hc_sr04.hc_sr04_setup(14, 15)

client = device_client.Client()
client.command_handler = command_handler
client.start('192.168.1.64', 21008)
