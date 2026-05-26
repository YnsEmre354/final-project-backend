namespace BitirmeTezi.ModelsDto.Question.Speaking
{
    public class SpeakingEvaluationRequestDto
    {
        public string Level { get; set; }
        public string Topic { get; set; }
        public string Instructions { get; set; }
        public IFormFile AudioFile { get; set; }

    }
}
