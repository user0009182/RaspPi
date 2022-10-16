from machine import Pin

#Provides methods to interact with a 28BYJ-48 stepper motor controlled by a ULN2003 driver
class Stepper28byj48:
    #Instantiate using the GPIO pin numbers connected to inputs 1 through 4
    def __init__(self, gpio_pin_nums):
        self._pins = []
        self._currentStep = 0
        for i in range(4):
            self.add_pin(gpio_pin_nums[i])
        self._sequence = [[0,0,0,1], 
                          [0,0,1,1],
                          [0,0,1,0],
                          [0,1,1,0],
                          [0,1,0,0],
                          [1,1,0,0],
                          [1,0,0,0],
                          [1,0,0,1]]
        return

    def add_pin(self, pin_num):
        pin = Pin(pin_num, mode=Pin.OUT)
        self._pins.append(pin)
        return pin

    def set_step(self, i):
        if i < 0:
            i+=8
        elif i > 7:
            i = i % 8
        self._currentStep = i
        self.output()
    def output(self):
        for i in range(4):
            self._pins[3-i].value(self._sequence[self._currentStep][i])

    def step_right(self):
        self.set_step(self._currentStep + 1)
    def step_left(self):
        self.set_step(self._currentStep - 1)