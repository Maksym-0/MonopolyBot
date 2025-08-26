namespace MonopolyBot.Models
{
    public class Cell
    {
        public string GameId { get; set; }
        public int? MonopolyIndex { get; set; }
        public string? MonopolyType { get; set; }
        public bool Unique { get; set; }
        public bool? IsMonopoly { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public int? Price { get; set; }
        public int? Rent { get; set; }
        public string? OwnerId { get; set; }
        public int Level { get; set; }
    }
}
