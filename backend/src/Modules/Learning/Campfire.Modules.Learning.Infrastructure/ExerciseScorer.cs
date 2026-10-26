using System.Text.Json;
using System.Text.Json.Nodes;
using Campfire.Modules.Learning.Application;

namespace Campfire.Modules.Learning.Infrastructure;

internal static class ExerciseScorer
{
    internal static (int Score, int Correct, int Total) Score(string contentJson, IReadOnlyList<LearningModels.ExerciseAnswer> answers)
    {
        using var document = JsonDocument.Parse(contentJson);
        var questions = document.RootElement.GetProperty("questions").EnumerateArray().ToArray();
        var supplied = answers.ToDictionary(x => x.QuestionId, x => x.Answer.Trim(), StringComparer.Ordinal);
        var correct = questions.Count(q => supplied.TryGetValue(q.GetProperty("id").GetString()!, out var answer) && string.Equals(answer, q.GetProperty("correctAnswer").GetString(), StringComparison.OrdinalIgnoreCase));
        return (questions.Length == 0 ? 0 : correct * 100 / questions.Length, correct, questions.Length);
    }

    internal static string WithoutAnswers(string contentJson)
    {
        var node = JsonNode.Parse(contentJson)!;
        if (node["questions"] is JsonArray questions)
            foreach (var question in questions.OfType<JsonObject>()) question.Remove("correctAnswer");
        return node.ToJsonString();
    }
}
