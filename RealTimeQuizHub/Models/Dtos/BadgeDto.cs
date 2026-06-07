namespace RealTimeQuizHub.Models.Dtos
{
    // Compact badge shape used inside leaderboard rows and tooltips.
    public class BadgeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconEmoji { get; set; } = string.Empty;
    }
}
