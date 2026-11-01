using Campfire.Modules.Learning.Application.Common;
using Campfire.Modules.Learning.Application.Models;
using MediatR;

namespace Campfire.Modules.Learning.Application.PrepareReadingText;

public sealed record PrepareReadingTextCommand(string Text) : IRequest<LearningResult<LearningModels.PreparedReadingText>>;
