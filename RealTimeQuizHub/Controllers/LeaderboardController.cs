using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    [Authorize]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        // GET /api/leaderboard — top 20 users overall.
        [HttpGet]
        public async Task<IActionResult> GetGlobal()
        {
            var entries = await _leaderboardService.GetGlobalLeaderboardAsync(20);
            return Ok(entries);
        }

        // GET /api/leaderboard/room/{roomId} — participants of one room, ranked.
        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetRoom(int roomId)
        {
            var entries = await _leaderboardService.GetRoomLeaderboardAsync(roomId);
            return Ok(entries);
        }
    }
}
