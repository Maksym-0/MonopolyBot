using MonopolyBot.Clients;
using MonopolyBot.Models.API.Request;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonopolyBot.Models.Service;

namespace MonopolyBot.Interface
{
    internal interface IAccService
    {
        public Task<AccServiceResponse> GetMyDataAsync(long chatId);
        public Task<string> RegisterAsync(string name, string password);
        public Task<AccServiceResponse> LoginAsync(long chatId, string name, string password);
        public Task<AccServiceResponse> DeleteAccountAsync(string name, string password);
    }
}
