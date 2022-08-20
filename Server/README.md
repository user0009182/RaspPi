## Command Server

Simple TCP server that Rasp Pi Pico W devices running device_client.py can connect to over WIFI. Such devices will then be controllable from the server, which can issue commands to them.

Currently the server should be run from Visual Studio under the debugger. This is because a lot of operational output is sent to the debug console. Standard output and input are reserved for sending commands and reading the response.

Current commands:

- `list` displays a list of connected devices
- `set led on` and `set led off` are commands that device_client.py supports which will switch the pico w's onboard LED on/off
- `shutdown` will cause the remote client to disconnect

## Resiliance

The server sends a periodic heartbeat to each connected device. This is to test that the client is still connected and responsive. If a client does not respond to a ping then the server considers them disconnected. Likewise if a device does not receive a ping for an extended period it can assume the server has disconnected.

## Security (TODO)
- Validate responses
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
