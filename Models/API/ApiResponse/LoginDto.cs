namespace MonopolyBot.Models.API.ApiResponse
{
    public class LoginDto
    {
        public AccountDto Account { get; set; }
        
        public string Token { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
