using Microsoft.AspNetCore.Routing;

namespace eMeetup.Common.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
