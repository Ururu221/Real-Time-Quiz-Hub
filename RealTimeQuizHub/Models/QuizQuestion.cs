namespace RealTimeQuizHub.Models
{
    // Link table between a Quiz and the existing Questions, preserving order.
    public class QuizQuestion
    {
        public int Id { get; set; }

        public int QuizId { get; set; }

        public int QuestionId { get; set; }

        public int OrderIndex { get; set; }

        public Quiz? Quiz { get; set; }

        public Question? Question { get; set; }
    }
}
