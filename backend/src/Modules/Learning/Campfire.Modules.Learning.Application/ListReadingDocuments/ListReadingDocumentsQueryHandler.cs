using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Models;
using MediatR;

namespace Campfire.Modules.Learning.Application.ListReadingDocuments;

public sealed class ListReadingDocumentsQueryHandler(IReadingDocumentService service)
    : IRequestHandler<ListReadingDocumentsQuery, IReadOnlyList<LearningModels.ReadingDocumentSummary>>
{
    public Task<IReadOnlyList<LearningModels.ReadingDocumentSummary>> Handle(
        ListReadingDocumentsQuery request,
        CancellationToken cancellationToken) =>
        service.ListAsync(request.UserId, cancellationToken);
}
