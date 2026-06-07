namespace RealTimeQuizHub.Models
{
    // One room = one quiz session. A room hosts a single quiz.
    public class Room
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int CreatedByUserId { get; set; }

        public int QuizId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool HasTimer { get; set; }

        public int TimerSecondsPerQuestion { get; set; } = 30;

        // 0.0 = time doesn't affect score, 1.0 = answering instantly gives full bonus.
        public double TimerScoreImpact { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Quiz? Quiz { get; set; }
    }
}
