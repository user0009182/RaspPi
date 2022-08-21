## Device scripts

To configure a Pico W as a client device:
- create the wifi connection info file w.dat on the device (see below)
- upload the device_client.py module to the device
- create a main.py script that starts a client using the device_client.py module (see main.py)

### Wifi connection info (w.dat)

The Rasperry Pi Pico W device needs to know which WIFI network to connect to and the password for that network. Instead of hardcoding this into the script, the client script reads this information from a file (w.dat) stored on the device. The WIFI name and password in this file is obfuscated. The w.dat file can be created on the device by modifying write_wifi.py with the correct WIFI network name and password and then running the script on the device.

# Client Protocol for command server
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
