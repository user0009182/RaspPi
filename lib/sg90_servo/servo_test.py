from machine import Pin, PWM
import sys
from sg90_servo import Sg90Servo
import time
#out = PWM(Pin(3)) #GPIO4
#out.freq(50)
i=1200

sg90_servo1 = Sg90Servo(3)
sg90_servo1.set_angle(45)

sg90_servo1 = Sg90Servo(15)
sg90_servo1.set_angle(45)
#20ms

#0.5ms   2.4ms
sys.exit(0)
#min duty cycle = 0.132 
#max duty cycle = 0.018
c= 0.13
a=int(65355 * c)
print(a)
out.duty_u16(a)
#start=1200
#end=8600
#for i in range(start, end, 100):
#    print(i)
#    out.duty_u16(i)
 #   time.sleep(1)

