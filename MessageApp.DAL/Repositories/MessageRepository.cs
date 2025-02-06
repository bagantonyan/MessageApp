using MessageApp.DAL.Entities;
using MessageApp.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MessageApp.DAL.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly string _connectionString;

        public MessageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> AddMessageAsync(Message message)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new NpgsqlCommand("INSERT INTO Messages (Text, CreatedAt, SequenceNumber) VALUES (@Text, @CreatedAt, @SequenceNumber) RETURNING Id", conn))
                {
                    cmd.Parameters.AddWithValue("Text", message.Text);
                    cmd.Parameters.AddWithValue("CreatedAt", message.CreatedAt);
                    cmd.Parameters.AddWithValue("SequenceNumber", message.SequenceNumber);

                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<List<Message>> GetMessagesAsync(DateTime startTime)
        {
            var messages = new List<Message>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new NpgsqlCommand("SELECT Id, Text, CreatedAt, SequenceNumber FROM Messages WHERE CreatedAt >= @StartTime", conn))
                {
                    cmd.Parameters.AddWithValue("StartTime", startTime);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var message = new Message
                            {
                                Id = reader.GetInt32(0),
                                Text = reader.GetString(1),
                                CreatedAt = reader.GetDateTime(2),
                                SequenceNumber = reader.GetInt32(3)
                            };
                            messages.Add(message);
                        }
                    }
                }
            }

            return messages;
        }

        public async Task<int> GetNextSequenceNumberAsync()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Messages", conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result) + 1;
                }
            }
        }
    }
}
