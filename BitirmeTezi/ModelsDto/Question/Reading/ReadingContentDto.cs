namespace BitirmeTezi.ModelsDto.Question.Reading
{
    public class ReadingContentDto
    {
        public string Title {get; set;}
        public string Paragraph { get; set; }
        public List<ReadingQuestion> Questions { get; set; }
    }
}
