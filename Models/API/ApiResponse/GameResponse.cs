using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Models.API.ApiResponse
{
    internal class GameResponse
    {
        public string GameId { get; set; }
        public List<Cell> Cells { get; set; }
        public List<Player> Players { get; set; }
    }
}
