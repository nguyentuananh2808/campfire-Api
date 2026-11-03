using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
using MediatR;

namespace Campfire.Modules.Learning.Application.GetReadingDocument;

public sealed class GetReadingDocumentQueryHandler(IReadingDocumentService service)
    : IRequestHandler<GetReadingDocumentQuery, LearningResult<LearningModels.ReadingDocumentDetail>>
{
    public async Task<LearningResult<LearningModels.ReadingDocumentDetail>> Handle(
        GetReadingDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await service.GetAsync(request.UserId, request.DocumentId, cancellationToken);
        return document is null
            ? LearningResult<LearningModels.ReadingDocumentDetail>.Failure("not_found", "Không tìm thấy văn bản luyện nghe.")
            : LearningResult<LearningModels.ReadingDocumentDetail>.Success(document);
    }
}
