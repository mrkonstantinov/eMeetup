using eMeetup.Common.Application.Messaging;

namespace eMeetup.Modules.Enrolls.Application.Customers.GetCustomer;

public sealed record GetCustomerQuery(Guid CustomerId) : IQuery<CustomerResponse>;
