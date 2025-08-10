using Telegram.Bot.Polling;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MonopolyBot.Interface.IService;
using MonopolyBot.Interface.IRepository;
using MonopolyBot.Interface;
using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Models.Bot;
using MonopolyBot.Models.Service;
using System.Text;

namespace MonopolyBot
{
    internal class MonopolyBot
    {
        TelegramBotClient botClient = new TelegramBotClient(Constants.BotID);
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions();

        ReplyKeyboardMarkup loginKeyboardMarkup = new ReplyKeyboardMarkup
                    (
                    new[]
                        {
                        new KeyboardButton [] {"Register", "Login"},
                        new KeyboardButton [] { "Me" }
                        }
                    )
        {
            ResizeKeyboard = true
        };
        ReplyKeyboardMarkup roomsKeyboardMarkup = new ReplyKeyboardMarkup
                    (
                    new[]
                        {
                        new KeyboardButton [] {"CreateRoom", "GetRooms"},
                        new KeyboardButton [] { "Me" }
                        }
                    )
        {
            ResizeKeyboard = true
        };
        ReplyKeyboardMarkup gameKeyboardMarkup = new ReplyKeyboardMarkup
                    (
                    new[]
                        {
                       new KeyboardButton [] {"GameStatus", "RollDices"},
                       new KeyboardButton [] {"Buy", "Pay"},
                       new KeyboardButton [] {"LevelUp", "LevelDown"},
                       new KeyboardButton [] { "EndAction", "LeaveGame" }
                        }
                    )
        {
            ResizeKeyboard = true
        };

        IAccService _accService;
        IRoomService _roomService;
        IGameService _gameService;
        IUserRepository _userRepository;
        IChatStatusRepository _chatRepository;

