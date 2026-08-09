using ErrorOr;
using MediatR;

namespace Assignly.Application.Core.Abstractions;

public interface ICommand<TResponse> : IRequest<ErrorOr<TResponse>>;

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, ErrorOr<TResponse>>
    where TCommand : ICommand<TResponse>;

public interface IQuery<TResponse> : IRequest<ErrorOr<TResponse>>;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, ErrorOr<TResponse>>
    where TQuery : IQuery<TResponse>;
