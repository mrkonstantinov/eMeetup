namespace eMeetup.Modules.Enrolls.Application.Customers.GetCustomer;

public sealed record CustomerResponse(Guid Id, string Email, string FirstName, string LastName);
