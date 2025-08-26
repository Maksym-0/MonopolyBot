using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Models.API.ApiRequest;

namespace MonopolyBot.Interface.IClient
{
    internal interface IRoomClient
    {
        public Task<Models.API.ApiResponse.ApiResponse<List<RoomDto>>> GetRoomsAsync(string jwt);
        public Task<Models.API.ApiResponse.ApiResponse<RoomDto>> CreateRoomAsync(string jwt, CreateRoomRequest dto);
        public Task<Models.API.ApiResponse.ApiResponse<RoomDto>> JoinRoomAsync(string jwt, JoinRoomRequest dto);
        public Task<Models.API.ApiResponse.ApiResponse<RoomDto>> QuitRoomAsync(string jwt);
    }
}
