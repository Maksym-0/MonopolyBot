using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Models.API.Request;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Interface.IClient
{
    internal interface IAccountClient
    {
        public Task<ApiResponse<AccountDto>> MeAsync(string jwt);
        public Task<ApiResponse<object>> RegisterAsync(AccountRequest account);
        public Task<ApiResponse<string>> LoginAndReturnJWTAsync(AccountRequest account);
    }
}
