import network
import time
import socket
import machine

#manages wifi connection, including reconnection
class __DeviceWifi:
    #logger can be provided to obtain debug log information. See class Logger
    def __init__(self, network_name, network_secret, logger=None):
        self.__logger = logger
        self.__wlan = network.WLAN(network.STA_IF)
        self.__network_name = network_name
        self.__network_secret = network_secret

    #connects to wifi network
    #if wifi connection is already established returns immediately
    #otherwise will retry connection with falloff interval until connection established
    def retry_until_connected(self):
        wifi_retry_interval=1
        while True:
            self.__log("connecting to {}".format(self.__network_name))
            is_connected = self.__connect()
            if is_connected:
                break
            self.__log("no connection - will retry in {} seconds".format(wifi_retry_interval))
            time.sleep(wifi_retry_interval)
            wifi_retry_interval *= 2
            if wifi_retry_interval > 60:
                wifi_retry_interval = 60
    def __connect(self):
        #self.__wlan.disconnect()
        if self.__wlan.isconnected():
            self.__log("already connected")
            return True
        #re-activate the interface, seems more reliable
        self.__log("reactivating wlan interface")
        self.__wlan.active(False)
        self.__wlan.active(True)
        self.__wlan.connect(self.__network_name, self.__network_secret)
        for i in range(5):
            time.sleep(2)
            self.__log("connecting...")
            #self.led_out.toggle()
            if self.__wlan.isconnected():
                self.__log("WIFI connected")
                self.__log(self.__wlan.ifconfig())
                return True
        return False
    def is_connected(self):
        return self.__wlan.isconnected()
    def disconnect(self):
        self.__wlan.disconnect()
    def __log(self,txt):
        if self.__logger != None:
            self.__logger.log(txt)

#manages connection to the server, including reconnection
class __ServerConnection:
    def __init__(self, wifi, logger):
        self.__logger = logger
        self.__wifi = wifi
        self.__socket = None
    def retry_until_connected(self, ip, port):
        addr = socket.getaddrinfo(ip, port)[0][-1]
        retryInterval=1
        s = None
        while True:
            try:
                self.__log("connecting to {}:{}".format(ip, port))
                self.__socket = socket.socket()
                self.__socket.settimeout(10)
                self.__socket.connect(addr)
                self.__log("connected")
                self.connect_handshake()
                return self.__socket
            except Exception as exc:
                self.__log("failed to connect")
                self.__log(exc)
                self.__socket.close()
                if not self.__wifi.is_connected():
                    return False
            self.__log("retrying in {}s".format(retryInterval))
            time.sleep(retryInterval)
            retryInterval*=2
            if retryInterval > 60:
                retryInterval=60
        return None
    def connect_handshake(self):
        self.send_string_data("device")
        str_data=self.receive_string_data()
        if str_data == None:
            self.__log("connect_handshake failed")
            raise Exception()
        if str_data == "server":
            self.send_string_data("ok")
        return True
    def send_string_data(self, str):
        data = bytes(str, 'ascii')
        length = len(data)
        lengthBytes = length.to_bytes(2, "little")
        self.__socket.write(lengthBytes)
        n = self.__socket.write(data)
        self.__log("sent " + str)
    def receive_string_data(self):
        data = self.receive_data()
        if data == None:
            return None
        str=data.decode("ascii")
        self.__log("received " + str)
        return str
    def receive_data(self):
        try:
            dataLength = self.__socket.recv(2)
        except OSError as exc: 
            if exc.errno != 110: #timeout
                raise exc
            return None
        length = int.from_bytes(dataLength, "little")
        if length > 64:
            self.__log("receive length {} exceeds __max_recv_length {}".format(length, self.__max_recv_length))
            self.__shutdown=True
            return None
        try:
            data = self.__socket.recv(length)
        except OSError as exc:
            if exc.errno != 110: #timeout
                raise exc
            #todo handle bad state
            return None
        return data
    def disconnect(self):
        self.__socket.close()
    def __log(self,txt):
        if self.__logger != None:
            self.__logger.log(txt)

#can be passed to a DeviceClient to output debug logging
class Logger:
    def log(self, txt):
        print(txt)

