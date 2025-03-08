namespace HardwareLayer;

/// <summary>
/// Handles displaying a LedImage using four Ws28xxVorhang devices with change detection
/// </summary>
public class QuadDeviceDisplay : IDisplay
{
    private readonly Ws28xxVorhang _deviceA;
    private readonly Ws28xxVorhang _deviceB;
    private readonly Ws28xxVorhang _deviceC;
    private readonly Ws28xxVorhang _deviceD;

    private readonly int _subWidth;
    private readonly int _subHeight;

    private LedImage? _lastImage;
    private bool _dirtyA = true;
    private bool _dirtyB = true;
    private bool _dirtyC = true;
    private bool _dirtyD = true;

    public QuadDeviceDisplay(Ws28xxVorhang a, Ws28xxVorhang b, Ws28xxVorhang c, Ws28xxVorhang d)
    {
        if (a.Image.Width != b.Image.Width || a.Image.Width != c.Image.Width || a.Image.Width != d.Image.Width)
        {
            throw new ArgumentException("All devices must have the same width", nameof(b));
        }

        if (a.Image.Height != b.Image.Height || a.Image.Height != c.Image.Height || a.Image.Height != d.Image.Height)
        {
            throw new ArgumentException("All devices must have the same height", nameof(b));
        }

        _deviceA = a;
        _deviceB = b;
        _deviceC = c;
        _deviceD = d;

        _subWidth = a.Image.Width;
        _subHeight = a.Image.Height;
    }

    /// <summary>
    /// Updates the devices with the content of the provided LedImage,
    /// detecting changes and only updating the affected devices
    /// </summary>
    public void Update(LedImage image)
    {
        ValidateImageDimensions(image);
        DetectChanges(image);
        UpdateDevices(image);
        SaveLastImage(image);
    }

    /// <summary>
    /// Forces update of all devices regardless of change detection
    /// </summary>
    public void ForceUpdate(LedImage image)
    {
        ValidateImageDimensions(image);
        
        // Mark all quadrants as dirty
        _dirtyA = true;
        _dirtyB = true;
        _dirtyC = true;
        _dirtyD = true;
        
        UpdateDevices(image);
        SaveLastImage(image);
    }

    private void ValidateImageDimensions(LedImage image)
    {
        if (image.Width != _subWidth * 2 || image.Height != _subHeight * 2)
        {
            throw new ArgumentException("Image dimensions must match combined device dimensions (2x2 array)", nameof(image));
        }
    }

    private void DetectChanges(LedImage image)
    {
        // If this is the first update, mark all as dirty
        if (_lastImage == null)
        {
            _dirtyA = _dirtyB = _dirtyC = _dirtyD = true;
            return;
        }

        // Check each quadrant for changes
        _dirtyA = HasChangesInQuadrant(image, 0, 0, _subWidth, _subHeight);
        _dirtyB = HasChangesInQuadrant(image, _subWidth, 0, _subWidth * 2, _subHeight);
        _dirtyC = HasChangesInQuadrant(image, 0, _subHeight, _subWidth, _subHeight * 2);
        _dirtyD = HasChangesInQuadrant(image, _subWidth, _subHeight, _subWidth * 2, _subHeight * 2);
        
        bool HasChangesInQuadrant(LedImage image, int startX, int startY, int endX, int endY)
        {
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    if (_lastImage!.GetPixel(x, y) != image.GetPixel(x, y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
    
    private void UpdateDevices(LedImage image)
    {
        if (_dirtyA)
        {
            UpdateQuadrant(_deviceA, image, 0, 0, true);
            _deviceA.Update();
        }

        if (_dirtyB)
        {
            UpdateQuadrant(_deviceB, image, _subWidth, 0, true);
            _deviceB.Update();
        }

        if (_dirtyC)
        {
            UpdateQuadrant(_deviceC, image, 0, _subHeight, false);
            _deviceC.Update();
        }

        if (_dirtyD)
        {
            UpdateQuadrant(_deviceD, image, _subWidth, _subHeight, false);
            _deviceD.Update();
        }
    }

    private void UpdateQuadrant(Ws28xxVorhang device, LedImage image, int offsetX, int offsetY, bool flipY)
    {
        for (int y = 0; y < _subHeight; y++)
        {
            for (int x = 0; x < _subWidth; x++)
            {
                int deviceY = flipY ? _subHeight - y - 1 : y;
                device.Image.SetPixel(x, deviceY, image.GetPixel(x + offsetX, y + offsetY));
            }
        }
    }

    private void SaveLastImage(LedImage image)
    {
        _lastImage = image.Clone();
    }
}