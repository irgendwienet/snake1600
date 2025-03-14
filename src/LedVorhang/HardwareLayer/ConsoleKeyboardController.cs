using Timer = System.Timers.Timer;

namespace HardwareLayer;

public class ConsoleKeyboardController : IDisposable
{
    private readonly Timer _timer; 
    
    public event EventHandler<ConsoleKeyboardEventArgs>? KeyPressed;

    public ConsoleKeyboardController()
    {
        _timer = new Timer(1.0);
        _timer.Elapsed += (sender, args) => OnTick();
        _timer.Start();
    }

    private void OnTick()
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(intercept: true);
            OnKeyPressed(key.Key);
        }
    }

    protected virtual void OnKeyPressed(ConsoleKey keyCode)
    {
        KeyPressed?.Invoke(this, new ConsoleKeyboardEventArgs(keyCode));
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}