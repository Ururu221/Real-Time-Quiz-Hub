namespace RealTimeQuizHub.Models.Dtos
{
    // A participant on the room result page: their best score in this room plus
    // their global level/profile (for the hover tooltip).
    public class RoomLeaderboardEntryDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Level { get; set; } = string.Empty;
        public int TotalScore { get; set; }
        public int QuizzesCompleted { get; set; }
        public int Wins { get; set; }
        public List<BadgeDto> Badges { get; set; } = new();
    }
}
