using RealTimeQuizHub.Models;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IJwtGenerator
    {
        string CreateToken(User user);
    }
}
