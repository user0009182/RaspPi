from machine import Pin, PWM

#Provides methods to interact with an SG90 Servo
class Sg90Servo:
    #Instantiate using the number of the GPIO pin connected to the servo data in
    def __init__(self, data_pin_num):
        self._pwm = PWM(Pin(data_pin_num, mode=Pin.OUT))
        self._pwm.freq(50)
        self._min_duty_cycle = 0.018
        self._max_duty_cycle = 0.132
        self._max_degree_range = 200
        return

    def set_angle(self, degrees):
        dc_range = self._max_duty_cycle - self._min_duty_cycle
        dc = self._min_duty_cycle + degrees / self._max_degree_range * dc_range
        value = int(65355 * dc)
        print(value)
        self._pwm.duty_u16(value)
