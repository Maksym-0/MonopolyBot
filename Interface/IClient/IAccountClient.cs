using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Models.API.Request;

namespace MonopolyBot.Interface.IClient
{
    internal interface IAccountClient
    {
        public Task<ApiResponse<AccountDto>> MeAsync(string jwt);
        public Task<ApiResponse<AccountDto>> RegisterAsync(AccountRequest account);
        public Task<ApiResponse<LoginDto>> LoginAndReturnJWTAsync(AccountRequest account);
        public Task<ApiResponse<DeleteAccountDto>> DeleteAccount(AccountRequest account);
    }
}
