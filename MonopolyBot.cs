using MonopolyBot.Interface;
using MonopolyBot.Interface.IRepository;
using MonopolyBot.Interface.IService;
using MonopolyBot.Models.API.ApiResponse;
using MonopolyBot.Models.ApiResponse;
using MonopolyBot.Models.Bot;
using MonopolyBot.Models.Service;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
                        new KeyboardButton [] {"Register", "Login", "Delete Account"},
                        new KeyboardButton [] { "Profile", "Rooms Menu" }
                        }
                    )
        {
            ResizeKeyboard = true
        };
        ReplyKeyboardMarkup roomsKeyboardMarkup = new ReplyKeyboardMarkup
                    (
                    new[]
                        {
                        new KeyboardButton [] {"Create Room", "View Rooms"},
                        new KeyboardButton [] { "Profile", "Accounts menu" }
                        }
                    )
        {
            ResizeKeyboard = true
        };
        ReplyKeyboardMarkup gameKeyboardMarkup = new ReplyKeyboardMarkup
                    (
                    new[]
                        {
                       new KeyboardButton [] {"Game Status", "Roll Dice"},
                       new KeyboardButton [] {"Buy", "Pay"},
                       new KeyboardButton [] {"Level Up", "Level Down"},
                       new KeyboardButton [] { "End Action", "Leave Game" }
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
            if(data.StartsWith("CreateRoom:"))
            {
                await HandleCallbackCreateRoom(botClient, chatId, data);
            }
            else
            if (data.StartsWith("GameStatus:"))
            {
                await HandleCallbackGameStatus(botClient, chatId, data);
            }
            else
            if (data.StartsWith("ReturnToGame:"))
            {
                await HandleCallbackReturnToGame(botClient, chatId, data);
            }
            else
            if (data.StartsWith("WatchGame:"))
            {
                await HandleCallbackWatchGame(botClient, chatId, data);
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
                case "Profile":
                    await HandleMe(botClient, message);
                    return;
                case "Delete Account":
                    await HandleDeleteAccount(botClient, message);
                    return;
                case "Rooms Menu":
                    await HandleRoomsMenu(botClient, message);
                    return;

                case "Create Room":
                    await HandleCreateRoom(botClient, message);
                    return;
                case "View Rooms":
                    await HandleGetRooms(botClient, message);
                    return;
                case "Accounts menu":
                    await HandleAccountsMenu(botClient, message);
                    return;
                
                case "Game Status":
                    await HandleGameStatus(botClient, message);
                    return;
                case "Roll Dice":
                    await HandleRollDice(botClient, message);
                    return;
                case "Buy":
                    await HandleBuy(botClient, message);
                    return;
                case "Pay":
                    await HandlePay(botClient, message);
                    return;
                case "Level Up":
                    await HandleLevelUp(botClient, message);
                    return;
                case "Level Down":
                    await HandleLevelDown(botClient, message);
                    return;
                case "End Action":
                    await HandleEndAction(botClient, message);
                    return;
                case "Leave Game":
                    await HandleLeaveGame(botClient, message);
                    return;
                case "End Watch":
                    await HandleEndWatchGame(botClient, message);
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
                if (status.IsAwaitingDeleteAccount)
                {
                    await HandleDeleteAccountStatus(botClient, message, status);
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
        private async Task HandleDeleteAccount(ITelegramBotClient botClient, Message message)
        {
            try
            {
                await _chatRepository.InsertChatStatus(new ChatStatus(message.Chat.Id) { IsAwaitingDeleteAccount = true });
                await botClient.SendMessage(message.Chat.Id, "Видалення аккаунту розпочато. Введіть ім'я.");
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
                return;
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при видаленні акаунту: {ex.Message}");
                return;
            }
        }
        private async Task HandleRoomsMenu(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: roomsKeyboardMarkup);
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
            try
            {
                List<RoomDto> rooms = await _roomService.GetRoomsAsync(message.Chat.Id);
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
                                InlineKeyboardButton.WithCallbackData("Return To Game", $"ReturnToGame:{room.RoomId}"),
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
                        else if (room.InGame)
                        {
                            keyboardMarkup = new
                            (
                                InlineKeyboardButton.WithCallbackData("Watch Game", $"WatchGame:{room.RoomId}")
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
            catch(UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
                return;
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при отриманні кімнат: {ex.Message}");
                return;
            }
        }
        private async Task HandleAccountsMenu(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
        }

        private async Task HandleGameStatus(ITelegramBotClient botClient, Message message)
        {
            try
            {
                GameDto game = await _gameService.GameStatusAsync(message.Chat.Id);
                await SendGameStatusMessage(botClient, message.Chat.Id, game);
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
        private async Task HandleRollDice(ITelegramBotClient botClient, Message message)
        {
            try
            {
                MoveDto result = await _gameService.RollDiceAsync(message.Chat.Id);

                string selfMessage;
                string othersMessage;

                string selfDublResult;
                string othersDublResult;

                if(result.Player.LastDiceResult.Dubl && result.Player.CountOfDubles == 3)
                {
                    selfDublResult = "\n💥 Це був ваш третій дубль підряд! Ви відправляєтесь до тюрми.";
                    othersDublResult = "\n💥 Це був його третій дубль підряд! Він відправляється до тюрми.";

                    selfMessage =
                        $"🎲 Ви кинули кубики: {result.Player.LastDiceResult.Dice1} + {result.Player.LastDiceResult.Dice2} = {result.Player.LastDiceResult.DiceSum}.{selfDublResult}\n\n" +
                        "Перевірте статус гри для деталей.";

                    othersMessage =
                        $"🎲 {result.Player.Name} кинув кубики: {result.Player.LastDiceResult.Dice1} + {result.Player.LastDiceResult.Dice2} = {result.Player.LastDiceResult.DiceSum}.{othersDublResult}\n\n" +
                        "Перевірте статус гри для деталей.";
                }
                else
                {
                    selfDublResult = result.Player.LastDiceResult.Dubl ? $"\n🔥 Ви викинули дубль №{result.Player.CountOfDubles}! Маєте додатковий хід" : "";
                    othersDublResult = result.Player.LastDiceResult.Dubl ? $"\n🔥 Викинуто дубль №{result.Player.CountOfDubles}! Гравець має додатковий хід" : "";

                    selfMessage =
                        $"🎲 Ви кинули кубики: {result.Player.LastDiceResult.Dice1} + {result.Player.LastDiceResult.Dice2} = {result.Player.LastDiceResult.DiceSum}.{selfDublResult}\n" +
                        $"Ви пересунулись на клітинку *{result.Cell.Name}* (#{result.Cell.Number}).\n" +
                        $"{result.CellMessage}\n\n" +
                        "Перевірте статус гри для деталей.";

                    othersMessage =
                        $"🎲 {result.Player.Name} кинув кубики: {result.Player.LastDiceResult.Dice1} + {result.Player.LastDiceResult.Dice2} = {result.Player.LastDiceResult.DiceSum}.{othersDublResult}\n" +
                        $"Перейшов на клітинку *{result.Cell.Name}* (#{result.Cell.Number}).\n" +
                        $"{result.CellMessage}\n\n" +
                        "Перевірте статус гри для деталей.";
                }

                await SendMessageToAllPlayersAsync(botClient, message.Chat.Id, selfMessage, othersMessage);
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
                BuyDto result = await _gameService.BuyCellAsync(message.Chat.Id);

                string selfMessage = 
                    $"Ви купили клітину №{result.CellNumber} ({result.CellName}) [{result.CellMonopolyType}] за {result.Price}.\n" +
                    $"Баланс: {result.OldBalance} → {result.NewBalance}.\n" +
                    $"{(result.HasMonopoly ? "🎉 У вас тепер монополія!" : "Монополії ще немає.")}";

                string othersMessage = 
                    $"{result.PlayerName} придбав клітину №{result.CellNumber} ({result.CellName}) [{result.CellMonopolyType}] за {result.Price}. " +
                    $"Баланс: {result.OldBalance} → {result.NewBalance}.\n" +
                    $"{(result.HasMonopoly ? "Тепер у нього монополія!" : "")}";

                await SendMessageToAllPlayersAsync(botClient, message.Chat.Id, selfMessage, othersMessage);
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
                PayDto result = await _gameService.PayAsync(message.Chat.Id);

                string selfMessage = $"Оплата {result.Amount}$ здійснена. Ваш баланс: {result.NewPlayerBalance}$";

                string othersMessage;

                if (result.ReceiverId != null)
                {
                    selfMessage = $"Оплата {result.Amount}$ на рахунок {result.ReceiverName} здійснена.\n" +
                        $"Ваш баланс: {result.NewPlayerBalance}$" +
                        $"Баланс {result.ReceiverName}: {result.NewReceiverBalance}";

                    othersMessage = $"{result.PlayerName} сплатив {result.Amount}$ гравцю {result.ReceiverName}. " +
                        $"{result.PlayerName} баланс: {result.NewPlayerBalance}$, " +
                        $"{result.ReceiverName} баланс: {result.NewReceiverBalance}$";
                }
                else
                {
                    selfMessage = $"Оплата {result.Amount}$ за вихід з тюрми здійснена. Ваш баланс: {result.NewPlayerBalance}$";

                    othersMessage = $"{result.PlayerName} сплатив за вихід з тюрми {result.Amount}$. " +
                        $"{result.PlayerName} баланс: {result.NewPlayerBalance}$";
                }

                await SendMessageToAllPlayersAsync(botClient, message.Chat.Id, selfMessage, othersMessage);
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
                NextActionDto result = await _gameService.EndActionAsync(message.Chat.Id);

                string selfMessage =
                    $"Ваша дія завершена. Наступним ходить {result.NewPlayerName}. Перевірте статус гри";

                string othersMessage =
                    $"{result.PlayerName} завершив свою дію. Наступним ходить {result.NewPlayerName}. Перевірте статус гри";

                await SendMessageToAllPlayersAsync(botClient, message.Chat.Id, selfMessage, othersMessage);
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
        private async Task HandleLeaveGame(ITelegramBotClient botClient, Message message)
        {
            try
            {
                var thisUser = await _userRepository.ReadUserWithChatId(message.Chat.Id);
                LeaveGameDto result = await _gameService.LeaveGameAsync(message.Chat.Id);

                string selfMessage;
                string othersMessage;

                if (result.IsGameOver)
                {
                    selfMessage = $"Ви вийшли з гри.\nГру завершено. Переможець: {result.Winner.Name}.";
                    othersMessage = $"{thisUser.Name} вийшов з гри.\nГру завершено. Переможець: {result.Winner.Name}.";

                    await SendLeaveGameMessageAsync(botClient, message.Chat.Id, thisUser.GameId, selfMessage, othersMessage, roomsKeyboardMarkup);
                }
                else
                {
                    selfMessage = $"Ви вийшли з гри.\nЗалишилось гравців: {result.RemainingPlayers}.";
                    othersMessage = $"{thisUser.Name} вийшов з гри.\nЗалишилось гравців: {result.RemainingPlayers}.";

                    await SendLeaveGameMessageAsync(botClient, message.Chat.Id, thisUser.GameId, selfMessage, othersMessage);
                }
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
        private async Task HandleEndWatchGame(ITelegramBotClient botClient, Message message)
        {
            try
            {
                await _userRepository.UpdateUserGameId(message.Chat.Id, null);
                await botClient.SendMessage(message.Chat.Id, "Ви припинили спостереження за грою.", replyMarkup: roomsKeyboardMarkup);
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(message.Chat.Id, ex.Message);
                await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(message.Chat.Id, $"Помилка при припиненні спостереження за грою: {ex.Message}");
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
        private async Task HandleDeleteAccountStatus(ITelegramBotClient botClient, Message message, ChatStatus status)
        {
            if(status.AccountName == null)
            {
                status.AccountName = message.Text;
                await _chatRepository.UpdateChatStatus(status);
                await botClient.SendMessage(message.Chat.Id, "Введіть пароль для видалення акаунту:");
            }
            else
            {
                AccServiceResponse deleteResult = await _accService.DeleteAccountAsync(status.AccountName, message.Text);
                await botClient.SendMessage(message.Chat.Id, deleteResult.Message);
                await _chatRepository.DeleteChatStatus(message.Chat.Id);
            }
        }
        private async Task HandleJoinRoomStatus(ITelegramBotClient botClient, Message message, ChatStatus status)
        {
            try
            {
                RoomDto roomResponse = await _roomService.JoinRoomAsync(message.Chat.Id, status.RoomId, message.Text);
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
            InlineKeyboardMarkup keyboardMarkup = new
                (
                    InlineKeyboardButton.WithCallbackData("🔐 Створити кімнату з паролем", $"CreateRoom:set"),
                    InlineKeyboardButton.WithCallbackData("🔓 Створити кімнату без пароля", $"CreateRoom:null")
                );

            if (status.MaxNumberOfPlayers == null)
            {
                int maxNumberOfPlayers;
                try
                {
                    maxNumberOfPlayers = Convert.ToInt32(message.Text);
                }
                catch (FormatException)
                {
                    await botClient.SendMessage(message.Chat.Id, "⚠️ Введіть коректне число для максимальної кількості гравців:");
                    return;
                }
                if (maxNumberOfPlayers > 4 || maxNumberOfPlayers < 2)
                {
                    await botClient.SendMessage(message.Chat.Id, "❌ Кількість гравців повинна бути від 2 до 4. Спробуйте ще раз:");
                    return;
                }
                status.MaxNumberOfPlayers = maxNumberOfPlayers;
                await _chatRepository.UpdateChatStatus(status);

                await botClient.SendMessage(message.Chat.Id, "Оберіть тип кімнати:", replyMarkup: keyboardMarkup);
            }
            else
            if (status.IsAwaitingCreateRoomPassword)
            {
                try
                {
                    RoomDto roomResponse = await _roomService.CreateRoomAsync(message.Chat.Id, status.MaxNumberOfPlayers.Value, message.Text);
                    await _userRepository.UpdateUserGameId(message.Chat.Id, roomResponse.RoomId);
                    
                    await botClient.SendMessage(message.Chat.Id, $"Кімната {roomResponse.RoomId} створена.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    await botClient.SendMessage(message.Chat.Id, ex.Message);
                    await botClient.SendMessage(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
                    await _chatRepository.DeleteChatStatus(message.Chat.Id);
                    return;
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(message.Chat.Id, $"Помилка при створенні кімнати: {ex.Message}");
                    await _chatRepository.DeleteChatStatus(message.Chat.Id);
                    return;
                }
            }
            else
            {
                await botClient.SendMessage(message.Chat.Id, "Оберіть тип кімнати:", replyMarkup: keyboardMarkup);
            }
        }
        private async Task HandleLevelUpStatus(ITelegramBotClient botClient, Message message, ChatStatus status)
        {
            try
            {
                int cellNumber = Convert.ToInt32(message.Text);
                LevelChangeDto result = await _gameService.LevelUpCellAsync(message.Chat.Id, cellNumber);

                string selfMessage =
                    $"✅ Ви підвищили рівень клітини №{result.CellNumber} ({result.CellName}) " +
                    $"з {result.OldLevel} до {result.NewLevel}.\n" +
                    $"Ваш баланс: {result.OldPlayerBalance} → {result.NewPlayerBalance}.";

                string othersMessage =
                    $"🔼 {result.PlayerName} підвищив рівень клітини №{result.CellNumber} ({result.CellName}) " +
                    $"з {result.OldLevel} до {result.NewLevel}.\n" +
                    $"Його баланс: {result.OldPlayerBalance} → {result.NewPlayerBalance}.";

                await SendMessageToAllPlayersAsync(botClient, message.Chat.Id, selfMessage, othersMessage);
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
                LevelChangeDto result = await _gameService.LevelDownCellAsync(message.Chat.Id, cellNumber);

                string selfMessage =
                    $"Клітина №{result.CellNumber} ({result.CellName}) знижена з рівня {result.OldLevel} до {result.NewLevel}.\n" +
                    $"Ваш баланс: {result.OldPlayerBalance} → {result.NewPlayerBalance}";

                string othersMessage =
                    $"{result.PlayerName} знизив рівень клітини №{result.CellNumber} ({result.CellName}) " +
                    $"з {result.OldLevel} до {result.NewLevel}.";

                await SendMessageToAllPlayersAsync(botClient, message.Chat.Id, selfMessage, othersMessage);
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
                    RoomDto roomResponse = await _roomService.JoinRoomAsync(chatId, id, null);
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
        private async Task HandleCallbackCreateRoom(ITelegramBotClient botClient, long chatId, string data)
        {
            string passwordStatus = data.Split(':')[1];
            string? password;

            if (passwordStatus == "set")
            {
                await botClient.SendMessage(chatId, "Введіть пароль для кімнати:");

            }
            else
            if (passwordStatus == "null")
            {
                try
                {
                    password = null;

                    ChatStatus status = await _chatRepository.ReadChatStatus(chatId);
                    RoomDto room = await _roomService.CreateRoomAsync(chatId, status.MaxNumberOfPlayers.Value, password);

                    await _userRepository.UpdateUserGameId(chatId, room.RoomId);
                    await botClient.SendMessage(chatId, $"Кімната {room.RoomId} створена.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    await botClient.SendMessage(chatId, ex.Message);
                    await botClient.SendMessage(chatId, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
                    return;
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(chatId, $"Помилка при створенні кімнати: {ex.Message}");
                    return;
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
                GameDto gameStatus = await _gameService.GameStatusAsync(chatId);
                await botClient.SendMessage(chatId, "Ви приєднались до гри", replyMarkup: gameKeyboardMarkup);
                await SendGameStatusMessage(botClient, chatId, gameStatus);
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
        private async Task HandleCallbackReturnToGame(ITelegramBotClient botClient, long chatId, string data)
        {
            string id = data.Split(':')[1];
            try
            {
                GameDto gameResponse = await _gameService.GameStatusAsync(chatId);
                await botClient.SendMessage(chatId, "Ви повернулись до гри", replyMarkup: gameKeyboardMarkup);
                await SendGameStatusMessage(botClient, chatId, gameResponse);
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
        private async Task HandleCallbackWatchGame(ITelegramBotClient botClient, long chatId, string data)
        {
            string id = data.Split(':')[1];
            try
            {
                GameDto gameResponse = await _gameService.GameStatusAsync(chatId);
                await _userRepository.UpdateUserGameId(chatId, id);
                ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup
                            (
                            new[]
                                {
                                    new KeyboardButton [] {"Game Status", "End Watch"},
                                    new KeyboardButton [] { "Profile" }
                                }
                            )
                {
                    ResizeKeyboard = true
                };
                await botClient.SendMessage(chatId, "Ви спостерігаєте за грою", replyMarkup: replyKeyboardMarkup);
                await SendGameStatusMessage(botClient, chatId, gameResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                await botClient.SendMessage(chatId, ex.Message);
                await botClient.SendMessage(chatId, "Виберіть пункт меню:", replyMarkup: loginKeyboardMarkup);
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(chatId, $"Помилка при спостереженні за грою: {ex.Message}");
            }
        }

        private string BuildRoomMessage(RoomDto room)
        {
            string players = "";
            foreach (var player in room.Players)
            {
                players += player.Name + ", ";
            }
            players = players.Substring(0, players.Length - 2);
            string text = $"Кімната {room.RoomId}\n" +
                $"Максимальна кількість гравців - {room.MaxNumberOfPlayers}\n";
            if (room.Players.Count != 0) text += $"Гравці в кімнаті: {players}\n";
            else text += "Гравці в кімнаті: Відсутні\n";
            if (room.HavePassword) text += "Пароль: Встановлено\n";
            else text += "Пароль: Не встановлено\n";
            if (room.InGame) text += "Гра в кімнаті: Так\n";
            else text += "Гра в кімнаті: Ні\n";
            
            return text;
        }

        private async Task SendGameStatusMessage(ITelegramBotClient botClient, long chatId, GameDto game)
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
                    cellInfo = $"<b>{cell.Number}: {cell.Name}</b> - 🌀 <i>Особлива клітина</i>\n";
                    if (playersOnCell.Count > 0)
                        cellInfo += $"👥 <i>Гравці на клітині:</i> {string.Join(", ", playersOnCell)}\n";
                    else
                        cellInfo += "👥 Немає гравців\n";
                }
                else
                {
                    cellInfo = $"<b>{cell.Number}: {cell.Name}</b>\n";
                    
                    string ownerName = "Нікому";
                    if (cell.OwnerId != null)
                    {
                        var owner = game.Players.Find(p => p.Id == cell.OwnerId);
                        if (owner != null)
                            ownerName = owner.Name;
                    }
                    cellInfo += $"Власник: {ownerName}\n";
                    if(cell.IsMonopoly.Value)
                        cellInfo += $"💠 <b>Монополія: {cell.MonopolyType}</b> (активна)\n";
                    else
                        cellInfo += $"💠 <b>Монополія: {cell.MonopolyType}</b> (неактивна)\n";
                    if (cell.OwnerId == null)
                        cellInfo += $"💰 Купівля: <b>{cell.Price}$</b>. Рента: <b>{cell.Rent}$</b>\n";
                    else
                        cellInfo += $"💸 Рента: <b>{cell.Rent}$</b>\n";
                    if (playersOnCell.Count > 0)
                        cellInfo += $"👥 <i>Гравці:</i> {string.Join(", ", playersOnCell)}\n";
                    else
                        cellInfo += "👥 Немає гравців\n";
                    cellInfo += $"📈 Рівень: {cell.Level}\n";
                }

                cellBlock += "\n";

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
                string playerInfo =
                    $"<b>{player.Name}</b> — 💵 <b>{player.Balance}$</b>\n" +
                    $"📍 Клітина: {player.Location} ({game.Cells[player.Location].Name})\n" +
                    (player.LastDiceResult != null ? $"🎲 Кубики: {player.LastDiceResult.Dice1}+{player.LastDiceResult.Dice2} = {player.LastDiceResult.DiceSum}" +
                    (player.LastDiceResult.Dubl ? " (Дубль!)" : "") + "\n" : "") +
                    (player.IsPrisoner ? "🚔 У в’язниці\n" : "") +
                    (player.CantAction > 0 ? $"⏳ Пропускає {player.CantAction} ходів\n" : "") +
                    (player.ReverseMove > 0 ? $"↩️ Рух назад на {player.ReverseMove}\n" : "") +

                    "\n" +

                    (player.HisAction ? "➡️ Зараз його хід\n" : "") +
                    (player.CanMove ? "🎲 Необхідно кинути кубики\n" : "") +
                    (player.NeedPay ? "💸 Треба оплатити борг\n" : "") +
                    (player.CanBuyCell ? "🛒 Може купити клітину\n" : "") +
                    (player.CanLevelUpCell ? "⬆️ Може прокачати клітину\n" : "") +
                    
                    "\n";

                if (playerBlock.Length + playerInfo.Length >= maxMessageLength)
                {
                    playerMessages.Add(playerBlock);
                    playerBlock = playerInfo;
                }
                else
                    playerBlock += playerInfo;
            }
            if (!string.IsNullOrEmpty(playerBlock))
            { 
                playerMessages.Add(playerBlock); 
            }

            foreach (string msg in cellMessages)
            { 
                await botClient.SendMessage(chatId, msg, parseMode: ParseMode.Html); 
            }
            foreach (string msg in playerMessages)
            { 
                await botClient.SendMessage(chatId, msg, parseMode: ParseMode.Html); 
            }
        }
        private async Task SendStartGameMessageAsync(ITelegramBotClient botClient, RoomDto room)
        {
            List<Task> tasks = new List<Task>();
            var usersInGame = await _userRepository.ReadUsersWithGameId(room.RoomId);

            InlineKeyboardMarkup keyboardMarkup = new
                    (
                        InlineKeyboardButton.WithCallbackData("Game Status", $"GameStatus:{room.RoomId}")
                    );

            foreach (var user in usersInGame)
            {
                Task task = botClient.SendMessage(user.ChatId, "Гру в Вашій кімнаті розпочато." +
                    "\nНатисніть кнопку нижче, щоб перейти до гри:", replyMarkup: keyboardMarkup);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
        private async Task SendLeaveGameMessageAsync(ITelegramBotClient botClient, long chatId, string gameId, string selfMessage, string othersMessage)
        {
            var tasks = new List<Task>();
            var usersInGame = await _userRepository.ReadUsersWithGameId(gameId);

            tasks.Add(botClient.SendMessage(chatId, selfMessage, replyMarkup: roomsKeyboardMarkup));

            foreach (var user in usersInGame)
            {
                Task task = botClient.SendMessage(user.ChatId, othersMessage);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
        private async Task SendLeaveGameMessageAsync(ITelegramBotClient botClient, long chatId, string gameId, string selfMessage, string othersMessage, ReplyKeyboardMarkup markup)
        {
            var tasks = new List<Task>();
            var usersInGame = await _userRepository.ReadUsersWithGameId(gameId);

            tasks.Add(botClient.SendMessage(chatId, selfMessage, replyMarkup: roomsKeyboardMarkup));

            foreach (var user in usersInGame)
            {
                Task task = botClient.SendMessage(user.ChatId, othersMessage, replyMarkup: markup);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
        
        private async Task SendMessageToAllPlayersAsync(ITelegramBotClient botClient, long chatId, string selfMessage, string othersMessage)
        {
            List<Task> tasks = new List<Task>();

            var thisUser = await _userRepository.ReadUserWithChatId(chatId);
            var usersInGame = await _userRepository.ReadUsersWithGameId(thisUser.GameId);

            foreach (var user in usersInGame)
            {
                Task task;
                if (user.ChatId != chatId)
                    task = botClient.SendMessage(user.ChatId, othersMessage);
                else
                    task = botClient.SendMessage(user.ChatId, selfMessage);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
    }
}