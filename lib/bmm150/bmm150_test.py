from bmm150 import Bmm150Magnetometer
import time
import math
bmm150 = Bmm150Magnetometer(0, 1)
print(bmm150.is_connected())
bmm150.activate()
print("chip_id={}".format(bmm150.get_chip_id()))

for i in range(50):
    m=bmm150.read_xyz()
    print(m)
    time.sleep(0.5)
    print(math.sqrt(m[0]*m[0]+m[1]*m[1]+m[2]*m[2]))
