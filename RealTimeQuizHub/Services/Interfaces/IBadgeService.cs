using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IBadgeService
    {
        // Evaluates the badge rules for a finished quiz and awards any the user
        // has newly earned. Returns only the badges granted by this call.
        Task<List<Badge>> CheckAndAwardBadgesAsync(BadgeCheckContext context);
    }
}
