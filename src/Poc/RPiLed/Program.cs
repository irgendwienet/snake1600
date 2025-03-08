using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;
using Gamepad;

// dotnet restore --runtime linux-arm /p:PublishReadyToRun=true

// SPI mit mehr CS
// https://gist.github.com/mcbridejc/d060602e892f6879e7bc8b93aa3f85be

namespace hello_ws2812
{
    class Program
    {
        static int count = 20;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Pi!");

            Ws28xxVorhang device0 = CreateWs2812B(0);
            Ws28xxVorhang device1 = CreateWs2812B(1);
            Ws28xxVorhang device2 = CreateWs2812B(3);
            Ws28xxVorhang device3 = CreateWs2812B(4);

            var image = new LedImage(40,40);
            var imagedevice = new QuadDeviceDisplay(device1, device3, device2, device0);

            var gamepad = new GamepadController("/dev/input/js0");

            for (int i = 0; i < 40; i++)
            {
                image.SetPixel(i, 0, Color.Red);
                imagedevice.Update(image);
            }

            //image.Clear();

            for (int i = 0; i < 40; i++)
            {
                image.SetPixel(0, i, Color.Green);
                imagedevice.Update(image);
            }

            //image.Clear();

            for (int i = 0; i < 40; i++)
            {
                image.SetPixel(i, 39, Color.Blue);
                imagedevice.Update(image);
            }

            //image.Clear();

            for (int i = 0; i < 40; i++)
            {
                image.SetPixel(39, i, Color.Yellow);
                imagedevice.Update(image);
            }


            Console.WriteLine("Let's go!");

            int x = 20;
            int y = 20;
            var c = Color.Red;

            gamepad.ButtonChanged += (_, e) =>
            {
                if (e.Button == 0)
                    ChangeColor(Color.Red);
                else if (e.Button == 1)
                    ChangeColor(Color.Blue);
                else if (e.Button == 2)
                    ChangeColor(Color.Green);
                else if (e.Button == 3)
                    ChangeColor(Color.Yellow);
            };

            // Configure this if you want to get events when the state of an axis changes
            gamepad.AxisChanged += (object sender, AxisEventArgs e) =>
            {


                if (e.Axis == 7 && e.Value < 0)
                    Move(-1, 0);
                else if (e.Axis == 7 && e.Value > 0)
                    Move(1, 0);
                else if (e.Axis == 6 && e.Value < 0)
                    Move(0, -1);
                else if (e.Axis == 6 && e.Value > 0)
                    Move(0, 1);
                else
                {
                    Console.WriteLine($"Axis {e.Axis} Pressed: {e.Value}");
                }
            };

            void Move(int deltaX, int deltaY)
            {
                if (x+deltaX < 0 || x+deltaX >= 40 || y+deltaY < 0 || y+deltaY >= 40)
                    return;

                image.SetPixel(x, y, Color.Black);

                x += deltaX;
                y += deltaY;

                image.SetPixel(x, y, c);
                imagedevice.Update(image);
            }

            void ChangeColor(Color newC)
            {
                c = newC;

                image.SetPixel(x, y, c);
                imagedevice.Update(image);
            }


            while (true)
            {
                Thread.Sleep(TimeSpan.FromHours(1));
            }

            // 3 integer als Arguments und daraus machen wir eine RGB farbe
            // var r = int.Parse(args[1]);
            // var g = int.Parse(args[2]);
            // var b = int.Parse(args[3]);
            // var color = Color.FromArgb(r, g, b);
            // for (int i = 0; i < count; i++)
            // {
            //     test.SetPixel(i,0,color);
            //     //device0.Image.SetPixel(i,0,color);
            // }
            // test.Update();
            //device0.Update();


//            MovingAll(device0, device1, device2, device3);
            //ColorFadeAll(device0, device1, device2, device3);

            // Bunt(device0);
            // Thread.Sleep(2500);
            //
            // Bunt(device1);
            // Thread.Sleep(2500);

            // // Chase some blue leds
            // MovingBlue(device0);
            // MovingBlue(device1);
            //
            // // Color Fade
            // //ColorFade(device0);
            //
            // Clear(device0);
            // Clear(device1);
        }

        private static Ws28xxVorhang CreateWs2812B(int chipSelectLine)
        {
            var settings0 = new SpiConnectionSettings(0, chipSelectLine)
            {
                ClockFrequency = 3_400_000, // sollte eigentlich 2400000 sein
                Mode = SpiMode.Mode0,
                DataBitLength = 8,
                //ChipSelectLineActiveState = PinValue.High

            };
            var spi0 = SpiDevice.Create(settings0);
            var device0 = new Ws2812Vorhang(spi0, 20, 20);

            Console.WriteLine($"Turn on {chipSelectLine}");

            return device0;
        }

