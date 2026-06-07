namespace RealTimeQuizHub.Models.Dtos
{
    // Returned to the client after recording a finished quiz: the points earned
    // this round, the player's rank in the room, refreshed totals/level, and any
    // badges that were just awarded.
    public class QuizCompletionResultDto
    {
        public int Score { get; set; }
        public int Rank { get; set; }
        public bool IsWin { get; set; }
        public int TotalScore { get; set; }
        public string Level { get; set; } = string.Empty;
        public int QuizzesCompleted { get; set; }
        public int Wins { get; set; }
        public List<BadgeDto> NewBadges { get; set; } = new();
    }
}
