using MonopolyBot.Models.ApiResponse;
using MonopolyBot.Models.API.ApiResponse;

namespace MonopolyBot.Interface
{
    internal interface IGameService
    {
        public Task<GameDto> GameStatusAsync(long chatId);
        public Task<MoveDto> RollDiceAsync(long chatId);
        public Task<PayDto> PayAsync(long chatId);
        public Task<BuyDto> BuyCellAsync(long chatId);
        public Task<LevelChangeDto> LevelUpCellAsync(long chatId, int cellNumber);
        public Task<LevelChangeDto> LevelDownCellAsync(long chatId, int cellNumber);
        public Task<NextActionDto> EndActionAsync(long chatId);
        public Task<LeaveGameDto> LeaveGameAsync(long chatId);
    }
}
