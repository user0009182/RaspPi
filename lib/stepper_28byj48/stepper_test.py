from stepper_28byj48 import Stepper28byj48
import time
stepper = Stepper28byj48([15,14,13,12])

#stepper.set_step(7)

end = time.time() + 2
while time.time() < end:
    stepper.step_right()
    time.sleep(0.001)

end = time.time() + 2
while time.time() < end:
    stepper.step_left()
    time.sleep(0.001)