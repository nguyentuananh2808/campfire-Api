using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
using MediatR;

namespace Campfire.Modules.Learning.Application.GetReadingDocument;

public sealed record GetReadingDocumentQuery(Guid UserId, Guid DocumentId)
    : IRequest<LearningResult<LearningModels.ReadingDocumentDetail>>;
