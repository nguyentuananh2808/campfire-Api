using System.Data;
using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Models;
using Campfire.Modules.Learning.Domain.Reading;
using Campfire.Modules.Learning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Campfire.Modules.Learning.Infrastructure.Services;

public sealed class EfReadingDocumentService(
    LearningDbContext dbContext,
    TimeProvider timeProvider) : IReadingDocumentService
{
    public async Task<LearningModels.ReadingDocumentDetail?> SaveAsync(
        Guid userId,
        string title,
        LearningModels.PreparedReadingText prepared,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        var userBytes = userId.ToByteArray();
        var lockKeyPartOne = BitConverter.ToInt32(userBytes, 0);
        var lockKeyPartTwo = BitConverter.ToInt32(userBytes, 4);
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({lockKeyPartOne}, {lockKeyPartTwo})",
            cancellationToken);
        if (await dbContext.ReadingDocuments.CountAsync(x => x.UserId == userId, cancellationToken) >= 50)
            return null;

        var document = new ReadingDocument(
            userId,
            title,
            prepared.Text,
            prepared.ChineseCharacterCount,
            timeProvider.GetUtcNow());
        dbContext.ReadingDocuments.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Detail(document);
    }

    public async Task<IReadOnlyList<LearningModels.ReadingDocumentSummary>> ListAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await dbContext.ReadingDocuments
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(50)
            .Select(x => new LearningModels.ReadingDocumentSummary(
                x.Id,
                x.Title,
                x.CharacterCount,
                x.ChineseCharacterCount,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

    public async Task<LearningModels.ReadingDocumentDetail?> GetAsync(
        Guid userId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.ReadingDocuments
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == documentId && x.UserId == userId, cancellationToken);
        return document is null ? null : Detail(document);
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid documentId,
        CancellationToken cancellationToken) =>
        await dbContext.ReadingDocuments
            .Where(x => x.Id == documentId && x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken) == 1;

    private static LearningModels.ReadingDocumentDetail Detail(ReadingDocument document) =>
        new(
            document.Id,
            document.Title,
            document.Content,
            document.CharacterCount,
            document.ChineseCharacterCount,
            "zh-CN",
            document.CreatedAtUtc,
            document.UpdatedAtUtc);
}
