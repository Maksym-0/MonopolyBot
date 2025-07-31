using MonopolyBot.Models.Bot;

namespace MonopolyBot.Interface.IRepository
{
    internal interface IUserRepository
    {
        public Task InsertUser(User user);
        public Task<User> ReadUserWithChatId(long chatId);
        public Task<User> ReadUserWithId(string userId);
        public Task UpdateUserGameId(User user);
        public Task DeleteUserWithChatId(long chatId);
        public Task DeleteUserWithId(string userId);

        public Task<bool> SearchUserByChatId(long chatId);
    }
}
