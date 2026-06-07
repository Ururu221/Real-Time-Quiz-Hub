using RealTimeQuizHub.Models.Dtos;

namespace RealTimeQuizHub.Services.Interfaces
{
    public interface IRoomService
    {
        Task<List<RoomDto>> GetActiveRoomsAsync();
        Task<RoomDetailDto?> GetRoomDetailAsync(int id);
        Task<RoomDto> CreateRoomAsync(CreateRoomDto dto, int createdByUserId);
        Task<bool> DeleteRoomAsync(int id);
    }
}
