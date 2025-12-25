# SPI mit 5 CS-Pins auf RaspberryPi

Ich verwende hier einen Raspberry Pi 2 Model B Rev 1.1.

Die SPI-Pins können zwar auch NeoPixel ansteuern, aber nur einer ist schnell genug für unsere Anforderungen. 

Um SPI nutzen zu können, muss es aktiviert werden. Außerdem benötigt es eine Extension um auf dem einen SPI-Bus mehrere
Signale mittels CS-Pin (Chip Select) ausgeben zu können. Da die LEDs aber keinen CS haben braucht es ein IC. Siehe unten. 

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

## Wiring

Der Raspberry Pi stellt und ein SPI-Datensignal sowie 4 ChipSelect Pins bereit. Die LEDs haben aber kein CS so dass 
ich einen SN7NCN08 (4x NOR) genutzt habe, so dass jeder der 4 LED-Stränge nur dann ein Signal bekommt, wenn der 
entsprechende ChipSelect Pin aktiv ist.

![Pinout](pinout.png) 