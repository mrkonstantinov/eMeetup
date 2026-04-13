using FluentValidation;

namespace eMeetup.Modules.Enrolls.Application.Customers.UpdateCustomer;

internal sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.UserName).NotEmpty();
    }
}
