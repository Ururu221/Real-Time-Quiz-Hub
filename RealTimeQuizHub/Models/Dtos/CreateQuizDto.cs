using System.ComponentModel.DataAnnotations;

namespace RealTimeQuizHub.Models.Dtos
{
    public class CreateQuizDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool HasTimer { get; set; }

        [Range(1, 3600)]
        public int TimerSecondsPerQuestion { get; set; } = 30;

        [Range(0.0, 1.0)]
        public double TimerScoreImpact { get; set; }

        // New questions (with their answers) to create together with the quiz.
        public List<NewQuestionDto> Questions { get; set; } = new List<NewQuestionDto>();

        // Legacy: IDs of already-existing questions to link, in order.
        // Kept for backward compatibility with room creation.
        public List<int> QuestionIds { get; set; } = new List<int>();
    }

    public class NewQuestionDto
    {
        [Required]
        [MaxLength(300)]
        public string Text { get; set; } = string.Empty;

        public List<NewAnswerDto> Answers { get; set; } = new List<NewAnswerDto>();
    }

    public class NewAnswerDto
    {
        [Required]
        [MaxLength(100)]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}
