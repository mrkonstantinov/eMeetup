namespace eMeetup.Modules.Users.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore.Storage;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
