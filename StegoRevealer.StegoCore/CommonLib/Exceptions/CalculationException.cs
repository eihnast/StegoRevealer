namespace StegoRevealer.StegoCore.CommonLib.Exceptions;

public class CalculationException : Exception
{
    public CalculationException() : base() { }
    public CalculationException(string message) : base(message) { }
    public CalculationException(string message, Exception inner) : base(message, inner) { }
}
