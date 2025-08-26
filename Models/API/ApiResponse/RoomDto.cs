namespace MonopolyBot.Models.API.ApiResponse
{
    internal class RoomDto
    {
        public string RoomId { get; set; }
        public int MaxNumberOfPlayers { get; set; }
        public int NumberOfPlayers { get; set; }
        public List<PlayerInRoom> Players { get; set; }
        public bool HavePassword { get; set; }
        public bool InGame { get; set; }
    }
}
