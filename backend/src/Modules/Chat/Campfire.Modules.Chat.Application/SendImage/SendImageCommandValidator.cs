using FluentValidation;

namespace Campfire.Modules.Chat.Application.SendImage;

public sealed class SendImageCommandValidator : AbstractValidator<SendImageCommand>
{
    public SendImageCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ClientMessageId).NotEmpty();
        RuleFor(x => x.ContentType).Equal("image/webp");
        RuleFor(x => x.Bytes).NotEmpty().Must(bytes => bytes.Length <= 1_048_576)
            .WithMessage("Ảnh sau nén không được vượt quá 1 MiB.");
    }
}
