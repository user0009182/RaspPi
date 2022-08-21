## Device scripts

To configure a Pico W as a client device:
- create the wifi connection info file w.dat on the device (see below)
- upload the device_client.py module to the device
- create a main.py script that starts a client using the device_client.py module (see main.py)

### Wifi connection info (w.dat)

The Rasperry Pi Pico W device needs to know which WIFI network to connect to and the password for that network. Instead of hardcoding this into the script, the client script reads this information from a file (w.dat) stored on the device. The WIFI name and password in this file is obfuscated. The w.dat file can be created on the device by modifying write_wifi.py with the correct WIFI network name and password and then running the script on the device.

