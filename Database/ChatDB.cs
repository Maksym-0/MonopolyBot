using MonopolyBot.Interface.IRepository;
using MonopolyBot.Models.Bot;
using Npgsql;

namespace MonopolyBot.Database
{
    internal class ChatDB : IChatStatusRepository
    {
        public async Task InsertChatStatus(ChatStatus status)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.ChatStatusTable}\" (\"ChatId\", \"IsAwaitingLogin\", \"IsAwaitingRegister\", \"IsAwaitingJoinRoom\", \"IsAwaitingCreateRoom\", \"IsAwaitingLevelUpCell\", \"IsAwaitingLevelDownCell\", \"AccountName\", \"RoomId\", \"MaxNumberOfPlayers\") " +
                $"VALUES (@chatId, @isAwaitingLogin, @isAwaitingRegister, @isAwaitingJoinRoom, @isAwaitingCreateRoom, @isAwaitingevelUpCell, @isAwaitingLevelDownCell, @accountName, @roomId, @maxNumberOfPlayers)";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.DBConnect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            
            AddWithValue(cmd, status);
            
            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task<ChatStatus?> ReadChatStatus(long chatId)
        {
            var sql = $"SELECT \"ChatId\", \"IsAwaitingLogin\", \"IsAwaitingRegister\", \"IsAwaitingJoinRoom\", \"IsAwaitingCreateRoom\", \"IsAwaitingLevelUpCell\", \"IsAwaitingLevelDownCell\", \"AccountName\", \"RoomId\", \"MaxNumberOfPlayers\" " +
                $"FROM PUBLIC.\"{Constants.ChatStatusTable}\" " +
                $"WHERE \"ChatId\" = @chatId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.DBConnect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("chatId", chatId);

            await _connection.OpenAsync();
            NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            if(!await reader.ReadAsync())
            {
                await _connection.CloseAsync();
                return null;
            }
            ChatStatus status = await BuildChatStatus(reader);
            await _connection.CloseAsync();
            return status;
        }
        public async Task UpdateChatStatus(ChatStatus status)
        {
            var sql = $"UPDATE PUBLIC.\"{Constants.ChatStatusTable}\" " +
                $"SET \"IsAwaitingLogin\" = @isAwaitingLogin, \"IsAwaitingRegister\" = @isAwaitingRegister, \"IsAwaitingJoinRoom\" = @isAwaitingJoinRoom, \"IsAwaitingCreateRoom\" = @isAwaitingCreateRoom, \"IsAwaitingLevelUpCell\" = @isAwaitingLevelUpCell, \"IsAwaitingLevelDownCell\" = @isAwaitingLevelDownCell, \"AccountName\" = @accountName, \"RoomId\" = @roomId, \"MaxNumberOfPlayers\" = @maxNumberOfPlayers " +
                $"WHERE \"ChatId\" = @chatId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.DBConnect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, status);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeleteChatStatus(long chatId)
        {
            var sql = $"DELETE FROM PUBLIC.\"{Constants.ChatStatusTable}\" " +
                $"WHERE \"ChatId\" = @chatId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.DBConnect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("chatId", chatId);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        private void AddWithValue(NpgsqlCommand cmd, ChatStatus status)
        {
            cmd.Parameters.AddWithValue("chatId", status.ChatId);
            cmd.Parameters.AddWithValue("IsAwaitingLogin", status.IsAwaitingLogin);
            cmd.Parameters.AddWithValue("IsAwaitingRegister", status.IsAwaitingRegister);
            cmd.Parameters.AddWithValue("IsAwaitingJoinRoom", status.IsAwaitingJoinRoom);
            cmd.Parameters.AddWithValue("IsAwaitingCreateRoom", status.IsAwaitingCreateRoom);
            cmd.Parameters.AddWithValue("IsAwaitingLevelUpCell", status.IsAwaitingLevelUpCell);
            cmd.Parameters.AddWithValue("IsAwaitingLevelDownCell", status.IsAwaitingLevelDownCell);
            cmd.Parameters.AddWithValue("AccountName", (object)status.AccountName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("RoomId", (object)status.RoomId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("MaxNumberOfPlayers", (object)status.MaxNumberOfPlayers ?? DBNull.Value);
        }
        private async Task<ChatStatus> BuildChatStatus(NpgsqlDataReader npgsqlData)
        {
            ChatStatus status = new ChatStatus(npgsqlData.GetInt64(0))
            {
                IsAwaitingLogin = npgsqlData.GetBoolean(1),
                IsAwaitingRegister = npgsqlData.GetBoolean(2),
                IsAwaitingJoinRoom = npgsqlData.GetBoolean(3),
                IsAwaitingCreateRoom = npgsqlData.GetBoolean(4),
                IsAwaitingLevelUpCell = npgsqlData.GetBoolean(5),
                IsAwaitingLevelDownCell = npgsqlData.GetBoolean(6),

                AccountName = npgsqlData.IsDBNull(7) ? null : npgsqlData.GetString(7),
                RoomId = npgsqlData.IsDBNull(8) ? null : npgsqlData.GetString(8),
                MaxNumberOfPlayers = npgsqlData.IsDBNull(9) ? null : npgsqlData.GetInt32(9)
            };
            return status;
        }
    }
}
