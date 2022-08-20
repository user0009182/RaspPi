from machine import Pin
import utime
import network
import socket
import time
import sys
from hc_sr04 import hc_sr04_setup, read_distance_m
(pinEcho, pinTrig) = hc_sr04_setup(14, 15)
start = time.time()
for i in range(200):
    dist_m = read_distance_m(pinEcho, pinTrig)
    print(dist_m)
    time.sleep(0.1)