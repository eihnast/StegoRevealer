using System.Runtime.InteropServices;
using System.Text;

namespace StegoRevealer.Common.ConsoleInterface;

public static class WinConsole
{
    [DllImport("kernel32.dll")] 
    private static extern bool AttachConsole(int dwProcessId);

    [DllImport("kernel32.dll")] 
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")] 
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll")] 
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")] 
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")] 
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll")]
    private static extern uint GetConsoleOutputCP();

    [DllImport("kernel32.dll")]
    private static extern uint GetConsoleCP();

    private const int ATTACH_PARENT_PROCESS = -1;
    private const int STD_OUTPUT_HANDLE = -11;
    private const int STD_ERROR_HANDLE = -12;
    private const int STD_INPUT_HANDLE = -10;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    private static bool _attached;

    private static Encoding? _encIn, _encOut;

    public static void ConnectConsole()
    {
        if (_attached)
            return;

        if (!AttachConsole(ATTACH_PARENT_PROCESS))
            AllocConsole();

        _attached = true;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var cpOut = (int)GetConsoleOutputCP();
        var cpIn = (int)GetConsoleCP();
        _encOut = Encoding.GetEncoding(cpOut);
        _encIn = Encoding.GetEncoding(cpIn);

        Console.OutputEncoding = _encOut;
        Console.InputEncoding = _encIn;

        ReopenStandardStreams();

        TryEnableVtProcessing(STD_OUTPUT_HANDLE);
        TryEnableVtProcessing(STD_ERROR_HANDLE);
    }

    public static void DetachConsole()
    {
        if (!_attached) return;
        FreeConsole();
        _attached = false;
    }

    private static void ReopenStandardStreams()
    {
        var encOut = _encOut ?? Console.OutputEncoding;
        var encIn = _encIn ?? Console.InputEncoding;

        // stdout
        var stdOut = Console.OpenStandardOutput();
        var writerOut = new StreamWriter(stdOut, encOut) { AutoFlush = true };
        Console.SetOut(writerOut);

        // stderr
        var stdErr = Console.OpenStandardError();
        var writerErr = new StreamWriter(stdErr, encOut) { AutoFlush = true };
        Console.SetError(writerErr);

        // stdin
        var stdIn = Console.OpenStandardInput();
        var readerIn = new StreamReader(stdIn, encIn);
        Console.SetIn(readerIn);
    }

    private static void TryEnableVtProcessing(int stdHandle)
    {
        IntPtr handle = GetStdHandle(stdHandle);
        if (handle == IntPtr.Zero) return;
        if (!GetConsoleMode(handle, out uint mode)) return;
        SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
    }
}
