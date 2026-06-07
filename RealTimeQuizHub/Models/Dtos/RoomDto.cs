namespace RealTimeQuizHub.Models.Dtos
{
    // Summary shape used by the lobby room cards.
    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public bool IsActive { get; set; }
        public bool HasTimer { get; set; }
        public int TimerSecondsPerQuestion { get; set; }
        public double TimerScoreImpact { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
