using MonopolyBot.Interface.IClient;
using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Interface.IRepository;
using MonopolyBot.Interface.IService;
using MonopolyBot.Models.Bot;

namespace MonopolyBot.Service
{
    internal class RoomService : IRoomService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoomClient _roomClient;
        private readonly IAuthorization _authorization;

        public RoomService(IRoomClient roomClient, IAuthorization authorization, IUserRepository userRepository)
        {
            _roomClient = roomClient;
            _authorization = authorization;
            _userRepository = userRepository;
        }

        public async Task<List<RoomDto>> GetRoomsAsync(long chatId)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);

            ApiResponse<List<RoomDto>> response = await _roomClient.GetRoomsAsync(user.JWT);
            if (!response.Success)
                throw new Exception(response.Message);

            return response.Data;
        }
        public async Task<RoomDto> CreateRoomAsync(long chatId, int maxNumberOfPlayers, string? password)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);

            ApiResponse<RoomDto> response = await _roomClient.CreateRoomAsync(user.JWT, new Models.API.ApiRequest.CreateRoomRequest
            {
                MaxNumberOfPlayers = maxNumberOfPlayers,
                Password = password
            });

            if (!response.Success)
                throw new Exception(response.Message);

            return response.Data;
        }
        public async Task<RoomDto> JoinRoomAsync(long chatId, string roomId, string? password)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);

            ApiResponse<RoomDto> response = await _roomClient.JoinRoomAsync(user.JWT, new Models.API.ApiRequest.JoinRoomRequest
            {
                RoomId = roomId,
                Password = password
            });

            if (!response.Success)
                throw new Exception(response.Message);
            
            user.GameId = roomId;
            await _userRepository.UpdateUserGameIdWithChatId(user.ChatId, user.GameId);
            return response.Data;
        }
        public async Task<string> QuitRoomAsync(long chatId)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);

            ApiResponse<RoomDto> response = await _roomClient.QuitRoomAsync(user.JWT);

            if (!response.Success)
                throw new Exception(response.Message);

            user.GameId = null;
            await _userRepository.UpdateUserGameIdWithChatId(user.ChatId, user.GameId);
            return response.Message;
        }
    }
}
