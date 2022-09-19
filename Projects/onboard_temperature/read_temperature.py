import machine
import utime

sensor_temp = machine.ADC(4)

for i in range(10):
    reading = sensor_temp.read_u16()
    print(reading)
    v = reading * 3.3 / 65535
    print(v)
    temperature = 27 - (v - 0.706) / 0.001721    
    print("temp: ",temperature)
    utime.sleep(0.2)