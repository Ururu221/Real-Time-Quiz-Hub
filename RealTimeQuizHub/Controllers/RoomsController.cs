using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(IRoomService roomService, ILogger<RoomsController> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }

        // GET /api/rooms — list all active rooms (any authenticated user).
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rooms = await _roomService.GetActiveRoomsAsync();
            return Ok(rooms);
        }

        // GET /api/rooms/{id} — room details with quiz info.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _roomService.GetRoomDetailAsync(id);
            if (room == null)
            {
                return NotFound(new { message = "Room not found" });
            }
            return Ok(room);
        }

        // POST /api/rooms — create a room (admins only).
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
        {
            if (!IsAdmin())
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Only administrators can create rooms." });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var room = await _roomService.CreateRoomAsync(dto, GetUserId());
                _logger.LogInformation("Room {RoomId} created by user {UserId}", room.Id, GetUserId());
                return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/rooms/{id} — admins only.
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Only administrators can delete rooms." });
            }

            var deleted = await _roomService.DeleteRoomAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Room not found" });
            }
            return NoContent();
        }

        private bool IsAdmin()
        {
            var claim = User.FindFirst("isAdmin")?.Value;
            return string.Equals(claim, "true", StringComparison.OrdinalIgnoreCase);
        }

        private int GetUserId()
        {
            var claim = User.FindFirst("id")?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
