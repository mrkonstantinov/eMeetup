using eMeetup.Common.Application.Messaging;
using eMeetup.Common.Domain;
using eMeetup.Modules.Enrolls.Application.Abstractions.Data;
using eMeetup.Modules.Enrolls.Domain.Customers;

namespace eMeetup.Modules.Enrolls.Application.Customers.UpdateCustomer;

internal sealed class UpdateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCustomerCommand>
{
    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        Customer? customer = await customerRepository.GetAsync(request.CustomerId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound(request.CustomerId));
        }

        customer.Update(request.UserName);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
