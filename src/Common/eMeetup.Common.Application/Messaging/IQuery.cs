using eMeetup.Common.Domain;
using MediatR;

namespace eMeetup.Common.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
