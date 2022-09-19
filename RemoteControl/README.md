## Remote control of multiple Raspberry Pi Pico W devices

This repository contains a work in progress client and server that can be used to set up a network of raspberry pi pico W devices which can be controlled remotely from a central point. Each device can have its own attached hardware and provide commands to drive the hardware. A terminal can be connected to the server and commands and responses relayed to and from devices.

See the professional mspaint diagram:

![Architecture](/arch.png)

The repository is broken into two parts:
- [A .NET based TCP server that can remotely control multiple deployed devices](./Server/README.md) 
  - accepts multiple connecting devices
  - allows connected devices to be listed
  - allows a connected device to be queried for the commands it supports
  - allows a command to be sent to a remote device and returns the response from the device, for example:
    - `set led on` turns the device's onboard LED on
    - `get led` returns the current state of the device's onboard LED
    - `get_distance` returns a reading from a distance sensor attached to the device
    - `set_pixel 4 10 10 1` sets the color of the neopixel pixel #4 to rgb 10,10,1
  - detects device disconnects using a periodic ping
- [Micropython scripts](./Device/README.md) that run on a [Raspberry Pi Pico W](https://www.raspberrypi.com/documentation/microcontrollers/raspberry-pi-pico.html)
  - base script that provides client functionality allowing the device to connect to the command server and receive and process commands
    - extensible so that devices with custom sensors attached can provide their own custom commands and handlers 
  - scripts for driving hardware types, eg distance sensor 
  - example script that enables support for specific sensors attached to the device

