using Campfire.Modules.Learning.Application.Common;
using MediatR;

namespace Campfire.Modules.Learning.Application.DeleteReadingDocument;

public sealed record DeleteReadingDocumentCommand(Guid UserId, Guid DocumentId)
    : IRequest<LearningResult<bool>>;
