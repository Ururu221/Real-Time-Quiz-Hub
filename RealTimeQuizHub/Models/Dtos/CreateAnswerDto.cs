using System.ComponentModel.DataAnnotations;

namespace RealTimeQuizHub.Models.Dtos
{
    public class CreateAnswerDto
    {
        [Required] public int QuestionId { get; set; }
        [Required] public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
