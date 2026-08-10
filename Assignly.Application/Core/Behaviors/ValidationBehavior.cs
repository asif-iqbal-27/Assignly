using ErrorOr;
using FluentValidation;
using MediatR;

namespace Assignly.Application.Core.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IErrorOr
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .GroupBy(f => f.PropertyName)
                .SelectMany(g => g)
                .ToList();

            if (failures.Count != 0)
            {
                var responseType = typeof(TResponse);
                if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ErrorOr<>))
                {
                    var errorType = responseType.GetGenericArguments()[0];
                    var errors = failures.Select(f => Error.Validation(f.PropertyName, f.ErrorMessage)).ToList();
                    var createMethod = typeof(ErrorOr<>).MakeGenericType(errorType).GetMethod("From", new[] { typeof(List<Error>) })!;
                    return (TResponse)createMethod.Invoke(null, new object[] { errors })!;
                }
            }
        }

        return await next();
    }
}
