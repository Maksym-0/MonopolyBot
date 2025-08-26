using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Models.ApiResponse;

namespace MonopolyBot.Interface.IClient
{
    internal interface IGameClient
    {
        public Task<ApiResponse<GameDto>> GetGameStatusAsync(string jwt, string gameId);
        public Task<ApiResponse<MoveDto>> RollTheDiceAsync(string jwt, string gameId);
        public Task<ApiResponse<PayDto>> PayAsync(string jwt, string gameId);
        public Task<ApiResponse<BuyDto>> BuyCellAsync(string jwt, string gameId);
        public Task<ApiResponse<LevelChangeDto>> LevelUpCellAsync(string jwt, string gameId, int cellNumber);
        public Task<ApiResponse<LevelChangeDto>> LevelDownCellAsync(string jwt, string gameId, int cellNumber);
        public Task<ApiResponse<NextActionDto>> EndActionAsync(string jwt, string gameId);
        public Task<ApiResponse<LeaveGameDto>> LeaveGameAsync(string jwt, string gameId);
    }
}
