using FluentValidation;

namespace eMeetup.Modules.Enrolls.Application.Customers.CreateCustomer;

internal sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.Email).EmailAddress();
        RuleFor(c => c.UserName).NotEmpty();
        RuleFor(c => c.DateOfBirth).NotEmpty();
        RuleFor(c => c.Gender).NotEmpty();
    }
}
