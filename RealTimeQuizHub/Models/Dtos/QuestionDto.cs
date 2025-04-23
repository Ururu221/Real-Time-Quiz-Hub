using System.ComponentModel.DataAnnotations;

namespace RealTimeQuizHub.Models.Dtos
{
    public class QuestionDto
    {
        [Required] public string Text { get; set; } = string.Empty;
    }
}
