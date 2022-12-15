from machine import Pin, I2C
import math

class Bmm150Magnetometer:
    def __init__(self, sda_pin_num, scl_pin_num):
        self._i2c = I2C(0, sda=Pin(sda_pin_num), scl=Pin(scl_pin_num), freq=400000)
        self._i2c_addr=19
        return

    def is_connected(self):
        found_i2c_addrs = self._i2c.scan()
        print(found_i2c_addrs)
        b1 = bytearray(1)
        if not self._i2c_addr in found_i2c_addrs:
            print("i2c address {} not found".format(self._i2c_addr))
            return False
        return True

    def get_chip_id(self):
        b1 = bytearray(1)
        self._i2c.readfrom_mem_into(self._i2c_addr, 0x40, b1)
        return b1[0]

    def get_power_mode(self):
        b1 = bytearray(1)
        self._i2c.readfrom_mem_into(self._i2c_addr, 0x40, b1)

    def is_suspended(self):
        b1 = bytearray(1)
        self._i2c.readfrom_mem_into(self._i2c_addr, 0x4B, b1)
        return b1[0] & 1 == 0

    def activate(self):
        if self.is_suspended():
            #set power control bit to enter sleep mode
            self._i2c.writeto_mem(self._i2c_addr, 0x4B, b'\x01')
            #set operation mode bits to normal operation active mode
        self._i2c.writeto_mem(self._i2c_addr, 0x4C, b'\x00')
    
    def comp2ToSignedShort(self, comp2_value, num_bits):
        v = comp2_value
        #print("v={}".format(bin(v)))
        if v >= (1 << (num_bits-1)):
            #xor with FF.. to invert bits
            v = -((v ^ (1<<num_bits)-1) + 1)
        return v

    def read_xyz(self):
        b2 = bytearray(6)
        self._i2c.readfrom_mem_into(self._i2c_addr, 0x42, b2)
        #x is read as a 13 bit value held in bytes 1 & 2
        #byte 1 is the most significant 8 bits. The top 5 bits of byte 2 are the least significant 5 bits
        x = (b2[1] << 5) + (b2[0] >> 3)
        #same for y
        y = (b2[3] << 5) + (b2[2] >> 3)
        #z is a 15 bit value
        z = (b2[5] << 7) + (b2[4] >> 1)

        x = self.comp2ToSignedShort(x, 13)
        y = self.comp2ToSignedShort(y, 13)
        z = self.comp2ToSignedShort(z, 15)
        length=x*x+y*y+z*z
        if length == 0:
            return None
        length=math.sqrt(length)
        return [x/length,y/length,z/length]