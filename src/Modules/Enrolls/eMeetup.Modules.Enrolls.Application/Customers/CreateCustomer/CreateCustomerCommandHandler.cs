using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Enrolls.Application.Abstractions.Data;
using eMeetup.Modules.Enrolls.Domain.Customers;

namespace eMeetup.Modules.Enrolls.Application.Customers.CreateCustomer;

internal sealed class CreateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCustomerCommand>
{
    public async Task<Result> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(request.CustomerId, request.Email, request.UserName, request.DateOfBirth, request.Gender);

        customerRepository.Insert(customer);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