        private static void Clear(Ws28xxVorhang device)
        {
            if (device == null) return;

            var image = device.Image;
            image.Clear();
            device.Update();
        }

        private static void Bunt(Ws28xxVorhang device)
        {
            var image = device.Image;
            image.SetPixel(0, 0, Color.Orange);
            image.SetPixel(1, 0, Color.Red);
            image.SetPixel(2, 0, Color.Green);
            image.SetPixel(3, 0, Color.Blue);
            image.SetPixel(4, 0, Color.Yellow);
            image.SetPixel(5, 0, Color.Cyan);
            image.SetPixel(6, 0, Color.Magenta);
            image.SetPixel(7, 0, Color.FromArgb(unchecked((int)0xffff8000)));
            device.Update();
        }

        private static void MovingAll(Ws28xxVorhang device0, Ws28xxVorhang device1, Ws28xxVorhang device2, Ws28xxVorhang device3)
        {
            for(int i=0; i<5; i++)
            {
                device0.Image.Clear();
                device1.Image.Clear();
                device2.Image.Clear();
                device3.Image.Clear();

                for(int j=0; j<count; j++)
                {
                    device0.Image.SetPixel(j, 0, Color.Blue);
                    device1.Image.SetPixel(j, 0, Color.Red);
                    device2.Image.SetPixel(j, 0, Color.Green);
                    device3.Image.SetPixel(j, 0, Color.Yellow);

                    device0.Update();
                    device1.Update();
                    device2.Update();
                    device3.Update();
                    System.Threading.Thread.Sleep(25);
                }
            }
        }

        private static void MovingBlue(Ws28xxVorhang device)
        {
            var image = device.Image;

            for(int i=0; i<5; i++)
            {
                image.Clear();
                for(int j=0; j<count; j++)
                {
                    image.SetPixel(j, 0, Color.LightBlue);
                    device.Update();
                    System.Threading.Thread.Sleep(10);
                    image.SetPixel(j, 0, Color.Blue);
                    device.Update();
                    System.Threading.Thread.Sleep(25);
                }
            }
        }

        private static void ColorFadeAll(Ws28xxVorhang device0, Ws28xxVorhang device1, Ws28xxVorhang device2, Ws28xxVorhang device3)
        {
            int r=255;
            int g=0;
            int b=0;

            while (! Console.KeyAvailable) {
                if(r > 0 && b == 0){
                    r--;
                    g++;
                }
                if(g > 0 && r == 0){
                    g--;
                    b++;
                }
                if(b > 0 && g == 0){
                    r++;
                    b--;
                }

                if (device0 != null)
                {
                    device0.Image.Clear();
                    device0.Image.SetPixel(0, 0, Color.FromArgb(r, g, b));
                    device0.Image.SetPixel(4, 0, Color.FromArgb(r, g, b));
                    device0.Update();
                }

                if (device1 != null)
                {
                    device1.Image.Clear();
                    device1.Image.SetPixel(0, 0, Color.FromArgb(r, g, b));
                    device1.Image.SetPixel(5, 0, Color.FromArgb(r, g, b));
                    device1.Update();
                }

                if (device2 != null)
                {
                    device2.Image.Clear();
                    device2.Image.SetPixel(0, 0, Color.FromArgb(r, g, b));
                    device2.Image.SetPixel(6, 0, Color.FromArgb(r, g, b));
                    device2.Update();
                }

                if (device3 != null)
                {
                    device3.Image.Clear();
                    device3.Image.SetPixel(0, 0, Color.FromArgb(r, g, b));
                    device3.Image.SetPixel(7, 0, Color.FromArgb(r, g, b));
                    device3.Update();
                }

                Console.Write(".");

                System.Threading.Thread.Sleep(10);
            }
        }


        private static void ColorFade(Ws28xxVorhang device)
        {
            var image = device.Image;

            int r=255;
            int g=0;
            int b=0;

            while (! Console.KeyAvailable) {
                if(r > 0 && b == 0){
                    r--;
                    g++;
                }
                if(g > 0 && r == 0){
                    g--;
                    b++;
                }
                if(b > 0 && g == 0){
                    r++;
                    b--;
                }

                image.Clear(Color.FromArgb(r,g,b));
                device.Update();
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}