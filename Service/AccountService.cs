using MonopolyBot.Models.API.Request;
using MonopolyBot.Interface;
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
        public async Task<AccountDto> RegisterAsync(string name, string password)
        {
            AccountRequest account = new AccountRequest()
            {
                Name = name,
                Password = password
            };
            ApiResponse<AccountDto> data = await _accountClient.RegisterAsync(account);

            if (!data.Success)
                throw new Exception(data.Message);
            return data.Data;
        }
        public async Task<AccountDto> LoginAsync(long chatId, string name, string password)
        {
            AccountRequest account = new AccountRequest()
            {
                Name = name,
                Password = password
            };
            ApiResponse<LoginDto> data = await _accountClient.LoginAndReturnJWTAsync(account);

            if (data.Success)
            {
                if(await _userRepository.SearchUserByChatId(chatId))
                {
                    await _userRepository.DeleteUserWithChatId(chatId);
                }
                
                await _userRepository.InsertUser(new User()
                {
                    ChatId = chatId,
                    UserId = data.Data.Account.Id,
                    GameId = null,
                    Name = name,
                    JWT = data.Data.Token,
                    CreatedAt = data.Data.CreatedAt,
                    ExpiresAt = data.Data.ExpiresAt
                });
                return data.Data.Account;
            }
            else
            {
                throw new Exception(data.Message);
            }
        }
        public async Task<DeleteAccountDto> DeleteAccountAsync(string name, string password)
        {
            AccountRequest account = new AccountRequest()
            {
                Name = name,
                Password = password
            };
            ApiResponse<DeleteAccountDto> data = await _accountClient.DeleteAccount(account);

            if (data.Success)
            {
                return data.Data;
            }
            else
            {
                throw new Exception(data.Message);
            }
        }
    }
}
