using MonopolyBot.Models.Service;
using MonopolyBot.Models.API.ApiResponse;

namespace MonopolyBot.Interface
{
    internal interface IAccService
    {
        public Task<AccServiceResponse> GetMyDataAsync(long chatId);
        public Task<AccountDto> RegisterAsync(string name, string password);
        public Task<AccountDto> LoginAsync(long chatId, string name, string password);
        public Task<DeleteAccountDto> DeleteAccountAsync(string name, string password);
    }
}
