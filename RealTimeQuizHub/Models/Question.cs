namespace RealTimeQuizHub.Models
{
    public class Question
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<Answer> Answers { get; set; } = new List<Answer>();
    }
}
