namespace RealTimeQuizHub.Models
{
    public class QuizSession
    {
        public string QuizId { get; set; } = string.Empty;

        public Question CurrentQuestion { get; set; } = new Question();

        public int CurrentQuestionIndex { get; set; } = 0;

        public int TotalQuestions { get; set; } = 0;

        public int CorrectAnswers { get; set; } = 0;
    }
}
