## Centralized remote control of multiple Raspberry Pi Pico W devices

This repository contains a work in progress client and server that can be used to set up a network of raspberry pi pico W devices which can be controlled from a central point. Each device can have its own attached hardware and provide commands to drive the hardware. A terminal can be connected to the server and commands and responses relayed to and from devices.

See the professional mspaint diagram:

![Architecture](/arch.png)

The repository is broken into two parts:
- [A .NET based TCP server that can remotely control multiple deployed devices](./Server/README.md) 
  - supports sending a command to a remote device such as `set led on` which will turn on its onboard LED.
- [Micropython scripts](./Device/README.md) that run on a [Raspberry Pi Pico W](https://www.raspberrypi.com/documentation/microcontrollers/raspberry-pi-pico.html)
  - scripts that enable a Pico W to connect to the command server and receive and process commands
  - scripts that enable support for specific sensors attached to a Raspberry Pi Pico W

The command system is extensible so that devices with custom sensors attached can provide custom commands to control them. There is an example script for a device that has an HC-SR04 sonar sensor, which adds support for a `get_distance` command, which when received from the server responds with a distance reading taken from the sensor.
