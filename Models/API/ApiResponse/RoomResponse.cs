using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Models.API.ApiResponse
{
    internal class RoomResponse
    {
        public string RoomId { get; set; }
        public int MaxNumberOfPlayers { get; set; }
        public int NumberOfPlayers { get; set; }
        public List<PlayerInRoom> Players { get; set; }
        public bool HavePassword { get; set; }
        public bool InGame { get; set; }
    }
}
