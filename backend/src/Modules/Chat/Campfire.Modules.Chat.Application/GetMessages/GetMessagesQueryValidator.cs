using FluentValidation;

namespace Campfire.Modules.Chat.Application.GetMessages;

public sealed class GetMessagesQueryValidator : AbstractValidator<GetMessagesQuery>
{
    public GetMessagesQueryValidator()
    {
        RuleFor(query => query.GroupId).NotEmpty();
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.Limit).InclusiveBetween(1, 100);
        RuleFor(query => query).Must(query => string.IsNullOrEmpty(query.Before) || string.IsNullOrEmpty(query.After))
            .WithMessage("Before and after cursors cannot be combined.");
    }
}
