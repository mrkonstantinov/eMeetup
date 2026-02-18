namespace eMeetup.Common.Domain;

public enum ErrorType
{
    Validation = 0,
    NotFound = 1,
    Conflict = 2,
    Failure = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Problem = 6
}
