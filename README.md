## Raspberry Pi Pico W

The Raspberry Pi Pico W (https://www.raspberrypi.com/documentation/microcontrollers/raspberry-pi-pico.html) is a microcontroller with an onboard WIFI interface. It can connect to a named WIFI network and then communiate with other devices on that network.

The Server folder contains the implementation of a .NET based TCP server that is intended to act as a command server for multiple Pico W devices. A Pico W device connects to the command server over wifi and responds to commands sent from the command server.

[Command Server](./Server/README.md)
[Device scripts](./Device/README.md) - various MicroPython scripts that can be uploaded to a Pico W, including the base code for enabling a device to connect to the command server.


To setup a Rasperry Pi Pico W device as a client:
- Modify Device/write_wifi.py
  - set wifiUid to the name of the WIFI network that the server is accessible from
  - set wifiSecret to the WIFI network secret
- Run Device/write_wifi.py on a Rasp Pi Pico W
  - this will write the WIFI information in obfuscated form to a /w.dat file on the Rasp Pi Pico W
  - this step is to avoid having the WIFI info hardcoded into the main script
- Modify Device/main.py
  - set the IP address and port of the server (currently this has to be fixed and hardcoded)
- Upload Device/main.py to the device
- On startup the device will now attempt to connect to the server, and upon successfully doing so will listen for and respond to commands from the server



Command Server

Devices must run a program that implements the client protocol to be comptaible with the server. An example micropython script (Device/vanilla/main.py) provides an example implementation of the client.



To start the server:
- Run the server from visual studio
- Currently output is to the debug console (this is why to run from visual studio). You should see in the debug output console when a device connects
- The console window allows commands to be entered and responses displayed (this is why debug output doesn't go to the console)
  - Issuing a "set led on" or "set led off" will change the LED state
  
# Client Protocol
- A device first must connect to the server's TCP listen port
- Once connected the device should begin receiving commands from the server in the format:
  - 2 byte little endian integer (ushort) denoting the length of the command (n)
  - n bytes ascii encoded string that make up the command
- upon receiving a command the device must process it and respond back with a reply:
  - 2 byte little endian integer (ushort) denoting the length of the reply (n)
  - n byte ascii encoded string that makes up the reply content
- at a minimum a device must recognize the "ping" command 
  - the device must reply back with "pong"
- the device then waits to receive the next command

