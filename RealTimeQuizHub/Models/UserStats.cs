using System.ComponentModel.DataAnnotations.Schema;

namespace RealTimeQuizHub.Models
{
    // Aggregated, per-user totals. One row per user (UserId is the primary key).
    public class UserStats
    {
        public int UserId { get; set; }

        public int TotalScore { get; set; }

        public int QuizzesCompleted { get; set; }

        public int Wins { get; set; }

        public User? User { get; set; }

        // Level is derived from TotalScore, never stored.
        [NotMapped]
        public string Level => LevelHelper.GetLevel(TotalScore);
    }
}
