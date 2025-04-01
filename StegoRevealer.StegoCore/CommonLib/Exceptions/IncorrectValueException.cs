namespace StegoRevealer.StegoCore.CommonLib.Exceptions;

public class IncorrectValueException : Exception
{
    public IncorrectValueException() : base() { }
    public IncorrectValueException(string message) : base(message) { }
    public IncorrectValueException(string message, Exception inner) : base(message, inner) { }
}
