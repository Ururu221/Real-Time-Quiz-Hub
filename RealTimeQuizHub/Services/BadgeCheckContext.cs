namespace RealTimeQuizHub.Services
{
    // Everything BadgeService needs to know about a just-finished quiz to decide
    // which achievements to hand out.
    public class BadgeCheckContext
    {
        public int UserId { get; set; }

        // The player finished first in the room this round.
        public bool WonThisGame { get; set; }

        // The room had its timer enabled.
        public bool TimerRoom { get; set; }

        // Every answer was correct.
        public bool PerfectScore { get; set; }
    }
}
