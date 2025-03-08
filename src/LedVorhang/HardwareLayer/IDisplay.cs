namespace HardwareLayer;

public interface IDisplay
{
    /// <summary>
    /// Updates the devices with the content of the provided LedImage,
    /// detecting changes and only updating the affected devices
    /// </summary>
    void Update(LedImage image);

    /// <summary>
    /// Forces update of all devices regardless of change detection
    /// </summary>
    void ForceUpdate(LedImage image);
}