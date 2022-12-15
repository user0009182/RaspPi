#implements a device that takes a photo with an attached camera
from device_client import DeviceClient,Logger
import ov2640
import time
import ubinascii

buffer_size=100
buffer_index = 0
reading_buffer = [None] * buffer_size

def command_handler(command):
    if command == "get_image":
        (image_data, image_size) = camera.capture_image()
        print("captured image is {} bytes".format(image_size))
        base64_bytes=ubinascii.b2a_base64(image_data[0:image_size])
        base64_string=base64_bytes.decode('ascii')
        return base64_string
    return None
 
def start_device():
    global camera
    camera = ov2640.ov2640(resolution=ov2640.OV2640_320x240_JPEG)
    client = DeviceClient("camera", Logger())
    client.set_command_handler(command_handler, ['get_image'])
    #client.set_think_handler(think_handler)
    client.start('192.168.1.64', 21008)

if __name__ == '__main__':
    start_device()

