using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Models.API.ApiRequest;

namespace MonopolyBot.Interface.IClient
{
    internal interface IRoomClient
    {
        public Task<Models.API.ApiResponse.ApiResponse<List<RoomResponse>>> GetRoomsAsync(string jwt);
        public Task<Models.API.ApiResponse.ApiResponse<RoomResponse>> CreateRoomAsync(string jwt, CreateRoomRequest dto);
        public Task<Models.API.ApiResponse.ApiResponse<RoomResponse>> JoinRoomAsync(string jwt, JoinRoomRequest dto);
        public Task<Models.API.ApiResponse.ApiResponse<RoomResponse>> QuitRoomAsync(string jwt);
    }
}
