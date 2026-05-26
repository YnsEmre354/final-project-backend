namespace BitirmeTezi.ModelsDto.Question.Writing
{
    public class WritingEvaluationDto
    {
        public string CorrectedText { get; set; }   
        public string Feedback { get; set; }        
        public string DetectedLevel { get; set; }    
        public string MotivationMessage { get; set; }
        public int Score { get; set; }
    }
}
