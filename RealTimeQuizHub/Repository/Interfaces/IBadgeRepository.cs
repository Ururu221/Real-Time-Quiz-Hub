using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Repository.Interfaces
{
    public interface IBadgeRepository
    {
        // The full badge catalogue.
        Task<List<Badge>> GetAllAsync();

        // Badges a single user has already earned.
        Task<List<Badge>> GetUserBadgesAsync(int userId);

        // Awards a badge to a user (caller guarantees it isn't a duplicate).
        Task AddUserBadgeAsync(UserBadge userBadge);

        // Earned badges for many users at once, keyed by user id.
        Task<Dictionary<int, List<Badge>>> GetBadgesForUsersAsync(IEnumerable<int> userIds);
    }
}
