namespace RealTimeQuizHub.Models.Dtos
{
    // A single row in the global (or room) leaderboard, including everything the
    // hover tooltip needs (level, totals, earned badges).
    public class LeaderboardEntryDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalScore { get; set; }
        public string Level { get; set; } = string.Empty;
        public int QuizzesCompleted { get; set; }
        public int Wins { get; set; }
        public List<BadgeDto> Badges { get; set; } = new();
    }
}
