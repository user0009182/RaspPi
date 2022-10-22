from dht11 import Dht11

dht11 = Dht11(15)
reading = dht11.get_temperature_humidity()
if reading != None:
    (temp, humidity) = reading
    print("temp={}C".format(temp))
    print("humidty={}%".format(humidity))
    