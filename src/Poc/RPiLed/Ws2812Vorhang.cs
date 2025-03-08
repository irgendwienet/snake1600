using System.Device.Spi;
using Iot.Device.Ws28xx;

namespace hello_ws2812;

public class Ws2812Vorhang(SpiDevice spiDevice, int width, int height = 1) : Ws28xxVorhang(spiDevice, (RawPixelContainer) new BitmapImageNeo3RgbVorhang(width, height))
{
}

public class Ws28xxVorhang
{
    /// <summary>
    /// SPI device used for communication with the LED driver
    /// </summary>
    protected readonly SpiDevice _spiDevice;

    /// <summary>
    /// Backing image to be updated on the driver
    /// </summary>
    public RawPixelContainer Image { get; }

    /// <summary>
    /// Constructs Ws28xx instance
    /// </summary>
    /// <param name="spiDevice">SPI device used for communication with the LED driver.</param>
    /// <param name="image">The bitmap that represents the screen or led strip.</param>
    public Ws28xxVorhang(SpiDevice spiDevice, RawPixelContainer image)
    {
        _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
        Image = image;
    }

    /// <summary>
    /// Sends backing image to the LED driver
    /// </summary>
    public void Update() => _spiDevice.Write(Image.Data);
}