# Command Server for Raspberry Pi Pico W devices

A work in progress server that multiple Rasp Pi Pico W devices can connect to over wifi. The server monitors connected devices, allowing them to be listed and queried. Commands can be issued from the server to a connected device to query its state or perform actions. For example a command "set led on" can be issued from the server and the device will turn its onboard LED on.

Devices must run a program that implements the client protocol to be comptaible with the server. An example micropython script (Device/vanilla/main.py) provides an example implementation of the client.

To setup a Rasperry Pi Pico W device as a client:
- Modify Device/write_wifi.py
  - set wifiUid to the name of the WIFI network that the server is accessible from
  - set wifiSecret to the WIFI network secret
- Run Device/write_wifi.py on a Rasp Pi Pico W
  - this will write the WIFI information in obfuscated form to a /w.dat file on the Rasp Pi Pico W
  - this step is to avoid having the WIFI info hardcoded into the main script
- Modify Device/vanilla/main.py
  - set the IP address and port of the server (currently this has to be fixed and hardcoded)
- Upload Device/vanilla/main.py to the device
- On startup the device will now attempt to connect to the server, and upon successfully doing so will listen for and respond to commands from the server

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

# Server discovery (TODO)
Currently devices hardcode the server IP address.

# Resiliance (TODO)
The server sends a periodic heartbeat to each connected device. This is to test that the client is still connected and responsive. If a client does not respond to a ping then the server considers them disconnected. Likewise if a device does not receive a ping for an extended period then the device assumes the server has disconnected.
Devices should (but the example code doesn't yet) attempt to reconnect if they become disconnected.

# Security (TODO)
- Prevent unauthorized client devices from connecting to the server
  - It's an attack vector. A client could be written to exploit a flaw in the protocol to attack the server.
  - Client authentication using a secret stored on the client device. The server immediately disconnects any client that is unable to verify itself.
- Prevent spoofing of the command server
  - An attacker could set up a fake command server which client devices would connect to.
  - This can be solved by assigning the command server a public key pair. The public key would be stored on all devices and used to verify the server.
- End-to-end Encryption of data
  - prevents anyone reading the data send between the server and devices even the attacker has the WIFI password
- Physical access to device
  - This is a weakness of the system. Because the device has no way of securing secrets, an attacker can obtain any information stored on the device.
    - The wifi password stored on that device
    - Any secret used for client authentication by that device, allowing them to spoof that device and potentially others
