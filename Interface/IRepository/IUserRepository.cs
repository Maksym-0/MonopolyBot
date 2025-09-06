using MonopolyBot.Models.Bot;

namespace MonopolyBot.Interface.IRepository
{
    internal interface IUserRepository
    {
        public Task InsertUser(User user);
        public Task<User> ReadUserWithChatId(long chatId);
        public Task<User> ReadUserWithId(string userId);
        Task<List<User>> ReadUsersWithGameId(string gameId);
        public Task UpdateUserGameIdWithChatId(long chatId, string? gameId);
        public Task UpdateUserGameIdWithUserId(string userId, string? gameId);
        public Task DeleteUserWithChatId(long chatId);
        public Task DeleteUserWithId(string userId);

        public Task<bool> SearchUserByChatId(long chatId);
    }
}
