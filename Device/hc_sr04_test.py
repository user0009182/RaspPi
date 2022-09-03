from machine import Pin
import utime
import network
import socket
import time
import sys
import hc_sr04

distance_sensor1 = hc_sr04.Hc_sr04(15, 14)
start = time.time()
for i in range(200):
    dist_m = distance_sensor1.read_distance_m(2000)
    print(str(i) + " " + str(dist_m) + "m")
    time.sleep(0.2)