namespace Campfire.Modules.Learning.Domain;
public sealed class Vocabulary
{
    private Vocabulary() { }
    public Vocabulary(Guid id, string chinese, string pinyin, string meaningVi, int hskLevel, string exampleChinese, string examplePinyin, string exampleTranslationVi) =>
        (Id, Chinese, Pinyin, MeaningVi, HskLevel, ExampleChinese, ExamplePinyin, ExampleTranslationVi) = (id, chinese, pinyin, meaningVi, hskLevel, exampleChinese, examplePinyin, exampleTranslationVi);
    public Guid Id { get; private set; }
    public string Chinese { get; private set; } = string.Empty;
    public string Pinyin { get; private set; } = string.Empty;
    public string MeaningVi { get; private set; } = string.Empty;
    public int HskLevel { get; private set; }
    public string ExampleChinese { get; private set; } = string.Empty;
    public string ExamplePinyin { get; private set; } = string.Empty;
    public string ExampleTranslationVi { get; private set; } = string.Empty;
}
