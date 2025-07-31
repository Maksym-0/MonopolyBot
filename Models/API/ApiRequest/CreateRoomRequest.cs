using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Models.API.ApiRequest
{
    internal class CreateRoomRequest
    {
        public int MaxNumberOfPlayers { get; set; }
        public string? Password { get; set; }
    }
}
