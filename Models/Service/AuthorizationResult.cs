using MonopolyBot.Models.Bot;

namespace MonopolyBot.Models.Service
{
    internal class AuthorizationResult
    {
        public bool IsAuthorized { get; set; }
        public string Message { get; set; }
        public User? User { get; set; }
    }
}
