using System.ComponentModel.DataAnnotations;

namespace RealTimeQuizHub.Models.Dtos
{
    public class UpdateAnswerDto
    {
        [Required] public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
