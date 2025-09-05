using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Models.API.ApiRequest;

namespace MonopolyBot.Interface.IClient
{
    internal interface IRoomClient
    {
        public Task<ApiResponse<List<RoomDto>>> GetRoomsAsync(string jwt);
        public Task<ApiResponse<RoomDto>> CreateRoomAsync(string jwt, CreateRoomRequest dto);
        public Task<ApiResponse<RoomDto>> JoinRoomAsync(string jwt, JoinRoomRequest dto);
        public Task<ApiResponse<RoomDto>> QuitRoomAsync(string jwt);
    }
}
