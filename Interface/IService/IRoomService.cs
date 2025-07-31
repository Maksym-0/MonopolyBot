using MonopolyBot.Models.API.ApiResponse;

namespace MonopolyBot.Interface.IService
{
    internal interface IRoomService
    {
        public Task<List<RoomResponse>> GetRoomsAsync(long chatId);
        public Task<RoomResponse> CreateRoomAsync(long chatId, int maxNumberOfPlayers, string? password);
        public Task<RoomResponse> JoinRoomAsync(long chatId, string roomId, string? password);
        public Task<string> QuitRoomAsync(long chatId);
    }
}
