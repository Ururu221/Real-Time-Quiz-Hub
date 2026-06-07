namespace RealTimeQuizHub.Models.Dtos
{
    // Shape returned by GET /api/quizzes for the public quiz list.
    public class QuizListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int QuestionCount { get; set; }
        public bool HasTimer { get; set; }
        public int TimerSecondsPerQuestion { get; set; }
        public double TimerScoreImpact { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
