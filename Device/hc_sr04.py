from machine import Pin
import machine
import utime
import time

#HC-SR04 distance sensor
class Hc_sr04:
    def __init__(self, pin_trigger_num, pin_echo_num):
        self.pin_trigger = machine.Pin(pin_trigger_num, machine.Pin.OUT)  
        self.pin_echo = machine.Pin(pin_echo_num, machine.Pin.IN)

    def wait_for_echo_value(self, value, timeout_ms):
        i=0
        current_time_us = utime.ticks_us()
        end_time_us = current_time_us + timeout_ms * 1000
        while current_time_us <= end_time_us:
            if value == 0:
                if self.pin_echo.value() < 0.5:
                    return current_time_us
            else:
                if self.pin_echo.value() != 0:
                    return current_time_us
            current_time_us = utime.ticks_us()
        return 0

    #obtains a distance reading in meters
    #returns 100 on timeout
    def read_distance_m(self, timeout_ms):
        self.pin_trigger.value(0)
        #set trigger to high for 0.01ms
        self.pin_trigger.value(1)
        time.sleep_us(10)
        self.pin_trigger.value(0)
        #measure time that echo pin is high
        start_time = self.wait_for_echo_value(1, timeout_ms)
        if start_time == 0:
            return 100 #failure
        stop_time = self.wait_for_echo_value(0, timeout_ms)
        if stop_time == 0:
            return 100 #failure
        elapsed_time_us = stop_time - start_time
        #calculate distance to target using speed of sound
        speed_sound = 343300 #mm per second
        #time to hit obstacle in seconds
        hit_time_us = elapsed_time_us / 2
        hit_time_s = hit_time_us / 1000000
        distance_mm = speed_sound * hit_time_s
        distance_m = distance_mm / 1000
        return distance_m