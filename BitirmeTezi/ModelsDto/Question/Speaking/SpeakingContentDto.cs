namespace BitirmeTezi.ModelsDto.Question.Speaking
{
    public class SpeakingContentDto
    {
        public string Title { get; set; }
        public string Instructions { get; set; }
        public string TargetLevel { get; set; }
        public List<string> SuggestedVocabulary { get; set; }
        public List<string> SpeakingTips { get; set; }
    }
}
