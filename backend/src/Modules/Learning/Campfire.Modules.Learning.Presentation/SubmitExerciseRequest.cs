namespace Campfire.Modules.Learning.Presentation;
public sealed record SubmitExerciseRequest(IReadOnlyList<SubmitExerciseRequest.Answer> Answers)
{
    public sealed record Answer(string QuestionId, string Value);
}
