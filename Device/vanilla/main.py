from machine import Pin
import utime
import network
import socket
import time
import sys

def obf(byte_data):
    mask = b'abcdefg'
    lmask = len(mask)
    return bytes(c ^ mask[i % lmask] for i, c in enumerate(byte_data))

def getWifiInfo():
    with open('/w.dat', 'rb') as file:
        data = file.read()
        data=obf(data)
        str=data.decode("ascii")
        (a,b) = str.split(',')
        return (a,b)

def connect_to_wifi():
    wlan = network.WLAN(network.STA_IF)
    wlan.active(False)
    wlan.disconnect()
    wlan.active(True)
    wlan.disconnect()
    (n, p) = getWifiInfo()
    print(n)
    print(p)
    wlan.connect(n, p)
    for i in range(5):
        print("connecting to WIFI")
        led.toggle()
        if wlan.isconnected():
            print("connected to WIFI")
            print('status='+str(wlan.status()))
            print(wlan.ifconfig())
            break
        utime.sleep(2)
    return wlan

def sendStringData(socket, str):
    data = bytes(str, 'ascii')
    print(data)
    length = len(data)
    print(length)
    lengthBytes = length.to_bytes(2, "little")
    socket.write(lengthBytes)
    n = socket.write(data)
    print("sent " + str)

def receiveData(socket):
    dataLength = socket.recv(2)
    length = int.from_bytes(dataLength, "little")
    data = s.recv(length)
    return data

def receiveStringData(socket):
    data = receiveData(socket)
    str=data.decode("ascii")
    print("received " + str)
    return str

def connectToServer(ip, port):
    addr = socket.getaddrinfo(ip, port)[0][-1]
    retryInterval=1
    s = None
    while True:
        try:
            print("connecting to {}:{}".format(ip, port))
            s = socket.socket()
            s.connect(addr)
            print("connected")
            return s
        except:
            print("failed to connect")
            s.close()
        print("retry in {}s".format(retryInterval))
        utime.sleep(retryInterval)
        retryInterval*=2
        if retryInterval > 60:
            retryInterval=60

led = machine.Pin("LED", machine.Pin.OUT)
ledIn = machine.Pin("LED", machine.Pin.IN)
led.off()
wlan = connect_to_wifi()
if wlan.isconnected() == False:
    sys.exit(0)
led.off()
s = connectToServer('192.168.1.64', 21008)
led.on()
sendStringData(s, "device")
strData=receiveStringData(s)
print("received " + strData)
if strData == "server":
    sendStringData(s, "ok")
print(str(ledIn.value()))

for i in range(20):
    strData=receiveStringData(s)
    if strData == "ping":
        sendStringData(s, "pong")
    elif strData == "shutdown":
        break
    elif strData == "get led":
        aa = str(ledIn.value())
        sendStringData(s, aa)
    elif strData == "set led off":
        led.off()
        sendStringData(s, "ok")
    elif strData == "set led on":
        led.on()
        sendStringData(s, "ok")
    else:
        sendStringData(s, "unknown command")

print("shutting down")
utime.sleep(3)
s.close()
utime.sleep(3)
print("disconnecting WIFI")
wlan.disconnect()
led.off()
