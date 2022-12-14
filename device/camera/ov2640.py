#ArduCAM OV2640 Camera Shield controller
#modified from https://github.com/namato/micropython-ov2640
#https://cdn.shopify.com/s/files/1/0176/3274/files/ArduCAM_Mini_2MP_Camera_Shield_DS.pdf?v=1614860211
#OV2640
#https://www.arducam.com/ov2640/
#https://github.com/kanflo/esparducam/blob/master/arducam/arducam.c
#https://github.com/ArduCAM/Sensor-Regsiter-Decoder/blob/master/OV2640_JPEG_INIT.csv
#another controller in CircuitPython:
#https://github.com/ArduCAM/PICO_SPI_CAM/tree/master/Python
from ov2640_constants import *
from ov2640_lores_constants import *
from ov2640_hires_constants import *
import machine
import time
import ubinascii
import uos
import gc
import sys

class ov2640(object):
    def __init__(self, sclpin=9, sdapin=8, cspin=5, resolution=OV2640_320x240_JPEG):
        self.sdapin=sdapin
        self.sclpin=sclpin
        self.cspin=cspin
        self.standby = False

        self.hspi = machine.SPI(0, baudrate=80000000, polarity=0, phase=0,
        sck=machine.Pin(2),
                  mosi=machine.Pin(3),
                  miso=machine.Pin(4))
        self.i2c = machine.SoftI2C(scl=machine.Pin(sclpin), sda=machine.Pin(sdapin), freq=1000000)
    
        # first init spi assuming the hardware spi is connected
        self.hspi.init(baudrate=2000000)

        # chip select -- active low
        self.cspin = machine.Pin(cspin, machine.Pin.OUT)
        self.cspin.on()

        # init the i2c interface
        addrs = self.i2c.scan()
        print('ov2640_init: devices detected on on i2c:')
        for a in addrs:
            print('0x%x' % a)
        # select register set
        self.i2c.writeto_mem(SENSORADDR, 0xff, b'\x01')
        # initiate system reset
        self.i2c.writeto_mem(SENSORADDR, 0x12, b'\x80')
        # let it come up
        time.sleep_ms(100)
        # jpg init registers
        cam_write_register_set(self.i2c, SENSORADDR, OV2640_JPEG_INIT)
        cam_write_register_set(self.i2c, SENSORADDR, OV2640_YUV422)
        cam_write_register_set(self.i2c, SENSORADDR, OV2640_JPEG)
        # select register set
        self.i2c.writeto_mem(SENSORADDR, 0xff, b'\x01')
        self.i2c.writeto_mem(SENSORADDR, 0x15, b'\x00')
   
        # select jpg resolution
        cam_write_register_set(self.i2c, SENSORADDR, resolution)
    
        # test the SPI bus
        cam_spi_write(b'\x00', b'\x55', self.hspi, self.cspin)
        res = cam_spi_read(b'\x00', self.hspi, self.cspin)
        #print("ov2640 init:  register test return bytes %s" % ubinascii.hexlify(res))
        if (res == 0x55): #b'\x55'):
            print("ov2640_init: register test successful")
        else:
            print("ov2640_init: register test failed!")
            sys.exit(0)
    
        # register set select
        self.i2c.writeto_mem(SENSORADDR, 0xff, b'\x01')
        # check the camera type
        parta = self.i2c.readfrom_mem(SENSORADDR, 0x0a, 1)
        partb = self.i2c.readfrom_mem(SENSORADDR, 0x0b, 1)
        if ((parta != b'\x26') or (partb != b'\x42')):
            print("ov2640_init: device type does not appear to be ov2640, bytes: %s/%s" % \
                    (ubinascii.hexlify(parta), ubinascii.hexlify(partb)))
        else:
            print("ov2640_init: device type looks correct, bytes: %s/%s" % \
                    (ubinascii.hexlify(parta), ubinascii.hexlify(partb)))

    def capture_image(self):
        # bit 0 - clear FIFO write done flag
        cam_spi_write(b'\x04', b'\x01', self.hspi, self.cspin)
    
        # bit 1 - start capture then read status
        cam_spi_write(b'\x04', b'\x02', self.hspi, self.cspin)
        time.sleep_ms(10)
    
        # read status
        res = cam_spi_read(b'\x41', self.hspi, self.cspin)
        cnt = 0
        #if (res == b'\x00'):
        #    print("initiate capture may have failed, return byte: %s" % ubinascii.hexlify(res))

        # read the image from the camera fifo
        while True:
            res = cam_spi_read(b'\x41', self.hspi, self.cspin)
            mask = 0x08 #b'\x08'
            if (res & mask):
                break
            #print("continuing, res register %s" % ubinascii.hexlify(res))
            time.sleep_ms(10)
            cnt += 1
        #print("slept in loop %d times" % cnt)
   
        # read the fifo size
        b1 = cam_spi_read(b'\x44', self.hspi, self.cspin)
        b2 = cam_spi_read(b'\x43', self.hspi, self.cspin)
        b3 = cam_spi_read(b'\x42', self.hspi, self.cspin)
        val = b1 << 16 | b2 << 8 | b3
        print("ov2640_capture: %d bytes in fifo" % val)
        gc.collect()
    
        bytebuf = bytearray(2)
        l = 0
        #todo dynamic size
        image_data = bytearray(20000)
        while (bytebuf[0] != 0xd9) or (bytebuf[1] != 0xff):
            bytebuf[1] = bytebuf[0]   
            bytebuf[0] = cam_spi_read(b'\x3d', self.hspi, self.cspin)
            image_data[l] = bytebuf[0]
            l += 1
        print("read %d bytes from fifo, camera said %d were available" % (l, val))
        return (image_data, l)

    # XXX these need some work
    def standby(self):
        # register set select
        self.i2c.writeto_mem(SENSORADDR, 0xff, b'\x01')
        # standby mode
        self.i2c.writeto_mem(SENSORADDR, 0x09, b'\x10')
        self.standby = True

    def wake(self):
        # register set select
        self.i2c.writeto_mem(SENSORADDR, 0xff, b'\x01')
        # standby mode
        self.i2c.writeto_mem(SENSORADDR, 0x09, b'\x00')
        self.standby = False

def cam_write_register_set(i, addr, set):
    for el in set:
        raddr = el[0]
        val = bytes([el[1]])
        if (raddr == 0xff and val == b'\xff'):
            return
        #print("writing byte %s to addr %x register addr %x" % \
        #   (ubinascii.hexlify(val), addr, raddr))
        i.writeto_mem(addr, raddr, val)

def appendbuf(fn, picbuf, howmany):
    try:
        f = open(fn, 'ab')
        c = 1
        for by in picbuf:
            if (c > howmany):
                break
            c += 1
            f.write(bytes([by[0]]))
        f.close()
    except OSError:
        print("error writing file")
    print("write %d bytes from buffer" % howmany)

def cam_spi_write(address, value, hspi, cspin):
    cspin.off()
    modebit = b'\x80'
    d = bytes([address[0] | modebit[0], value[0]])
    #print("bytes %s" % ubinascii.hexlify(d))
    #print (ubd.hex())
    hspi.write(d)
    cspin.on()

wbuf2 = bytearray(1)
def cam_spi_read(address, hspi, cspin):
    cspin.off()
    maskbits = b'\x7f'
    wbuf2[0] = address[0] & maskbits[0]
    hspi.write(wbuf2)
    hspi.readinto(wbuf2)
    cspin.on()
    return wbuf2[0]
