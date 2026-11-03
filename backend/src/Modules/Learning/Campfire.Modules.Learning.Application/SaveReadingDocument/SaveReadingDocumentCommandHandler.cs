using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
using MediatR;

namespace Campfire.Modules.Learning.Application.SaveReadingDocument;

public sealed class SaveReadingDocumentCommandHandler(
    ILearningTextService textService,
    IReadingDocumentService documentService)
    : IRequestHandler<SaveReadingDocumentCommand, LearningResult<LearningModels.ReadingDocumentDetail>>
{
    public async Task<LearningResult<LearningModels.ReadingDocumentDetail>> Handle(
        SaveReadingDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var prepared = textService.Prepare(request.Text);
        var document = await documentService.SaveAsync(
            request.UserId,
            request.Title.Trim(),
            prepared,
            cancellationToken);
        return document is null
            ? LearningResult<LearningModels.ReadingDocumentDetail>.Failure(
                "conflict",
                "Thư viện đã đủ 50 văn bản. Hãy xóa một bài cũ trước khi lưu bài mới.")
            : LearningResult<LearningModels.ReadingDocumentDetail>.Success(document);
    }
}
