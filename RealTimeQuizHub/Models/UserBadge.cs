namespace RealTimeQuizHub.Models
{
    // Join row: which badge a user has earned, and when. (UserId, BadgeId) is the key.
    public class UserBadge
    {
        public int UserId { get; set; }

        public int BadgeId { get; set; }

        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }

        public Badge? Badge { get; set; }
    }
}
