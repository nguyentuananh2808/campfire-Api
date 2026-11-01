using Campfire.Modules.Learning.Application.Models;

namespace Campfire.Modules.Learning.Application.Abstractions;

public interface ILearningTextService
{
    LearningModels.PreparedReadingText Prepare(string text);
}
