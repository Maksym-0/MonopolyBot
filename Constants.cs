using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonopolyBot
{
    internal static class Constants
    {
        public static string BotID = Environment.GetEnvironmentVariable("BOT_TOKEN");
        
        public static string ApiAddress = Environment.GetEnvironmentVariable("API_ADDRESS") ?? "https://localhost:7265";
        public static string ApiAccouuntHost = Environment.GetEnvironmentVariable("API_ACCOUNT_HOST") ?? "/api/account";
        public static string ApiRoomHost = Environment.GetEnvironmentVariable("API_ROOM_HOST") ?? "/api/rooms";
        public static string ApiGameHost = Environment.GetEnvironmentVariable("API_GAME_HOST") ?? "/api/game";

        public static string DBConnect = Environment.GetEnvironmentVariable("DB_CONNECT") ?? "Host=localhost;Port=5432;Username=postgres;Password=123456;Database=postgres";
        public static string UserTable = Environment.GetEnvironmentVariable("USER_DB_TABLE") ?? "UserData";
        public static string ChatStatusTable = Environment.GetEnvironmentVariable("CHAT_STATUS_DB_TABLE") ?? "СhatStatusData";
    }
}
