namespace RealTimeQuizHub.Models.Dtos
{
    // Sent by the client when a player finishes a room's quiz. The server
    // re-computes the score from the per-answer data so it can't be spoofed.
    public class CompleteQuizDto
    {
        public int RoomId { get; set; }

        public List<AnswerResultDto> Answers { get; set; } = new();
    }

    // One answered question: whether it was correct and how many seconds were
    // left on the timer when it was submitted (0 when the room has no timer).
    public class AnswerResultDto
    {
        public bool IsCorrect { get; set; }
        public int TimeRemaining { get; set; }
    }
}
