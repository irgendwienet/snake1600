# Snake auf einem  1600 LED Vorhang

Das ganze startete als Idee, vier dieser LED-Vorhänge an einen Raspberry Pi zu bekommen und dann diesem Ding eine sinnvolle Funktion zu geben.

Im Handel (beim großen A oder beim chinesischen A) finden sich LED-Vorhänge mit 20x20 LEDs als 2x2m oder 1x1m. 
Die 2x2m sind zu grob um als Display genutzt zu werden, daher habe ich 4 Stück 1x1m zu 2x2m zusammengebaut um ein 40x40 Pixeldisplay zu erhalten.

Die LEDs werden im Prinzip je Vorhang als NeoPixel / WS2812 angesteuert. Interessant ist, dass die Datenleitung nicht in Reihe durch alle Fäden
des Vorhangs verläuft. Die 4 Vorhänge also nicht in Reihe geschaltet werden können (was bei 1600 LEDs auch zu viel wäre).

Es wird also ein Controller benötigt, der 4 Datenleitungen liefert, die jeweils 400 LEDs ansteuert. 

Ich verwende hier einen Raspberry Pi 2 Model B Rev 1.1. Die SPI-Pins können zwar auch NeoPixel ansteuern, aber nur einer ist schnell genug 
für unsere Anforderungen. Siehe [spi.md](RaspberryPi/spi.md)

Zur Steuerung werden zwei Gamepads benötigt, die unter `/dev/input/js0` und `/dev/input/js1` gefunden werden. Ich verwende MSI Force GC30 V2. 

## Photos

TDB

## Das Spiel

Das Spiel besteht aus folgenden Ansichten:

 - von The Matrix inspirierter Bildschirmschoner, der nach 2 Minuten inaktivität startet
 - Auswahl für 1 oder 2 Spielermodus
 - das Spiel selbst
 - Anzeige der erreichten Punktzahl
 - Eingabe des Namen (4 Zeichen) bei Highscore. Dies wird in einer SQLite-DB gespeichert.
 - (Testscreen um verschiedene Farben zu testen)
 - (Laufschrift mit beliebigem Text)

## Die Software

Der Code ist in .net 8 geschreiben und läuft somit auch unter Linux. Die Harwareansteuerung ist in C# und das Spiel selbst in F# geschrieben. 
Vll ist das alles etwas kompliziert, aber es funktioniert.

## Website

Eine einfache Website greift auf die selben SQLite-DB zu, um die Highscores anzuzeigen.

## Stromverbrauch (gemessen)

Je LED / Farbe < 13 mA

Aber es skaliert nicht wenn man alle LEDs anktiviert. Ein 400 LEDs (1 Vorhang) braucht bei voll weiß ca 4 A.

 - 100x blau = 1,24 A
 - 100x rot = 1,25 A
 - 100x grün = 1,23 A
 - 100x weiß (255/255/255) = 2,3 A
 - 200x rot = 2,3 A
 - 400x rot = 3 A
 - 400x weiß = 3,4 A