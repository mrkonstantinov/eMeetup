using eMeetup.Common.Application.Messaging;
using eMeetup.Modules.Enrolls.Domain.Customers;

namespace eMeetup.Modules.Enrolls.Application.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(Guid CustomerId, string Email, string UserName, DateTime DateOfBirth, Gender Gender)
    : ICommand;
