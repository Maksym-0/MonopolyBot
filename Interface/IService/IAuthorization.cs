using MonopolyBot.Models.Bot;
using MonopolyBot.Models.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Interface.IService
{
    internal interface IAuthorization
    {
        Task<AuthorizationResult> GetAuthorizationResultAsync(long chatId);
        Task<User> GetAuthorizedUserAsync(long chatId);
    }
}