        public MonopolyBot(IAccService accService, IRoomService roomService, IGameService gameService, 
            IUserRepository userRepository, IChatStatusRepository chatRepository)
        {
            _accService = accService;
            _gameService = gameService;
            _roomService = roomService;
            _userRepository = userRepository;
            _chatRepository = chatRepository;
        }

        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMe();
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
            await Task.Delay(-1);
        }
        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellation)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПІ:\n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellation)
        {
            if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                await HandlerMessageAsync(botClient, update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                await HandlerCallbackAsync(botClient, update.CallbackQuery);
            }
        }

        private async Task HandlerCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var data = callbackQuery.Data;

            if (data.StartsWith("JoinRoom:"))
            {
                await HandleCallbackJoinRoom(botClient, chatId, data);
            }
            else
            if (data.StartsWith("LeaveRoom:"))
            {
                await HandleCallbackLeaveRoom(botClient, chatId, data);
            }
            else 
            if (data.StartsWith("GameStatus:"))
            {
                await HandleCallbackGameStatus(botClient, chatId, data);
            }
            else
            if (data.StartsWith("ReturnGame:"))
            {
                await HandleCallbackReturnGame(botClient, chatId, data);
            }
        }

        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            switch (message.Text)
            {
                case "/start":
                    await HandleStart(botClient, message);
                    return;
                case "Register":
                    await HandleRegister(botClient, message);
                    return;
                case "Login":
                    await HandleLogin(botClient, message);
                    return;
                case "Me":
                    await HandleMe(botClient, message);
                    return;
                
                case "CreateRoom":
                    await HandleCreateRoom(botClient, message);
                    return;
                case "GetRooms":
                    await HandleGetRooms(botClient, message);
                    return;

                case "GameStatus":
                    await HandleGameStatus(botClient, message);
                    return;
                case "LeaveGame":
                    await HandleLeaveGame(botClient, message);
                    return;
                case "RollDices":
                    await HandleRollDices(botClient, message);
                    return;
                case "Buy":
                    await HandleBuy(botClient, message);
                    return;
                case "Pay":
                    await HandlePay(botClient, message);
                    return;
                case "LevelUp":
                    await HandleLevelUp(botClient, message);
                    return;
                case "LevelDown":
                    await HandleLevelDown(botClient, message);
                    return;
                case "EndAction":
                    await HandleEndAction(botClient, message);
                    return;
            }
            
            ChatStatus? status = await _chatRepository.ReadChatStatus(message.Chat.Id);
            if (status != null)
            {
                if (status.IsAwaitingLogin)
                {
                    await HandleLoginStatus(botClient, message, status);
                    return;
                }
                else
                if (status.IsAwaitingRegister)
                {
                    await HandleRegisterStatus(botClient, message, status);
                    return;
                }
                else
                if (status.IsAwaitingJoinRoom)
                {
                    await HandleJoinRoomStatus(botClient, message, status);
                    return;
                }
                else
                if (status.IsAwaitingCreateRoom)
                {
                    await HandleCreateRoomStatus(botClient, message, status);
                    return;
                }
                else
                if (status.IsAwaitingLevelUpCell)
                {
                    await HandleLevelUpStatus(botClient, message, status);
                    return;
                }
                else
                if (status.IsAwaitingLevelDownCell)
                {
                    await HandleLevelDownStatus(botClient, message, status);
                    return;
                }
            }

            await botClient.SendMessage(message.Chat.Id, "Невідома команда. Спробуйте ще раз.");
        }

        private async Task HandleStart(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendMessage(message.Chat.Id, "Вітаємо! Це бот для гри в Монополію. Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
        }
        private async Task HandleRegister(ITelegramBotClient botClient, Message message)
        {
            await _chatRepository.InsertChatStatus(new ChatStatus(message.Chat.Id) { IsAwaitingRegister = true });
            await botClient.SendMessage(message.Chat.Id, "Реєстрацію розпочато. Введіть ім'я:");
        }
        private async Task HandleLogin(ITelegramBotClient botClient, Message message)
        {
            await _chatRepository.InsertChatStatus(new ChatStatus(message.Chat.Id) { IsAwaitingLogin = true });
            await botClient.SendMessage(message.Chat.Id, "Вхід в обліковий запис розпочато. Введіть ім'я:");
        }
        private async Task HandleMe(ITelegramBotClient botClient, Message message)
        {
            try
            {
                var data = await _accService.GetMyDataAsync(message.Chat.Id);
                await botClient.SendMessage(message.Chat.Id,
                    $"Ваше ID: {data.Id}\n" +
                    $"Ваше ім'я: {data.Name}");
            }
            catch(UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch(Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при отриманні даних: {ex.Message}");
            }
        }

        private async Task HandleCreateRoom(ITelegramBotClient botClient, Message message)
        {
            await _chatRepository.InsertChatStatus(new ChatStatus(message.Chat.Id) { IsAwaitingCreateRoom = true });
            await botClient.SendMessage(message.Chat.Id, "Створення кімнати розпочато. Введіть максимальну кількість гравців (2-4):");
        }
        private async Task HandleGetRooms(ITelegramBotClient botClient, Message message)
        {
            List<RoomResponse> rooms = await _roomService.GetRoomsAsync(message.Chat.Id);
            if (rooms.Count == 0)
            {
                await botClient.SendMessage(message.Chat.Id, "Немає доступних кімнат.");
            }
            else
            {
                await botClient.SendMessage(message.Chat.Id, "Доступні кімнати:");
                AccServiceResponse account = await _accService.GetMyDataAsync(message.Chat.Id);
                foreach (var room in rooms)
                {
                    bool playerInside = false;
                    InlineKeyboardMarkup keyboardMarkup;
                    foreach (var player in room.Players)
                    {
                        if (player.Id == account.Id)
                            playerInside = true;
                    }
                    if (playerInside && room.InGame)
                    {
                        keyboardMarkup = new
                        (
                            InlineKeyboardButton.WithCallbackData("ReturnGame", $"ReturnGame:{room.RoomId}"),
                            InlineKeyboardButton.WithCallbackData("Leave", $"LeaveRoom:{room.RoomId}")
                        );
                    }
                    else if (playerInside)
                    {
                        keyboardMarkup = new
                        (
                            InlineKeyboardButton.WithCallbackData("Leave", $"LeaveRoom:{room.RoomId}")
                        );
                    }
                    else
                    {
                        keyboardMarkup = new
                        (
                            InlineKeyboardButton.WithCallbackData("Join", $"JoinRoom:{room.RoomId}:{room.HavePassword}")
                        );
                    }
                    string text = BuildRoomMessage(room);
                    await botClient.SendMessage(message.Chat.Id, text, replyMarkup: keyboardMarkup);
                }
            }
        }

        private async Task HandleGameStatus(ITelegramBotClient botClient, Message message)
        {
            try
            {
                GameResponse game = await _gameService.GameStatusAsync(message.Chat.Id);
                await SendGameMessage(botClient, message.Chat.Id, game);
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при отриманні статусу гри: {ex.Message}");
            }
        }
        private async Task HandleLeaveGame(ITelegramBotClient botClient, Message message)
        {
            try
            {
                bool result = await _gameService.LeaveGameAsync(message.Chat.Id);
                await botClient.SendMessage(message.Chat.Id, "Ви вийшли з гри.");
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при виході з гри: {ex.Message}");
            }
        }
        private async Task HandleRollDices(ITelegramBotClient botClient, Message message)
        {
            try
            {
                await _gameService.RollDicesAsync(message.Chat.Id);
                await botClient.SendMessage(message.Chat.Id, "Кубики кинуто. Перевірте статус гри для отримання результатів.");
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при кидку кубиків: {ex.Message}");
            }
        }
        private async Task HandleBuy(ITelegramBotClient botClient, Message message)
        {
            try
            {
                await _gameService.BuyCellAsync(message.Chat.Id);
                await botClient.SendMessage(message.Chat.Id, "Клітинка куплена. Перевірте статус гри для отримання результатів.");
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при купівлі клітини: {ex.Message}");
            }
        }
        private async Task HandlePay(ITelegramBotClient botClient, Message message)
        {
            try
            {
                await _gameService.PayAsync(message.Chat.Id);
                await botClient.SendMessage(message.Chat.Id, "Оплата здійснена. Перевірте статус гри для отримання результатів.");
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при оплаті: {ex.Message}");
            }
        }
        private async Task HandleLevelUp(ITelegramBotClient botClient, Message message)
        {
            try
            {
                await _chatRepository.InsertChatStatus(new ChatStatus(message.Chat.Id) { IsAwaitingLevelUpCell = true });
                await botClient.SendMessage(message.Chat.Id, "Підвищення рівня клітини розпочато. Введіть номер клітини для підвищення рівня:");
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при підвищенні рівня клітини: {ex.Message}");
            }
        }
        private async Task HandleLevelDown(ITelegramBotClient botClient, Message message)
        {
            try
            {
                await _chatRepository.InsertChatStatus(new ChatStatus(message.Chat.Id) { IsAwaitingLevelDownCell = true });
                await botClient.SendMessage(message.Chat.Id, "Зниження рівня клітини розпочато. Введіть номер клітини для зниження рівня:");
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при зниженні рівня клітини: {ex.Message}");
            }
        }
        private async Task HandleEndAction(ITelegramBotClient botClient, Message message)
        {
            try
            {
                await _gameService.EndActionAsync(message.Chat.Id);
                await SendEndActionMessageAsync(botClient, message.Chat.Id);
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при завершенні дії: {ex.Message}");
            }
        }

        private async Task HandleLoginStatus(ITelegramBotClient botClient, Message message, ChatStatus status)
        {
            if (status.AccountName == null)
            {
                status.AccountName = message.Text;
                await _chatRepository.UpdateChatStatus(status);
                await botClient.SendMessage(message.Chat.Id, "Введіть пароль для входу:");
            }
            else
            {
                AccServiceResponse loginData = await _accService.LoginAsync(message.Chat.Id, status.AccountName, message.Text);
                if (loginData.Success)
                {
                    await botClient.SendMessage(message.Chat.Id, "Ви увійшли в систему.");
                    await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: roomsKeyboardMarkup);
                    await _chatRepository.DeleteChatStatus(message.Chat.Id);
                }
                else
                {
                    await botClient.SendMessage(message.Chat.Id, "Помилка при вході в систему");
                    await botClient.SendMessage(message.Chat.Id, loginData.Message);
                    await _chatRepository.DeleteChatStatus(message.Chat.Id);
                }
            }
        }
        private async Task HandleRegisterStatus(ITelegramBotClient botClient, Message message, ChatStatus status)
        {
            if (status.AccountName == null)
            {
                status.AccountName = message.Text;
                await _chatRepository.UpdateChatStatus(status);
                await botClient.SendMessage(message.Chat.Id, "Введіть пароль для реєстрації:");
            }
            else
            {
                string registerResult = await _accService.RegisterAsync(status.AccountName, message.Text);
                await botClient.SendMessage(message.Chat.Id, registerResult);
                await _chatRepository.DeleteChatStatus(message.Chat.Id);
            }
        }
        private async Task HandleJoinRoomStatus(ITelegramBotClient botClient, Message message, ChatStatus status)
        {
            try
            {
                RoomResponse roomResponse = await _roomService.JoinRoomAsync(message.Chat.Id, status.RoomId, message.Text);
                await botClient.SendMessage(message.Chat.Id, $"Ви приєдналися до кімнати {roomResponse.RoomId}.");
                if (roomResponse.InGame)
                {
                    await SendStartGameMessageAsync(botClient, roomResponse);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при приєднанні до кімнати: {ex.Message}");
            }
            await _chatRepository.DeleteChatStatus(message.Chat.Id);
        }
        private async Task HandleCreateRoomStatus(ITelegramBotClient botClient, Message message, ChatStatus status)
        {
            if (status.MaxNumberOfPlayers == null)
            {
                int maxNumberOfPlayers;
                try
                {
                    maxNumberOfPlayers = Convert.ToInt32(message.Text);
                }
                catch (FormatException)
                {
                    await botClient.SendMessage(message.Chat.Id, "Будь ласка, введіть коректне число для максимальної кількості гравців:");
                    return;
                }
                if (maxNumberOfPlayers > 4 || maxNumberOfPlayers < 2)
                {
                    await botClient.SendMessage(message.Chat.Id, "Максимальна кількість гравців повинна бути від 2 до 4. Спробуйте ще раз:");
                    return;
                }
                status.MaxNumberOfPlayers = maxNumberOfPlayers;
                await _chatRepository.UpdateChatStatus(status);
                await botClient.SendMessage(message.Chat.Id, "Введіть пароль для кімнати або null:");
            }
            else
            {
                string? password;
                if (message.Text == "null")
                    password = null;
                else
                    password = message.Text;
                try
                {
                    RoomResponse roomResponse = await _roomService.CreateRoomAsync(message.Chat.Id, status.MaxNumberOfPlayers ?? 2, password);
                    await _userRepository.UpdateUserGameId(message.Chat.Id, roomResponse.RoomId);
                    await botClient.SendMessage(message.Chat.Id, $"Кімната {roomResponse.RoomId} створена.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    await botClient.SendMessage(message.Chat.Id, ex.Message);
                    await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(message.Chat.Id, $"Помилка при створенні кімнати: {ex.Message}");
                }
                await _chatRepository.DeleteChatStatus(message.Chat.Id);
            }
        }
        private async Task HandleLevelUpStatus(ITelegramBotClient botClient, Message message, ChatStatus status)
        {
            try
            {
                int cellNumber = Convert.ToInt32(message.Text);
                bool result = await _gameService.LevelUpCellAsync(message.Chat.Id, cellNumber);
                if (result)
                {
                    await botClient.SendMessage(message.Chat.Id, "Рівень клітини підвищено.");
                }
                else
                {
                    await botClient.SendMessage(message.Chat.Id, "Не вдалося підвищити рівень клітини. Перевірте статус гри для отримання результатів.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при підвищенні рівня клітини: {ex.Message}");
            }
            await _chatRepository.DeleteChatStatus(message.Chat.Id);
        }
        private async Task HandleLevelDownStatus(ITelegramBotClient botClient, Message message, ChatStatus status)
        {
            try
            {
                int cellNumber = Convert.ToInt32(message.Text);
                bool result = await _gameService.LevelDownCellAsync(message.Chat.Id, cellNumber);
                if (result)
                {
                    await botClient.SendMessage(message.Chat.Id, "Рівень клітини знижено.");
                }
                else
                {
                    await botClient.SendMessage(message.Chat.Id, "Не вдалося знизити рівень клітини. Перевірте статус гри для отримання результатів.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при зниженні рівня клітини: {ex.Message}");
            }
            await _chatRepository.DeleteChatStatus(message.Chat.Id);
        }

        private async Task HandleCallbackJoinRoom(ITelegramBotClient botClient, long chatId, string data)
        {
            string id = data.Split(':')[1];
            string passwordStatus = data.Split(':')[2];
            if (passwordStatus == "True")
            {
                await _chatRepository.InsertChatStatus(new ChatStatus(chatId) { IsAwaitingJoinRoom = true, RoomId = id });
                await botClient.SendMessage(chatId, "Введіть пароль для приєднання до кімнати:");
            }
            else
            {
                try
                {
                    RoomResponse roomResponse = await _roomService.JoinRoomAsync(chatId, id, null);
                    await _userRepository.UpdateUserGameId(chatId, id);
                    await botClient.SendMessage(chatId, $"Ви приєдналися до кімнати {roomResponse.RoomId}.");
                    if (roomResponse.InGame == true)
                    {
                        await SendStartGameMessageAsync(botClient, roomResponse);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    await botClient.SendMessage(chatId, ex.Message);
                    await botClient.SendMessage(chatId, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(chatId, $"Помилка при приєднанні до кімнати: {ex.Message}");
                }
            }
        }
        private async Task HandleCallbackLeaveRoom(ITelegramBotClient botClient, long chatId, string data)
        {
            string id = data.Split(':')[1];
            try
            {
                string result = await _roomService.QuitRoomAsync(chatId);
                await botClient.SendMessage(chatId, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(chatId, ex.Message);
                await botClient.SendMessage(chatId, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(chatId, $"Помилка при виході з кімнати: {ex.Message}");
            }
        }
        private async Task HandleCallbackGameStatus(ITelegramBotClient botClient, long chatId, string data)
        {
            try
            {
                GameResponse gameStatus = await _gameService.GameStatusAsync(chatId);
                await botClient.SendMessage(chatId, "Ви приєднались до гри", replyMarkup: gameKeyboardMarkup);
                await SendGameMessage(botClient, chatId, gameStatus);
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(chatId, ex.Message);
                await botClient.SendMessage(chatId, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(chatId, $"Помилка при отриманні статусу гри: {ex.Message}");
            }
        }
        private async Task HandleCallbackReturnGame(ITelegramBotClient botClient, long chatId, string data)
        {
            string id = data.Split(':')[1];
            try
            {
                GameResponse gameResponse = await _gameService.GameStatusAsync(chatId);
                await botClient.SendMessage(chatId, "Ви повернулись до гри", replyMarkup: gameKeyboardMarkup);
                await SendGameMessage(botClient, chatId, gameResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(chatId, ex.Message);
                await botClient.SendMessage(chatId, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(chatId, $"Помилка при поверненні до гри: {ex.Message}");
            }
        }

        private string BuildRoomMessage(RoomResponse room)
        {
            string players = "";
            foreach (var player in room.Players)
            {
                players += player.Name + ", ";
            }
            players = players.Substring(0, players.Length - 2);
            string text = $"Кімната {room.RoomId}\n" +
                $"Максимальна кількість гравців - {room.MaxNumberOfPlayers}\n" +
                $"Гравці в кімнаті: {players}\n" +
                $"Пароль: {room.HavePassword}\n" +
                $"В грі: {room.InGame}";
            return text;
        }
        private async Task SendGameMessage(ITelegramBotClient botClient, long chatId, GameResponse game)
        {
            const int maxMessageLength = 4000;

            List<string> cellMessages = new List<string>();
            List<string> playerMessages = new List<string>();

            string cellBlock = "";
            List<string> playersOnCell;
            foreach (var cell in game.Cells)
            {
                playersOnCell = new List<string>();
                foreach (var player in game.Players)
                {
                    if (player.Location == cell.Number)
                        playersOnCell.Add(player.Name);
                }

                string cellInfo = "";
                if (cell.Unique)
                {
                    cellInfo = $"{cell.Number}: {cell.Name} - Особлива клітина: {cell.Unique}\n";
                    if (playersOnCell.Count > 0)
                        cellInfo += $"Гравці на клітині: {string.Join(", ", playersOnCell)}\n";
                    else
                        cellInfo += "Гравців на клітині немає.\n";
                }
                else
                {
                    cellInfo = $"{cell.Number}: {cell.Name} - Належить: {cell.Owner ?? "Нікому"}\n";
                    if (cell.Owner == null)
                        cellInfo += $"Вартість придбання: {cell.Price}$. Орендна плата: {cell.Rent}$\n";
                    else
                        cellInfo += $"Орендна плата: {cell.Rent}$\n";
                    if (playersOnCell.Count > 0)
                        cellInfo += $"Гравці на клітині: {string.Join(", ", playersOnCell)}\n";
                    else
                        cellInfo += "Гравців на клітині немає.\n";
                    cellInfo += $"Рівень: {cell.Level}\n";
                }
                
                if(cellBlock.Length + cellInfo.Length > maxMessageLength)
                {
                    cellMessages.Add(cellBlock);
                    cellBlock = cellInfo;
                }
                else
                    cellBlock += cellInfo;
            }
            if(!string.IsNullOrEmpty(cellBlock))
                cellMessages.Add(cellBlock);

            string playerBlock = "";
            foreach (var player in game.Players)
            {
                string playerInfo = $"{player.Name} - {player.Balance}$. В грі: {player.InGame}\n" +
                    $"Статус ходу: {player.HisAction}\n" +
                    $"Клітина перебування: {player.Location}\n\n";

                if(playerBlock.Length + playerInfo.Length >= maxMessageLength)
                {
                    playerMessages.Add(playerBlock);
                    playerBlock = playerInfo;
                }
                else
                    playerBlock += playerInfo;
            }
            if(!string.IsNullOrEmpty(playerBlock))
                playerMessages.Add(playerBlock);

            foreach (string msg in cellMessages)
                await botClient.SendMessage(chatId, msg);

            foreach (string msg in playerMessages)
                await botClient.SendMessage(chatId, msg);
        }
        private async Task SendStartGameMessageAsync(ITelegramBotClient botClient, RoomResponse room)
        {
            List<Task> tasks = new List<Task>();
            foreach(var player in room.Players)
            {
                var user = await _userRepository.ReadUserWithId(player.Id);
                InlineKeyboardMarkup keyboardMarkup = new
                            (
                                InlineKeyboardButton.WithCallbackData("GameStatus", $"GameStatus:{room.RoomId}")
                            );
                Task task = botClient.SendMessage(user.ChatId, "Гру в Вашій кімнаті розпочато." +
                    "\nНатисність кнопку нижче, щоб перейти до гри:", replyMarkup: keyboardMarkup);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
        private async Task SendEndActionMessageAsync(ITelegramBotClient botClient, long chatId)
        {
            List<Task> tasks = new List<Task>();

            var thisUser = await _userRepository.ReadUserWithChatId(chatId);
            GameResponse game = await _gameService.GameStatusAsync(chatId);

            foreach (var player in game.Players)
            {
                Task task;
                var user = await _userRepository.ReadUserWithId(player.Id);
                if(user.ChatId == chatId)
                    task = botClient.SendMessage(user.ChatId, "Ваша дія завершена. Перевірте статус гри для отримання результатів.", replyMarkup: gameKeyboardMarkup);
                else
                    task = botClient.SendMessage( user.ChatId, $"{thisUser.Name} завершив свою дію. Перевірте статус гри для отримання результатів.", replyMarkup: gameKeyboardMarkup);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
    }
}