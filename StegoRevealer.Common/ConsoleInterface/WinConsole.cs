using System.Runtime.InteropServices;
using System.Text;

namespace StegoRevealer.Common.ConsoleInterface;

public static class WinConsole
{
    public static bool UseClearInput { get; set; } = false;

    private static string PromptLine = string.Empty;
    private static nint? SavedStdHandle;
    private static COORD? SavedCursorPosition;


    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD dwCursorPosition);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool ReadConsoleOutputCharacter(IntPtr hConsoleOutput, [Out] StringBuilder lpCharacter, uint nLength, COORD dwReadCoord, out uint lpNumberOfCharsRead);

    [StructLayout(LayoutKind.Sequential)]
    private struct COORD
    {
        public short X;
        public short Y;
    }

    private const int ATTACH_PARENT_PROCESS = -1;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private const int STD_OUTPUT_HANDLE = -11;

    private static void SavePromptLine()
    {
        // Сохраняем текущую позицию курсора и строку приглашения ввода
        IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
        COORD cursorPosition;
        cursorPosition.X = 0;
        cursorPosition.Y = (short)Console.CursorTop;

        // Читаем строку приглашения ввода
        StringBuilder promptLine = new StringBuilder(Console.WindowWidth);
        uint charsRead;
        ReadConsoleOutputCharacter(stdHandle, promptLine, (uint)Console.WindowWidth, cursorPosition, out charsRead);

        // Находим позицию символа приглашения (например, '>')
        int promptEndIndex = promptLine.ToString().LastIndexOf('>') + 1;
        if (promptEndIndex <= 0)
        {
            // Если символ приглашения не найден, используем всю строку
            promptEndIndex = (int)charsRead;
        }

        // Сохраняем только часть строки до символа приглашения
        PromptLine = promptLine.ToString(0, promptEndIndex);

        SavedStdHandle = stdHandle;
        SavedCursorPosition = cursorPosition;
    }

    public static void RestorePrompt()
    {
        if (SavedStdHandle is not null && SavedCursorPosition is not null)
        {
            // Восстанавливаем строку приглашения ввода
            SetConsoleCursorPosition(SavedStdHandle.Value, SavedCursorPosition.Value);
            Console.Write(PromptLine);
        }
    }

    public static void ConnectConsole()
    {
        UseClearInput = true;  // Если пытаемся присоединиться к консоли, это Win и нужно задействовать весь инструмент вывода

        // Проверяем, есть ли уже консоль
        if (GetConsoleWindow() == IntPtr.Zero)
        {
            // Попытка подключиться к консоли, из которой было запущено приложение
            if (!AttachConsole(ATTACH_PARENT_PROCESS))
            {
                // Если не удалось подключиться, создаем новую консоль
                AllocConsole();
            }
        }

        // Включаем обработку виртуальных терминалов для корректного вывода
        IntPtr consoleHandle = GetConsoleWindow();
        if (consoleHandle != IntPtr.Zero)
        {
            GetConsoleMode(consoleHandle, out uint mode);
            SetConsoleMode(consoleHandle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }

        SavePromptLine();
    }

    public static void StopConsole()
    {
        // Освобождаем консоль, если она была создана
        FreeConsole();
    }

    private static void ClearInput()
    {
        // Очищаем строку приглашения ввода
        IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
        COORD cursorPosition;
        cursorPosition.X = 0;
        cursorPosition.Y = (short)(Console.CursorTop - 0); // Перемещаем курсор на строку выше
        SetConsoleCursorPosition(stdHandle, cursorPosition);
        Console.Write(new string(' ', Console.WindowWidth)); // Очищаем строку
        SetConsoleCursorPosition(stdHandle, cursorPosition); // Возвращаем курсор
    }

    public static void Write(string message)
    {
        if (UseClearInput)
            ClearInput();
        Console.Write(message);
    }
    public static void WriteLine(string message)
    {
        if (UseClearInput)
            ClearInput();
        Console.WriteLine(message);
    }
}
