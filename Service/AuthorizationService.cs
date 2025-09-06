using MonopolyBot.Interface.IRepository;
using MonopolyBot.Interface.IService;
using MonopolyBot.Models.Bot;
using MonopolyBot.Models.Service;

namespace MonopolyBot.Service
{
    internal class AuthorizationService : IAuthorization
    {
        IUserRepository _userRepository;

        public AuthorizationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<AuthorizationResult> GetAuthorizationResultAsync(long chatId)
        {
            if (await _userRepository.SearchUserByChatId(chatId))
            {
                User user = await _userRepository.ReadUserWithChatId(chatId);
                if (user.ExpiresAt < DateTime.Now)
                {
                    await _userRepository.DeleteUserWithChatId(chatId);
                    return new AuthorizationResult()
                    {
                        IsAuthorized = false,
                        Message = "Час авторизації вичерпано. Увійдіть в систему"
                    };
                }
                else
                    return new AuthorizationResult()
                    {
                        IsAuthorized = true,
                        Message = "Авторизація успішна",
                        User = user
                    };
            }
            else
            {
                return new AuthorizationResult()
                {
                    IsAuthorized = false,
                    Message = "Користувач не знайдений. Увійдіть в систему"
                };
            }
        }
        public async Task<User> GetAuthorizedUserAsync(long chatId)
        {
            var data = await GetAuthorizationResultAsync(chatId);
            if (!data.IsAuthorized)
                throw new UnauthorizedAccessException(data.Message);
            return data.User;
        }
    }
}
