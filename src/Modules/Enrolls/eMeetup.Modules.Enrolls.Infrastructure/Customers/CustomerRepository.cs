using eMeetup.Modules.Enrolls.Domain.Customers;
using eMeetup.Modules.Enrolls.Infrastructure.Database;  
using Microsoft.EntityFrameworkCore;

namespace eMeetup.Modules.Enrolls.Infrastructure.Customers;

internal sealed class CustomerRepository(EnrollsDbContext context) : ICustomerRepository
{
    public async Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Customers.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public void Insert(Customer customer)
    {
        context.Customers.Add(customer);
    }
}
