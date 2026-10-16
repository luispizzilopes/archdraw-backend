namespace Archdraw.Domain.Exceptions;

public class DuplicateResourceException : Exception
{
    public DuplicateResourceException(string message, Exception innerException) : base(message, innerException) { }
}
