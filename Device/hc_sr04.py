#HC-sr04 distance sensor
from machine import Pin
import machine
import utime
import time

#obtains a distance reading in meters
def read_distance_m(pinEcho, pinTrig):
    pinTrig.value(0)
    #set trigger to high for 0.01ms
    pinTrig.value(1)
    time.sleep_us(10)
    pinTrig.value(0)
    #measure time that echo pin is high
    start_time = utime.ticks_us()
    stop_time = utime.ticks_us()
    while pinEcho.value() == 0:
        start_time = utime.ticks_us()
    while pinEcho.value() > 0.5:
        stop_time = utime.ticks_us()
    elapsed_time_us = stop_time - start_time
    #calculate distance to target using speed of sound
    speed_sound = 343300 #mm per second
    #time to hit obstacle in seconds
    hit_time_us = elapsed_time_us / 2
    hit_time_s = hit_time_us / 1000000
    distance_mm = speed_sound * hit_time_s
    distance_m = distance_mm / 1000
    return distance_m

#returns pins that can be passed to read_distance_m
def hc_sr04_setup(numPinEcho, numPinTrig):
    pinEcho = machine.Pin(numPinEcho, machine.Pin.IN)
    pinTrig = machine.Pin(numPinTrig, machine.Pin.OUT)  
    return (pinEcho, pinTrig)
