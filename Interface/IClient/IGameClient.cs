using MonopolyBot.Models.API.ApiResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Interface.IClient
{
    internal interface IGameClient
    {
        public Task<Models.API.ApiResponse.ApiResponse<GameResponse>> GetGameStatusAsync(string jwt, string gameId);
        public Task<Models.API.ApiResponse.ApiResponse<string>> RollTheDiceAsync(string jwt, string gameId);
        public Task<Models.API.ApiResponse.ApiResponse<bool>> PayAsync(string jwt, string gameId);
        public Task<Models.API.ApiResponse.ApiResponse<bool>> BuyCellAsync(string jwt, string gameId);
        public Task<Models.API.ApiResponse.ApiResponse<object>> LevelUpCellAsync(string jwt, string gameId, int cellNumber);
        public Task<Models.API.ApiResponse.ApiResponse<object>> LevelDownCellAsync(string jwt, string gameId, int cellNumber);
        public Task<Models.API.ApiResponse.ApiResponse<string>> EndActionAsync(string jwt, string gameId);
        public Task<Models.API.ApiResponse.ApiResponse<string>> LeaveGameAsync(string jwt, string gameId);
    }
}
