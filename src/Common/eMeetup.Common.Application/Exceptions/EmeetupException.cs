using eMeetup.Common.Domain;

namespace eMeetup.Common.Application.Exceptions;

public sealed class EmeetupException : Exception
{
    public EmeetupException(string requestName, Error? error = default, Exception? innerException = default)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public string RequestName { get; }

    public Error? Error { get; }
}
