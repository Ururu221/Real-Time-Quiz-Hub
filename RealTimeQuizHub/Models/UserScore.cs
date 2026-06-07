namespace RealTimeQuizHub.Models
{
    // One row per finished quiz session: how many points a user earned in a quiz.
    public class UserScore
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int QuizId { get; set; }

        public int Score { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }

        public Quiz? Quiz { get; set; }
    }
}
