using System.ComponentModel.DataAnnotations;

namespace RealTimeQuizHub.Models.Dtos
{
    public class CreateRoomDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Either reference an existing quiz via QuizId, or supply NewQuiz to create one inline.
        public int? QuizId { get; set; }

        public CreateQuizDto? NewQuiz { get; set; }

        public bool HasTimer { get; set; }

        [Range(1, 3600)]
        public int TimerSecondsPerQuestion { get; set; } = 30;

        [Range(0.0, 1.0)]
        public double TimerScoreImpact { get; set; }
    }
}
