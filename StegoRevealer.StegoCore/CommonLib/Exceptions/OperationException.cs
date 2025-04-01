namespace StegoRevealer.StegoCore.CommonLib.Exceptions;

public class OperationException : Exception
{
    public OperationException() : base() { }
    public OperationException(string message) : base(message) { }
    public OperationException(string message, Exception inner) : base(message, inner) { }
}
