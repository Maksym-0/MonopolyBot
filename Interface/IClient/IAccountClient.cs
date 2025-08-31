using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Models.API.Request;

namespace MonopolyBot.Interface.IClient
{
    internal interface IAccountClient
    {
        public Task<ApiResponse<AccountDto>> MeAsync(string jwt);
        public Task<ApiResponse<object>> RegisterAsync(AccountRequest account);
        public Task<ApiResponse<string>> LoginAndReturnJWTAsync(AccountRequest account);
        public Task<ApiResponse<object>> DeleteAccount(AccountRequest account);
    }
}
