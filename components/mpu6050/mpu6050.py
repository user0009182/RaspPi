from imu import MPU6050
from machine import Pin, I2C

class Mpu6050:
    def __init__(self, sda_pin_num, scl_pin_num):
        self._i2c = I2C(0, sda=Pin(sda_pin_num), scl=Pin(scl_pin_num), freq=400000)
        self._i2c_addr=0x68
        return

    def configure(self):
        found_i2c_addrs = self._i2c.scan()
        #exit if the mpu6050 is not found in the scan results
        if self._i2c_addr not in found_i2c_addrs:
            print("mpu6050 not found")
            return False
        #read from who_am_i register, should return 0x68
        b1 = bytearray(1)
        self._i2c.readfrom_mem_into(self._i2c_addr, 0x75, b1)
        if b1[0] != 0x68:
            print("unexpected id from mpu6050")
            return False
        
        #configure power mode and clock source
        #PLL with X axis gyroscope reference
        b1[0] = 1
        self._i2c.writeto_mem(self._i2c_addr, 0x6B, b1)
        #set accelerometer to highest sensitivity
        b1[0] = 0
        self._i2c.writeto_mem(self._i2c_addr, 0x1C, b1)
        #set gyroscope to highest sensitivity
        self._i2c.writeto_mem(self._i2c_addr, 0x1B, b1)
        return True

    def toSignedShort(self, b1, b2):
        v = (b1 << 8) + b2
        if v >= 0x8000:
            v = -((v ^ 0xFFFF) + 1)
        return v

    def read_address6(self, addr):
        #read address 0x3B
        data = bytearray(6)
        self._i2c.readfrom_mem_into(self._i2c_addr, addr, data)
        x = self.toSignedShort(data[0], data[1])
        y = self.toSignedShort(data[2], data[3])
        z = self.toSignedShort(data[4], data[5])
        acc_sensitivity = 16384
        x /= acc_sensitivity
        y /= acc_sensitivity
        z /= acc_sensitivity
        return (x,y,z)

    def get_gyroscope_value(self):
        return self.read_address6(0x43)

    def get_accelerometer_value(self):
        return self.read_address6(0x3B) 
