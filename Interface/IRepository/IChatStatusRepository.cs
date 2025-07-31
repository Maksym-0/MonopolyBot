using MonopolyBot.Models.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot.Interface.IRepository
{
    internal interface IChatStatusRepository
    {
        public Task InsertChatStatus(ChatStatus status);
        public Task<ChatStatus?> ReadChatStatus(long chatId);
        public Task UpdateChatStatus(ChatStatus status);
        public Task DeleteChatStatus(long chatId);
    }
}
