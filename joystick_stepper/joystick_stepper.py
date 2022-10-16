from joystick import Joystick
from stepper_28byj48 import Stepper28byj48
import time

stepper = Stepper28byj48([15,14,13,12])
joystick = Joystick(0, 1)

end = time.time() + 50
while time.time() < end:
    xy = joystick.readXY()
    if xy[1] > 4:
        stepper.step_right()
    elif xy[1] < -3:
        stepper.step_left()
    time.sleep(0.001)