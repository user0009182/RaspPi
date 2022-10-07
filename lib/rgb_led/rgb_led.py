from machine import Pin, PWM

#Provides methods to interact with an RGB LED
class RgbLed:
    #Instantiate using the GPIO pin numbers connected to the red, green and blue legs of the LED
    def __init__(self, red_pin_num, green_pin_num, blue_pin_num):
        self._pwm_rgb = []
        self.add_pwm(red_pin_num)
        self.add_pwm(green_pin_num)
        self.add_pwm(blue_pin_num)
        return

    def add_pwm(self, pin_num):
        pin = Pin(pin_num, mode=Pin.OUT)
        pwm = PWM(pin)
        self._pwm_rgb.append(pwm)
        pwm.freq(1000)
        return pwm

    def set_comp(self, i, f):
        self._pwm_rgb[i].duty_u16(int(65335 * f))

    #set color of LED where each component is a float 0-1
    def set_color(self,r,g,b):
        self.set_comp(0, r)
        self.set_comp(1, g)
        self.set_comp(2, b)