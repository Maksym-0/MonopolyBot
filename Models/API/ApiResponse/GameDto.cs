namespace MonopolyBot.Models.API.ApiResponse
{
    internal class GameDto
    {
        public string GameId { get; set; }
        public List<Cell> Cells { get; set; }
        public List<Player> Players { get; set; }
    }
}
