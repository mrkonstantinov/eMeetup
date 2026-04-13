using eMeetup.Common.Domain;

namespace eMeetup.Modules.Enrolls.Domain.Customers;

public sealed class Customer : Entity
{
    private Customer()
    {
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string UserName { get; private set; }

    public DateTime DateOfBirth { get; private set; }

    public Gender Gender { get; private set; }

    public static Customer Create(Guid id, string email, string userName, DateTime dateOfBirth, Gender gender)
    {
        return new Customer
        {
            Id = id,
            Email = email,
            UserName = userName,
            DateOfBirth = dateOfBirth,
            Gender = gender
        };
    }

    public void Update(string userName)
    {
        UserName = userName;
    }
}
