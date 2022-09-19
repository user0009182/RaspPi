# Raspberry Pi Pico W Projects

Each folder contains a project using a Raspberry Pi Pico W:

- read_joystick. This project hooks up a joystick to the Raspberry Pi and reads the joystick x & y values, printing them to the screen
- neopixel_ring. Cycles the LEDs around an attached neopixel ring.
- joystick_neopixel. Drives an LED on a neopixel ring based on the direction of an attached joystick.
- onboard_temperature. Reads the onboard temperature, converting it to celcius and displaying it.
- command_server. Enables a pico W to be controlled by a remote control server over wifi. A command server is implemented in .Net.

The lib folder contains common library classes that are typically for interacting with a particular sensor or input device, such as a class to interact with a joystick. When a project needs to use one of these library classes I just copy it into the project folder.
