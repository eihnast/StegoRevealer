namespace StegoRevealer.StegoCore.CommonLib.Exceptions;

public class NullValueException : IncorrectValueException
{
    public NullValueException() : base() { }
    public NullValueException(string message) : base(message) { }
    public NullValueException(string message, Exception inner) : base(message, inner) { }
}
