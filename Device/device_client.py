import machine
import utime
import network
import socket
import time
import sys

#Device client class
class Client:
    def __init__(self):
        self.led_out = machine.Pin("LED", machine.Pin.OUT)
        self.led_in = machine.Pin("LED", machine.Pin.IN)
        self.__socket = None
        self.command_handler = None
        self.extra_command_list = None

    def obf(self, byte_data):
        mask = b'abcdefg'
        lmask = len(mask)
        return bytes(c ^ mask[i % lmask] for i, c in enumerate(byte_data))

    def get_wifi_info(self):
        with open('/w.dat', 'rb') as file:
            data = file.read()
            data=self.obf(data)
            str=data.decode("ascii")
            (a,b) = str.split(',')
            return (a,b)

    def connect_to_wifi(self):
        wlan = network.WLAN(network.STA_IF)
        wlan.active(False)
        wlan.disconnect()
        wlan.active(True)
        wlan.disconnect()
        #obtain wifi connection info
        (n, p) = self.get_wifi_info()
        #print("{} {}".format(n, p))
        wlan.connect(n, p)
        for i in range(5):
            print("connecting to WIFI")
            self.led_out.toggle()
            if wlan.isconnected():
                print("connected to WIFI")
                print('status='+str(wlan.status()))
                print(wlan.ifconfig())
                break
            utime.sleep(2)
        return wlan

    def sendStringData(self, str):
        data = bytes(str, 'ascii')
        print(data)
        length = len(data)
        print(length)
        lengthBytes = length.to_bytes(2, "little")
        self.__socket.write(lengthBytes)
        n = self.__socket.write(data)
        print("sent " + str)

    def receiveData(self):
        dataLength = self.__socket.recv(2)
        length = int.from_bytes(dataLength, "little")
        data = self.__socket.recv(length)
        return data

    def receiveStringData(self):
        data = self.receiveData()
        str=data.decode("ascii")
        print("received " + str)
        return str

    def connectToServer(self, ip, port):
        addr = socket.getaddrinfo(ip, port)[0][-1]
        retryInterval=1
        self.__socket = None
        while True:
            try:
                print("connecting to {}:{}".format(ip, port))
                self.__socket = socket.socket()
                self.__socket.connect(addr)
                print("connected")
                return self.__socket
            except:
                print("failed to connect")
                self.__socket.close()
            print("retry in {}s".format(retryInterval))
            utime.sleep(retryInterval)
            retryInterval*=2
            if retryInterval > 60:
                retryInterval=60

    def connect_handshake(self):
        self.sendStringData("device")
        strData=self.receiveStringData()
        print("received " + strData)
        if strData == "server":
            self.sendStringData("ok")
        print(str(self.led_in.value()))

    def process_command(self, command):
        if not self.command_handler == None:
            return self.command_handler(command)
        return False
    def command_loop(self):
        while True:
            command=self.receiveStringData()
            if command == "ping":
                self.sendStringData("pong")
            elif command == "shutdown":
                break
            elif command == "?":
                command_list = "ping,shutdown,set led off,set led on"
                if not self.extra_command_list == None:
                    str=",".join(self.extra_command_list)
                    if len(str) > 0:
                        command_list += "," + str
                self.sendStringData(command_list)
            elif command == "get led":
                aa = str(self.led_in.value())
                self.sendStringData(aa)
            elif command == "set led off":
                self.led_out.off()
                self.sendStringData("ok")
            elif command == "set led on":
                self.led_out.on()
                self.sendStringData("ok")
            else:
                processed = self.process_command(command)
                if not processed:
                    self.sendStringData("unknown command {}".format(command))
                

    def start(self, server_ip_address, server_port):
        self.led_out.off()
        wlan = self.connect_to_wifi()
        if wlan.isconnected() == False:
            sys.exit(0)
        self.led_out.off()
        self.connectToServer(server_ip_address, server_port)
        self.led_out.on()
        self.connect_handshake()
        self.command_loop()
        print("shutting down")
        utime.sleep(3)
        self.__socket.close()
        utime.sleep(3)
        print("disconnecting WIFI")
        wlan.disconnect()
        self.led_out.off()
        return True

