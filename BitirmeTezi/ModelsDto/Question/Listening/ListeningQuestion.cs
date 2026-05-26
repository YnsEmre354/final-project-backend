namespace BitirmeTezi.ModelsDto.Question.Listening
{
    public class ListeningQuestion
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
    }
}
