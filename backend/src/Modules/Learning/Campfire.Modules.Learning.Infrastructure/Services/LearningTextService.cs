using System.Text.RegularExpressions;
using Campfire.Modules.Learning.Application.Abstractions;
using Campfire.Modules.Learning.Application.Models;

namespace Campfire.Modules.Learning.Infrastructure.Services;

public sealed partial class LearningTextService : ILearningTextService
{
    public LearningModels.PreparedReadingText Prepare(string text)
    {
        var normalized = LineWhitespace().Replace(text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n'), " ")
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var content = string.Join('\n', normalized).Trim();
        var chineseCharacters = content.Count(character => character is >= '\u3400' and <= '\u9fff');
        return new(content, content.Length, chineseCharacters, "zh-CN");
    }

    [GeneratedRegex(@"[\t\f\v ]+")]
    private static partial Regex LineWhitespace();
}
