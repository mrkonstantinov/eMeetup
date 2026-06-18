using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace eMeetup.Modules.Events.Domain.MateTypes;

public interface IMateTypeRepository
{
    Task<MateType?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid sessionId, CancellationToken cancellationToken = default);

    public void Insert(MateType mateType);    
}
