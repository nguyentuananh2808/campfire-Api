namespace Campfire.Modules.Learning.Application;

public static class LearningModels
{
    public sealed record CourseSummary(Guid Id, string Slug, string Name, string Description, int HskLevel, string ContentVersion);
    public sealed record ActivitySummary(Guid Id, string Type, string Title, int OrderIndex, bool Required, bool Completed, string ContentJson);
    public sealed record DaySummary(Guid Id, int DayNumber, string Title, string TitleChinese, string Description, int EstimatedMinutes, int XpReward, string Status, int ProgressPercent);
    public sealed record UnitPath(Guid Id, string Title, int OrderIndex, IReadOnlyList<DaySummary> Days);
    public sealed record CoursePath(CourseSummary Course, int TotalXp, IReadOnlyList<UnitPath> Units);
    public sealed record DayDetail(CourseSummary Course, DaySummary Day, IReadOnlyList<ActivitySummary> Activities);
    public sealed record Today(CourseSummary Course, int TotalXp, DayDetail CurrentDay);
    public sealed record Completion(DayDetail Day, int TotalXp, int XpAwarded, bool DayCompleted);
    public sealed record ExerciseAnswer(string QuestionId, string Answer);
    public sealed record ExerciseResult(int Score, int Correct, int Total, Completion Completion);
    public sealed record MindMapNode(Guid Id, string Chinese, string Pinyin, string MeaningVi, int HskLevel, string ExampleChinese, string ExamplePinyin, string ExampleTranslationVi, string RelationType, int MasteryScore);
    public sealed record MindMap(MindMapNode Root, IReadOnlyList<MindMapNode> Children, string Source, bool AiExpansionAvailable);
    public sealed record Capabilities(bool TextToSpeech, bool PronunciationAssessment, bool ConversationAi, bool VocabularyAi, string Message);
}
