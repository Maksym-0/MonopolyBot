using MonopolyBot.Models.API.ApiResponse;

namespace MonopolyBot.Interface.IService
{
    internal interface IRoomService
    {
        public Task<List<RoomDto>> GetRoomsAsync(long chatId);
        public Task<RoomDto> CreateRoomAsync(long chatId, int maxNumberOfPlayers, string? password);
        public Task<RoomDto> JoinRoomAsync(long chatId, string roomId, string? password);
        public Task<string> QuitRoomAsync(long chatId);
    }
}
