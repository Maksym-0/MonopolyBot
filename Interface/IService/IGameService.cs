using MonopolyBot.Interface.IClient;
using MonopolyBot.Interface.IRepository;
using MonopolyBot.Models.API.ApiResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Interface
{
    internal interface IGameService
    {
        public Task<GameResponse> GameStatusAsync(long chatId);
        public Task<bool> RollDicesAsync(long chatId);
        public Task<bool> PayAsync(long chatId);
        public Task<bool> BuyCellAsync(long chatId);
        public Task<bool> LevelUpCellAsync(long chatId, int cellNumber);
        public Task<bool> LevelDownCellAsync(long chatId, int cellNumber);
        public Task<bool> EndActionAsync(long chatId);
        public Task<bool> LeaveGameAsync(long chatId);
    }
}
