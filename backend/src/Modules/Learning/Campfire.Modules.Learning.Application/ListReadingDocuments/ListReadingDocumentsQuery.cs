using Campfire.Modules.Learning.Application.Models;
using MediatR;

namespace Campfire.Modules.Learning.Application.ListReadingDocuments;

public sealed record ListReadingDocumentsQuery(Guid UserId)
    : IRequest<IReadOnlyList<LearningModels.ReadingDocumentSummary>>;