#Runs a client that maintains a resiliant connection to a command server over WIFI
#The client will automtically reconnect if it detects a fault with the connection
#can be extended with custom commands
class DeviceClient:
    # logger          pass in an instance of Logger to log debug output
    def __init__(self, logger=None):
        self.__logger = logger
        self.__shutdown = False
        self.__command_handler = None
        self.__command_list = []
        self.__think_handler = None
        self.__led_out = machine.Pin("LED", machine.Pin.OUT)
        self.__led_in = machine.Pin("LED", machine.Pin.IN)
        self.__sensor_temp = machine.ADC(4)
        self.__next_think_time = 0

    #setup additional custom commands
    #command_handler        a function that will be called whenever the client receives a command from the server that isn't 
    #                       already handled. The command is passed in as a single string parameter. The function should either return a 
    #                       string that will be sent to the server as the response to the command, or return None to indicate the command isn't handled
    #command_list           an array of strings representing the names of the additional supported commands. This list is used to respond to
    #                       the server querying what commands are supported by the device
    def set_command_handler(self, command_handler, command_list):
        self.__command_handler = command_handler
        self.__command_list = command_list

    #think_handler          a function with no parameters that will be called periodically. This allows some code to be run by the device 
    #                       periodically, independently of a command from the server. It can be used to take periodic measurements. The
    #                       function should return an interger representing the approximate number of seconds until the function will be
    #                       called next
    def set_think_handler(self, think_handler):
        self.__think_handler = think_handler

    #start the client, connecting first to the WIFI and then over TCP to the given command server host and port
    #the WIFI network name and secret must be written to w.dat on the device (see write_wifi.py)
    #this function will block and try to remain connected to both the WIFI and the command server
    def start(self, server_host, server_port):
        (n, p) = self.__get_wifi_info()
        self.__wifi = __DeviceWifi(n, p, self.__logger)
        self.__server = __ServerConnection(self.__wifi, self.__logger)
        while not self.__shutdown:
            try:
                self.__wifi.retry_until_connected()
                self.__log("Connected to WIFI")
                self.__server.retry_until_connected(server_host, server_port)
                self.__log("Connected to server")
                self.__last_server_contact=time.time()
                self.__command_loop()
            except Exception as exc:
                self.__log(exc)
                self.__server.disconnect()
        self.__log("shutting down")
        time.sleep(3)
        self.__server.disconnect() 
        time.sleep(3)
        self.__log("disconnecting WIFI")
        self.__wifi.disconnect()
        self.led_out.off()
    def __command_loop(self):
        while True:
            self.__check_for_command()
            if self.__shutdown:
                break
            if time.time() > self.__next_think_time:
                self.__think()
            #if no contact has been made by the server for some time then assume the connection to the server has failed
            if time.time() > self.__last_server_contact + 120:
                self.__log("no ping received - reconnecting to server")
                raise Exception
            time.sleep(1)
    def __check_for_command(self):
        command=self.__server.receive_string_data()
        if command == None:
            return
        self.__last_server_contact = time.time()
        #implementation of automatically supported commands
        if command == "ping":
            self.__server.send_string_data("pong")
        elif command == "shutdown":
            self.__shutdown=True
        elif command == "?":
            #return a list of supported commands
            command_list = "ping,shutdown,get temp,get led,set led off,set led on"
            #append additional supported commands
            if not self.__command_list == None:
                additional_command_list=",".join(self.__command_list)
                if len(additional_command_list) > 0:
                    command_list += "," + additional_command_list
            self.__server.send_string_data(command_list)
        elif command == "get led":
            #return the current state of the onboard LED
            led_value = str(self.__led_in.value())
            self.__server.send_string_data(led_value)
        elif command == "set led off":
            self.__led_out.off()
            self.__server.send_string_data("ok")
        elif command == "set led on":
            self.__led_out.on()
            self.__server.send_string_data("ok")
        elif command == "get temp":
            reading = self.__readInternalTemperature()
            self.__server.send_string_data(str(reading))
        else:
            #command from server is not recognised as an internal command
            #try to run it through the custom command handler if one is defined
            processed = self.__process_custom_command(command)
            if not processed:
                self.__server.send_string_data("unknown command {}".format(command))
    def __process_custom_command(self, command):
        try:
            if not self.__command_handler == None:
                response_string = self.__command_handler(command)
                if response_string == None:
                    return False
                self.__server.send_string_data(response_string)
                return True
        except Exception as e:
            self.__log("error in command_handler - {}".format(e))
        return False
    def __readInternalTemperature(self):
        reading = self.__sensor_temp.read_u16()
        v = reading * 3.3 / 65535
        temperature = 27 - (v - 0.706) / 0.001721
        return temperature
    def __think(self):
        self.__log("running think")
        next_think_seconds=9999
        if self.__think_handler != None:
            next_think_seconds=self.__think_handler()
            if next_think_seconds < 30:
                next_think_seconds = 30
        self.__log("next think in {} seconds".format(next_think_seconds))
        self.__next_think_time = time.time() + next_think_seconds
    def __obf(self, byte_data):
        mask = b'abcdefg'
        lmask = len(mask)
        return bytes(c ^ mask[i % lmask] for i, c in enumerate(byte_data)) 
    def __get_wifi_info(self):
        with open('/w.dat', 'rb') as file:
            data = file.read()
            data=self.__obf(data)
            str=data.decode("ascii")
            (a,b) = str.split(',')
            return (a,b)
    def __log(self,txt):
        if self.__logger != None:
            self.__logger.log(txt)


