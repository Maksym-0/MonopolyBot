using MonopolyBot.Interface.IService;
using MonopolyBot.Interface;
using MonopolyBot.Interface.IClient;
using MonopolyBot.Interface.IRepository;
using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Models.Bot;

namespace MonopolyBot.Service
{
    internal class GameService : IGameService
    {
        private readonly IUserRepository _userRepository;
        private readonly IGameClient _gameClient;
        private readonly IAuthorization _authorization;

        public GameService(IGameClient gameClient, IAuthorization authorization, IUserRepository userRepository)
        {
            _gameClient = gameClient;
            _authorization = authorization;
            _userRepository = userRepository;
        }

        public async Task<GameResponse> GameStatusAsync(long chatId)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);
            var response = await _gameClient.GetGameStatusAsync(user.JWT, user.GameId);
            if(!response.Success)
            {
                throw new Exception($"Не вдалося отримати статус гри: {response.Message}");
            }
            return response.Data;
        }
        public async Task<bool> RollDiceAsync(long chatId)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);
            var response = await _gameClient.RollTheDiceAsync(user.JWT, user.GameId);
            if (!response.Success)
            {
                throw new Exception($"Не вдалося кинути кубики: {response.Message}");
            }
            return response.Success;
        }
        public async Task<bool> PayAsync(long chatId)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);
            var response = await _gameClient.PayAsync(user.JWT, user.GameId);
            if (!response.Success)
            {
                throw new Exception($"Не вдалося здійснити платіж: {response.Message}");
            }
            return response.Success;
        }
        public async Task<bool> BuyCellAsync(long chatId)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);
            var response = await _gameClient.BuyCellAsync(user.JWT, user.GameId);
            if (!response.Success)
            {
                throw new Exception($"Не вдалося купити клітинку: {response.Message}");
            }
            return response.Success;
        }
        public async Task<bool> LevelUpCellAsync(long chatId, int cellNumber)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);
            var response = await _gameClient.LevelUpCellAsync(user.JWT, user.GameId, cellNumber);
            if (!response.Success)
            {
                throw new Exception($"Не вдалося підвищити рівень клітинки: {response.Message}");
            }
            return response.Success;
        }
        public async Task<bool> LevelDownCellAsync(long chatId, int cellNumber)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);
            var response = await _gameClient.LevelDownCellAsync(user.JWT, user.GameId, cellNumber);
            if (!response.Success)
            {
                throw new Exception($"Не вдалося знизити рівень клітинки: {response.Message}");
            }
            return response.Success;
        }
        public async Task<bool> EndActionAsync(long chatId)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);
            var response = await _gameClient.EndActionAsync(user.JWT, user.GameId);
            if (!response.Success)
            {
                throw new Exception($"Не вдалося завершити дію: {response.Message}");
            }
            return response.Success;
        }
        public async Task<bool> LeaveGameAsync(long chatId)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);

            if(user.GameId == null)
                throw new Exception("Гравець вже поза грою");

            var response = await _gameClient.LeaveGameAsync(user.JWT, user.GameId);
            if (response.Success)
            {
                user.GameId = null;
                await _userRepository.UpdateUserGameId(user.ChatId, user.GameId);
            }
            else
            {
                throw new Exception($"Не вдалося вийти з гри: {response.Message}");
            }
            return response.Success;
        }
    }
}
