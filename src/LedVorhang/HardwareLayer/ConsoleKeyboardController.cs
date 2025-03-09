namespace HardwareLayer;

public class ConsoleKeyboardController : IDisposable
{
    private readonly Thread _keyboardThread;
    private volatile bool _running = true;

    public event EventHandler<ConsoleKeyboardEventArgs>? KeyPressed;

    public ConsoleKeyboardController()
    {
        _keyboardThread = new Thread(KeyboardLoop)
        {
            IsBackground = true
        };
        _keyboardThread.Start();
    }

    private void KeyboardLoop()
    {
        while (_running)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                OnKeyPressed(key.Key);
            }
            Thread.Sleep(10); // Prevent high CPU usage
        }
    }

    protected virtual void OnKeyPressed(ConsoleKey keyCode)
    {
        KeyPressed?.Invoke(this, new ConsoleKeyboardEventArgs(keyCode));
    }

    public void Dispose()
    {
        _running = false;
        _keyboardThread.Join(100); // Wait for thread to finish
    }
}