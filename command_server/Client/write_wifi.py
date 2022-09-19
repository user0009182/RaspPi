def obf(byte_data):
    mask = b'abcdefg'
    lmask = len(mask)
    return bytes(c ^ mask[i % lmask] for i, c in enumerate(byte_data))

wifiUid="uid"
wifiSecret="abcdef"

with open('/w.dat', 'wb') as file:
    u=wifiUid+","+wifiSecret
    a=obf(bytes(u, 'ascii'))
    file.write(a)

def getWifiInfo():
    with open('/w.dat', 'rb') as file:
        data = file.read()
        data=obf(data)
        str=data.decode("ascii")
        (a,b) = str.split(',')
        return (a,b)

(n, p) = getWifiInfo()
print(n)
print(p)