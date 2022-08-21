## Raspberry Pi Pico W

This repository contains the following:
- A .NET command server for control of multiple remotely deployed devices
  - supports sending a command to a remote device such as `set led on` which will turn on its onboard LED.
- Micropython scripts that enable a [Raspberry Pi Pico W](https://www.raspberrypi.com/documentation/microcontrollers/raspberry-pi-pico.html) device to connect to the command server
- Micropython scripts that enable support for specific sensors attached to a Raspberry Pi Pico W

The command system is extensible so that devices with custom sensors attached can provide custom commands to control them. There is an example script for a device that has an HC-SR04 sonar sensor attached. It adds support for a `get_distance` command, which when received from the server responds with a distance reading taken from the sensor. Some commands are supported by all devices. For example issuing a `set led on` command to a device will turn the onboard LED of that device on. Issuing a `shutdown` will cause the device to disconnect.

[Command Server](./Server/README.md)
[Device scripts](./Device/README.md)
