namespace eMeetup.Common.Application.Exceptions;

public class ConcurrencyException : RepositoryException
{
    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException) { }
}
