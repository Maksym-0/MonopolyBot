using MonopolyBot.Models.API.Request;
using MonopolyBot.Interface;
using MonopolyBot.Clients;
using System.IdentityModel.Tokens.Jwt;
using MonopolyBot.Interface.IService;
using MonopolyBot.Interface.IClient;
using MonopolyBot.Interface.IRepository;
using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Models.Service;
using MonopolyBot.Models.Bot;

namespace MonopolyBot.Service
{
    internal class AccountService : IAccService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountClient _accountClient;
        private readonly IAuthorization _authorization;

        public AccountService(IUserRepository userRepository, IAccountClient accountClient, IAuthorization authorization)
        {
            _userRepository = userRepository;
            _accountClient = accountClient;
            _authorization = authorization;
        }

        public async Task<AccServiceResponse> GetMyDataAsync(long chatId)
        {
            User user = await _authorization.GetAuthorizedUserAsync(chatId);

            return new AccServiceResponse()
            {
                Success = true,
                Message = "Ви авторизовані",
                Name = user.Name,
                Id = user.UserId
            };
        }
        public async Task<string> RegisterAsync(string name, string password)
        {
            AccountRequest account = new AccountRequest()
            {
                Name = name,
                Password = password
            };
            var data = await _accountClient.RegisterAsync(account);

            if (data.Success)
                return "Реєстрація успішна";
            else
                return $"Помилка реєстрації: {data.Message}";
        }
        public async Task<AccServiceResponse> LoginAsync(long chatId, string name, string password)
        {
            AccountRequest account = new AccountRequest()
            {
                Name = name,
                Password = password
            };
            var data = await _accountClient.LoginAndReturnJWTAsync(account);

            if (data == null || string.IsNullOrEmpty(data.Data))
            {
                return new AccServiceResponse()
                {
                    Success = false,
                    Message = "Помилка авторизації: не отримано JWT"
                };
            }
            if (data.Success)
            {
                if(await _userRepository.SearchUserByChatId(chatId))
                {
                    await _userRepository.DeleteUserWithChatId(chatId);
                }
                ApiResponse<AccountDto> accountResponse = await _accountClient.MeAsync(data.Data);
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(data.Data);
                await _userRepository.InsertUser(new User()
                {
                    ChatId = chatId,
                    UserId = accountResponse.Data.Id,
                    GameId = null,
                    Name = name,
                    JWT = data.Data,
                    CreatedAt = DateTimeOffset.FromUnixTimeSeconds(token.Payload.Iat.Value).DateTime,
                    ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(token.Payload.Exp.Value).DateTime
                });
                return new AccServiceResponse()
                {
                    Success = data.Success,
                    Message = data.Message,
                    Name = name,
                    Id = accountResponse.Data.Id,
                };
            }

            return new AccServiceResponse()
            {
                Success = data.Success,
                Message = data.Message
            };
        }
        public async Task<AccServiceResponse> DeleteAccountAsync(string name, string password)
        {
            AccountRequest account = new AccountRequest()
            {
                Name = name,
                Password = password
            };
            var data = await _accountClient.DeleteAccount(account);

            if (data.Success)
            {
                return new AccServiceResponse()
                {
                    Success = data.Success,
                    Message = data.Message
                };
            }
            else
            {
                return new AccServiceResponse()
                {
                    Success = data.Success,
                    Message = data.Message
                };
            }
        }
    }
}
