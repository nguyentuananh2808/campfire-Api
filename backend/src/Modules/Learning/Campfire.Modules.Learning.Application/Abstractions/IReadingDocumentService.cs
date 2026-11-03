using Campfire.Modules.Learning.Application.Models;

namespace Campfire.Modules.Learning.Application.Abstractions;

public interface IReadingDocumentService
{
    Task<LearningModels.ReadingDocumentDetail?> SaveAsync(
        Guid userId,
        string title,
        LearningModels.PreparedReadingText prepared,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<LearningModels.ReadingDocumentSummary>> ListAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<LearningModels.ReadingDocumentDetail?> GetAsync(
        Guid userId,
        Guid documentId,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid userId, Guid documentId, CancellationToken cancellationToken);
}
