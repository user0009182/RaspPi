#modified from https://github.com/namato/micropython-ov2640
import ov2640
import gc
import time
import sys

try:
    print("initializing camera")
    cam = ov2640.ov2640(resolution=ov2640.OV2640_320x240_JPEG)
    #cam = ov2640.ov2640(resolution=ov2640.OV2640_1024x768_JPEG)
    print(gc.mem_free())
    output_filename="image9.jpg"
    (image_data, image_size) = cam.capture_image()
    print("captured image is {} bytes".format(image_size))
    f = open(output_filename, 'wb')
    f.write(image_data, image_size)
    f.close()
    print("image written to {}".format(output_filename))
    sys.exit(0)
except KeyboardInterrupt:
    print("exiting...")
    sys.exit(0)
