#implements a device that takes a photo with an attached camera
from device_client import DeviceClient,Logger
from sg90_servo import Sg90Servo
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
    if command.startswith('turn'):
        parts=command.split(' ')
        yaw=parts[1]
        pitch=parts[2]
        turn(int(yaw), int(pitch))
        return "ok"
    return None

def turn(target_yaw, target_pitch):
    global current_yaw
    global current_pitch
    for i in range(0, 100):
        print("{} {}".format(current_yaw ,current_pitch))
        xd = target_yaw - current_yaw
        yd = target_pitch - current_pitch
        xd = max(min(xd, 1), -1)
        yd = max(min(yd, 1), -1)
        if xd == 0 and yd == 0:
            break
        current_pitch += yd
        current_yaw += xd
        sg90_yaw.set_angle(current_yaw)
        sg90_pitch.set_angle(current_pitch)
        time.sleep(0.02)
 
def start_device():
    global camera
    global sg90_yaw
    global sg90_pitch
    global current_yaw
    global current_pitch
    current_yaw=60
    current_pitch=60
    sg90_yaw = Sg90Servo(27)
    sg90_pitch = Sg90Servo(26)
    turn(60, 60)

    camera = ov2640.ov2640(resolution=ov2640.OV2640_320x240_JPEG, sclpin=21, sdapin=20, cspin=17, sckpin=18, mosipin=19, misopin=16)
    client = DeviceClient("camera", Logger())
    client.set_command_handler(command_handler, ['get_image', 'turn'])
    #client.set_think_handler(think_handler)
    client.start('192.168.1.64', 21008)

if __name__ == '__main__':
    start_device()

