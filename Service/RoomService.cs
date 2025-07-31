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

        public async Task<List<RoomResponse>> GetRoomsAsync(long chatId)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);

            ApiResponse<List<RoomResponse>> response = await _roomClient.GetRoomsAsync(user.JWT);
            if (!response.Success)
                throw new Exception($"Помилка при отриманні списку доступних кімнат: {response.Message}");

            return response.Data;
        }
        public async Task<RoomResponse> CreateRoomAsync(long chatId, int maxNumberOfPlayers, string? password)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);

            ApiResponse<RoomResponse> response = await _roomClient.CreateRoomAsync(user.JWT, new Models.API.ApiRequest.CreateRoomRequest
            {
                MaxNumberOfPlayers = maxNumberOfPlayers,
                Password = password
            });

            if (!response.Success)
                throw new Exception($"Помилка при створенні кімнати: {response.Message}");

            return response.Data;
        }
        public async Task<RoomResponse> JoinRoomAsync(long chatId, string roomId, string? password)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);

            ApiResponse<RoomResponse> response = await _roomClient.JoinRoomAsync(user.JWT, new Models.API.ApiRequest.JoinRoomRequest
            {
                RoomId = roomId,
                Password = password
            });

            if (!response.Success)
                throw new Exception($"Помилка при приєднанні до кімнати: {response.Message}");
            
            user.GameId = roomId;
            await _userRepository.UpdateUserGameId(user);
            return response.Data;
        }
        public async Task<string> QuitRoomAsync(long chatId)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);

            ApiResponse<RoomResponse> response = await _roomClient.QuitRoomAsync(user.JWT);

            if (!response.Success)
                throw new Exception($"Помилка при виході з кімнати: {response.Message}");

            user.GameId = null;
            await _userRepository.UpdateUserGameId(user);
            return response.Message;
        }
    }
}
