using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
using MediatR;

namespace Campfire.Modules.Learning.Application.SaveReadingDocument;

public sealed record SaveReadingDocumentCommand(Guid UserId, string Title, string Text)
    : IRequest<LearningResult<LearningModels.ReadingDocumentDetail>>;
