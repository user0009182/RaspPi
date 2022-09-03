import device_client
import hc_sr04

def command_handler(command):
    if command == "get_distance":
        distance = distance_sensor.read_distance_m(1000)
        client.sendStringData(str(distance))
        return True
    return False

distance_sensor = hc_sr04.Hc_sr04(15, 14)
client = device_client.Client()
client.command_handler = command_handler
client.extra_command_list = ['get_distance', 'cmd2']
client.start('192.168.1.64', 21008)
