namespace BitirmeTezi.ModelsDto.Question.Writing
{
    public class WritingTaskDto
    {
        public string Title { get; set; }         
        public string Instructions { get; set; }   
        public string TargetLevel { get; set; }    
        public List<string> SuggestedVocabulary { get; set; }
    }
}
