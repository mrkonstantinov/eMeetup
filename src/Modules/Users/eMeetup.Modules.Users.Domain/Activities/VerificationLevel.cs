using eMeetup.Common.Domain;

namespace eMeetup.Modules.Users.Domain.Activities;

public class VerificationLevel : ValueObject
{
    public VerificationLevelType Type { get; }
    public int Score { get; }

    private VerificationLevel() { }

    private VerificationLevel(VerificationLevelType type, int score)
    {
        Type = type;
        Score = score;
    }

    public static VerificationLevel Basic()
    {
        return new VerificationLevel(VerificationLevelType.Basic, 1);
    }

    public static VerificationLevel EmailVerified()
    {
        return new VerificationLevel(VerificationLevelType.EmailVerified, 2);
    }

    public static VerificationLevel PhoneVerified()
    {
        return new VerificationLevel(VerificationLevelType.PhoneVerified, 3);
    }

    public static VerificationLevel IdentityVerified()
    {
        return new VerificationLevel(VerificationLevelType.IdentityVerified, 4);
    }

    public VerificationLevel AddVerification(VerificationType type)
    {
        return type switch
        {
            VerificationType.Email => EmailVerified(),
            VerificationType.Phone => PhoneVerified(),
            VerificationType.Identity => IdentityVerified(),
            _ => this
        };
    }

    public bool IsVerified()
    {
        return Type != VerificationLevelType.Basic;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Score;
    }
}

public enum VerificationLevelType
{
    Basic,
    EmailVerified,
    PhoneVerified,
    IdentityVerified
}

public enum VerificationType
{
    Email,
    Phone,
    Identity
}
