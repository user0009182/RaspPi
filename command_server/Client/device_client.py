import machine
import utime
import network
import socket
import time
import sys

#Device client class
class Client:
    __max_recv_length = 64

    def __init__(self):
        self.led_out = machine.Pin("LED", machine.Pin.OUT)
        self.led_in = machine.Pin("LED", machine.Pin.IN)
        self.sensor_temp = machine.ADC(4)
        self.__socket = None
        self.command_handler = None
        self.think_handler = None
        self.extra_command_list = None
        self._shutdown=False
        self._think_interval=60
        self._next_think_time=0

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
        length = len(data)
        lengthBytes = length.to_bytes(2, "little")
        self.__socket.write(lengthBytes)
        n = self.__socket.write(data)
        print("sent " + str)

    def receiveData(self):
        try:
            dataLength = self.__socket.recv(2)
        except OSError as exc: 
            if exc.errno != 110: #timeout
                raise exc
            return None
        length = int.from_bytes(dataLength, "little")
        if length > 64:
            print("receive length {} exceeds __max_recv_length {}".format(length, self.__max_recv_length))
            self._shutdown=True
            return None
        try:
            data = self.__socket.recv(length)
        except OSError as exc:
            if exc.errno != 110: #timeout
                raise exc
            #todo handle bad state
            return None
        return data

    def receive_string_data(self):
        data = self.receiveData()
        if data == None:
            return None
        str=data.decode("ascii")
        print("received " + str)
        return str

    def readTemperature(self):
        reading = self.sensor_temp.read_u16()
        v = reading * 3.3 / 65535
        temperature = 27 - (v - 0.706) / 0.001721
        return temperature

    def connectToServer(self, ip, port):
        addr = socket.getaddrinfo(ip, port)[0][-1]
        retryInterval=1
        self.__socket = None
        while True:
            try:
                print("connecting to {}:{}".format(ip, port))
                self.__socket = socket.socket()
                self.__socket.settimeout(10)
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
        str_data=self.receive_string_data()
        if str_data == None:
            print("connect_handshake failed")
            return False
        print("received " + str_data)
        if str_data == "server":
            self.sendStringData("ok")
        return True
    def process_command(self, command):
        try:
            if not self.command_handler == None:
                return self.command_handler(command)
        except Exception as e:
            print("error in command_handler - {}".format(e))
        return False
    def check_for_command(self):
        command=self.receive_string_data()
        if command == None:
            return
        if command == "ping":
            self.sendStringData("pong")
        elif command == "shutdown":
            self._shutdown=True
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
        elif command == "get temp":
            reading = self.readTemperature()
            self.sendStringData(str(reading))
        else:
            processed = self.process_command(command)
            if not processed:
                self.sendStringData("unknown command {}".format(command))

    def command_loop(self):
        while True:
            self.check_for_command()
            if self._shutdown:
                break
            if time.time() > self._next_think_time:
                self.think()
            time.sleep(1)

    def think(self):
        if self.think_handler != None:
            next_think_seconds=self.think_handler()
            if next_think_seconds < 30:
                next_think_seconds = 30
            self._next_think_time = time.time() + next_think_seconds

    def start(self, server_ip_address, server_port):
        self.led_out.off()
        wlan = self.connect_to_wifi()
        if wlan.isconnected() == False:
            sys.exit(0)
        self.led_out.off()
        self.connectToServer(server_ip_address, server_port)
        self.led_out.on()
        connected = self.connect_handshake()
        if connected:
            self.command_loop()
        print("shutting down")
        utime.sleep(3)
        self.__socket.close()
        utime.sleep(3)
        print("disconnecting WIFI")
        wlan.disconnect()
        self.led_out.off()
        return True
