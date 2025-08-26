using MonopolyBot.Models;

namespace MonopolyBot.Models.ApiResponse
{
    public class MoveDto
    {
        public Player Player { get; set; }
        public Cell Cell { get; set; }
        public string CellMessage { get; set; }
    }
}
