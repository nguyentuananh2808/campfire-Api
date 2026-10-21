using FluentValidation;
using MediatR;

namespace Campfire.Modules.Werewolf.Application.Common;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = (await Task.WhenAll(validators.Select(validator => validator.ValidateAsync(request, cancellationToken))))
            .SelectMany(result => result.Errors).Where(failure => failure is not null).ToArray();
        if (failures.Length != 0)
            throw new RequestValidationException(failures.GroupBy(failure => failure.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray()));
        return await next();
    }
}
