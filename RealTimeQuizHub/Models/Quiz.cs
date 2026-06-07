namespace RealTimeQuizHub.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool HasTimer { get; set; }

        public int TimerSecondsPerQuestion { get; set; } = 30;

        // 0.0 = time doesn't affect score, 1.0 = answering instantly gives full bonus.
        public double TimerScoreImpact { get; set; }

        public int CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
    }
}
