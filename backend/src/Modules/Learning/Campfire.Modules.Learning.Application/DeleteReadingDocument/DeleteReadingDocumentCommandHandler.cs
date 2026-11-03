using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Common;
using MediatR;

namespace Campfire.Modules.Learning.Application.DeleteReadingDocument;

public sealed class DeleteReadingDocumentCommandHandler(IReadingDocumentService service)
    : IRequestHandler<DeleteReadingDocumentCommand, LearningResult<bool>>
{
    public async Task<LearningResult<bool>> Handle(
        DeleteReadingDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(request.UserId, request.DocumentId, cancellationToken);
        return deleted
            ? LearningResult<bool>.Success(true)
            : LearningResult<bool>.Failure("not_found", "Không tìm thấy văn bản luyện nghe.");
    }
}
