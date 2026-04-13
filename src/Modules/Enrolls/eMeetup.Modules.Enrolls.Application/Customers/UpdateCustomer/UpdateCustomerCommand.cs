using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Enrolls.Application.Customers.UpdateCustomer;

public sealed record UpdateCustomerCommand(Guid CustomerId, string UserName) : ICommand;
