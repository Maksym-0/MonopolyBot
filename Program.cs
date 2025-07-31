using Microsoft.Extensions.DependencyInjection;
using MonopolyBot.Clients;
using MonopolyBot.Database;
using MonopolyBot.Interface;
using MonopolyBot.Interface.IClient;
using MonopolyBot.Interface.IRepository;
using MonopolyBot.Interface.IService;
using MonopolyBot.Service;

var service = new ServiceCollection();

service.AddSingleton<IAccountClient, MonopolyClient>();
service.AddSingleton<IRoomClient, MonopolyClient>();
service.AddSingleton<IGameClient, MonopolyClient>();

service.AddSingleton<IUserRepository, UsersDB>();
service.AddSingleton<IChatStatusRepository, ChatDB>();

service.AddSingleton<IAuthorization, AuthorizationService>();
service.AddSingleton<IAccService, AccountService>();
service.AddSingleton<IRoomService, RoomService>();
service.AddSingleton<IGameService, GameService>();

service.AddSingleton<MonopolyBot.MonopolyBot>();

var provider = service.BuildServiceProvider();
var monopolyBot = provider.GetRequiredService<MonopolyBot.MonopolyBot>();
await monopolyBot.Start();