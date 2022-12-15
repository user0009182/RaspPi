# Device Network
Components to build a network of devices that can be controlled remotely.

Connected devices can be sent commands and queries:
![Architecture](/diag1.png)

This respository contains
- A central Hub that acts as a server through which devices can be monitored and controlled
- Definition of a protocol for communication between devices and a hub
- A MicroPython client that implements the protocol and can be deployed onto a Raspberry Pi Pico W
- A C# server and client implementation of the protocol
- Example devices
- Example web application that communicates with a hub



