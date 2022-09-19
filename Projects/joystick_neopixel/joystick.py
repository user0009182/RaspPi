#class for reading joystick with separate 3.3v analog x,y output
import machine
import utime
import math

class Joystick:
    def __init__(self, adc_x_pin, adc_y_pin):
        self.x_pin = machine.ADC(0)
        self.y_pin = machine.ADC(1)

    def get_coord_value(self, pin):
        reading = pin.read_u16()
        value = reading - 65535/2
        value /= 65535
        value *= 10
        value = round(value)
        return value

    #returns x and y position, where positive y is up
    def readXY(self):
        x = self.get_coord_value(self.x_pin)
        y = -self.get_coord_value(self.y_pin)
        return (x,y)

    #returns the angle and magnitude of the joystick
    def readAngle(self):
        (x,y) = self.readXY()
        length=math.sqrt(x*x+y*y)
        if length <= 0.001:
            return (0,0)
        n_x = x / length
        n_y = y / length
        angle = math.acos(n_y)
        if n_x < 0:
            angle=math.pi*2-angle
        #todo magnitude is not accurate
        return (angle, length/10)