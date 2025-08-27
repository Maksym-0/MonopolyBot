namespace MonopolyBot.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string GameId { get; set; }
        public int Balance { get; set; }
        public int Location { get; set; }
        public int CantAction { get; set; }
        public int ReverseMove { get; set; }
        public Dice LastDiceResult { get; set; }
        public bool IsPrisoner { get; set; }
        public bool InGame { get; set; }
        public bool NeedPay { get; set; }
        public bool HisAction { get; set; }
        public bool CanMove { get; set; }
        public bool CanBuyCell { get; set; }
        public bool CanLevelUpCell { get; set; }
    }
}
