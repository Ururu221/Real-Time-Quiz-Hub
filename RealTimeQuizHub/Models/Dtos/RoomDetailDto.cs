namespace RealTimeQuizHub.Models.Dtos
{
    // Full room view including the ordered questions of its quiz.
    public class RoomDetailDto : RoomDto
    {
        public List<QuizQuestionDto> Questions { get; set; } = new List<QuizQuestionDto>();
    }

    public class QuizQuestionDto
    {
        public int QuestionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }
}
