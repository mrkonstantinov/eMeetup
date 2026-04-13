using eMeetup.Modules.Enrolls.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eMeetup.Modules.Enrolls.Infrastructure.Customers;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserName).HasMaxLength(200);

        builder.Property(c => c.Email).HasMaxLength(300);
    }
}
