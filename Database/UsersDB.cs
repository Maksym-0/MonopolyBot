using Npgsql;
using MonopolyBot.Interface.IRepository;
using MonopolyBot.Models.Bot;

namespace MonopolyBot.Database
{
    internal class UsersDB : IUserRepository
    {
        public async Task InsertUser(User user)
        {
            var sql = $"INSERT INTO PUBLIC.\"{Constants.UserTable}\" (\"ChatId\", \"UserId\", \"GameId\", \"Name\", \"JWT\", \"CreatedAt\", \"ExpiresAt\") " +
                $"Values (@chatId, @userId, @gameId, @name, @jwt, @createdAt, @expiresAt) " +
                $"ON CONFLICT (\"ChatId\") " +
                $"DO UPDATE SET \"ChatId\", \"UserId\", \"GameId\", \"Name\", \"JWT\", \"CreatedAt\", \"ExpiresAt\"";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.DBConnect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, user);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task<User> ReadUserWithChatId(long chatId)
        {
            var sql = $"SELECT * FROM PUBLIC.\"{Constants.UserTable}\" " +
                $"WHERE \"ChatId\" = @chatId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.DBConnect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("chatId", chatId);

            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            if (!await npgsqlData.ReadAsync())
            {
                await _connection.CloseAsync();
                throw new Exception("Користувача не знайдено");
            }

            User user = ConstructUser(npgsqlData);
            await _connection.CloseAsync();
            return user;
        }
        public async Task<User> ReadUserWithId(string userId)
        {
            var sql = $"SELECT * FROM PUBLIC.\"{Constants.UserTable}\" " +
                $"WHERE \"UserId\" = @userId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.DBConnect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("userId", userId);

            await _connection.OpenAsync();
            NpgsqlDataReader npgsqlData = await cmd.ExecuteReaderAsync();
            if (!await npgsqlData.ReadAsync())
            {
                await _connection.CloseAsync();
                throw new Exception("Користувача не знайдено");
            }

            User user = ConstructUser(npgsqlData);
            await _connection.CloseAsync();
            return user;
        }
        public async Task UpdateUserGameId(User user)
        {
            var sql = $"UPDATE PUBLIC.\"{Constants.UserTable}\" " +
                $"SET \"GameId\" = @gameId" +
                $"WHERE \"UserId\" = @userId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.DBConnect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            AddWithValue(cmd, user);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeleteUserWithChatId(long chatId)
        {
            var sql = $"DELETE FROM PUBLIC.\"{Constants.UserTable}\" " +
                $"WHERE \"ChatId\" = @chatId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.DBConnect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("chatId", chatId);
            
            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }
        public async Task DeleteUserWithId(string userId)
        {
            var sql = $"DELETE FROM PUBLIC.\"{Constants.UserTable}\" " +
                $"WHERE \"UserId\" = @userId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.DBConnect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);

            cmd.Parameters.AddWithValue("userId", userId);

            await _connection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await _connection.CloseAsync();
        }

        public async Task<bool> SearchUserByChatId(long chatId)
        {
            var sql = $"SELECT * FROM PUBLIC.\"{Constants.UserTable}\" " +
                $"WHERE \"ChatId\" = @chatId";
            NpgsqlConnection _connection = new NpgsqlConnection(Constants.DBConnect);
            NpgsqlCommand cmd = new NpgsqlCommand(sql, _connection);
            
            cmd.Parameters.AddWithValue("chatId", chatId);
            await _connection.OpenAsync();
            NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            bool data = await reader.ReadAsync();
            await _connection.CloseAsync();
            return data;
        }

        private User ConstructUser(NpgsqlDataReader reader)
        {
            return new User
            {
                ChatId = reader.GetInt64(0),
                UserId = reader.GetString(1),
                GameId = reader.IsDBNull(2) ? null : reader.GetString(2),
                Name = reader.GetString(3),
                JWT = reader.GetString(4),
                CreatedAt = reader.GetDateTime(5),
                ExpiresAt = reader.GetDateTime(6)
            };
        }
        private void AddWithValue(NpgsqlCommand cmd, User user)
        {
            cmd.Parameters.AddWithValue("chatId", user.ChatId);
            cmd.Parameters.AddWithValue("userId", user.UserId);
            if (user.GameId != null)
                cmd.Parameters.AddWithValue("gameId", user.GameId);
            else
                cmd.Parameters.AddWithValue("gameId", DBNull.Value);
            cmd.Parameters.AddWithValue("name", user.Name);
            cmd.Parameters.AddWithValue("jwt", user.JWT);
            cmd.Parameters.AddWithValue("createdAt", user.CreatedAt);
            cmd.Parameters.AddWithValue("expiresAt", user.ExpiresAt);
        }
    }
}
