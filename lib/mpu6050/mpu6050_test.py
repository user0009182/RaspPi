import mpu6050
import time

mpu = mpu6050.Mpu6050(0,1)
mpu.configure()

for i in range(100):
    (x,y,z) = mpu.get_gyroscope_value()
    print("{},{},{}".format(x,y,z))
    time.sleep(0.1)