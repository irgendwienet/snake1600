# SPI mit 5 CS-Pins auf RaspberryPi

## SPI aktivieren

In `/boot/firmware/config.txt`:

```
dtparam=spi=on
```

## Mehrere CS-Pins

```
dtc -@ -I dts -O dtb -o spi-cs-extend.dtbo spi-cs-extend.dts
cp spi-cs-extend.dtbo /boot/overlays
```

In `/boot/firmware/config.txt`:

```
dtoverlay=spi-cs-extend
```

Quelle: https://gist.github.com/mcbridejc/d060602e892f6879e7bc8b93aa3f85be