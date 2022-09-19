import machine
import utime
import math

def get_coord_value(pin):
    reading = pin.read_u16()
    value = reading - 65535/2
    value /= 65535
    value *= 10
    value = round(value)
    return value

joystick_x_pin = machine.ADC(0)
joystick_y_pin = machine.ADC(1)

for i in range(200):
    x = get_coord_value(joystick_x_pin)
    y = get_coord_value(joystick_y_pin)
    print(r"{},{}".format(x,y))
    utime.sleep(0.1)
