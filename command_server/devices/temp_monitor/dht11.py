#class for reading temperature and humidity from DHT11 sensor
#https://components101.com/sites/default/files/component_datasheet/DHT11-Temperature-Sensor.pdf
from machine import Pin
import utime

class Dht11:
    def __init__(self, data_pin_num):
        self._pin = Pin(data_pin_num, Pin.OUT, Pin.PULL_DOWN)

    def get_temperature_humidity(self):
        #send begin signal to sensor
        self._pin.init(Pin.OUT, Pin.PULL_DOWN)
        self._pin.value(1)
        utime.sleep_ms(500)
        self._pin.value(0)
        utime.sleep_ms(18)
        self._pin.init(Pin.IN, Pin.PULL_UP)

        data = self.__read_data()
        if len(data) < 80:
            return None
        data = data[len(data)-80:]
        bits = self.__get_bits_from_intervals(data)
        if len(bits) != 40:
            print("error: expected 40 bits, received {}".format(len(bits)))
            return None

        bytes = self.__get_bytes_from_bits(bits)
        if not self.__verify_checksum(bytes):
            print("invalid checksum")
            return None
        humidity=bytes[0]
        temp=bytes[2]
        return (temp, humidity)

    @micropython.native
    def __read_data(self):
        last=utime.ticks_us()
        next_value=0
        intervals = []
        for r in range(120):
            for i in range(150):
                if self._pin.value() == next_value:
                    next_value = 1 - next_value
                    cur=utime.ticks_us()
                    delta=cur-last
                    last=cur
                    intervals.append(delta)
                    break
            if i == 99:
                break
        return intervals

    def __get_bits_from_intervals(self, data):
        bits=[]
        for i in range(0, 80, 2):
            if data[i] < 35:
                bits.append(0)
            elif data[i] > 65:
                bits.append(1)
        return bits
    
    def __get_bytes_from_bits(self, bits):
        bytes=[]
        for i in range(5):
            bytes.append(0)
            for j in range(8):
                bit=bits[i*8+(7-j)]
                bytes[i] += (bit << j)
        return bytes

    def __verify_checksum(self, bytes):
        total = (bytes[0]+bytes[1]+bytes[2]+bytes[3])%256
        parity=bytes[4]
        return total == parity