using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
using MediatR;

namespace Campfire.Modules.Learning.Application.PrepareReadingText;

public sealed class PrepareReadingTextCommandHandler(ILearningTextService service)
    : IRequestHandler<PrepareReadingTextCommand, LearningResult<LearningModels.PreparedReadingText>>
{
    public Task<LearningResult<LearningModels.PreparedReadingText>> Handle(
        PrepareReadingTextCommand request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(LearningResult<LearningModels.PreparedReadingText>.Success(service.Prepare(request.Text)));
    }
}
