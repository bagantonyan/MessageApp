using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MessageApp.DAL.Helpers
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task EnsureDatabaseAndTableCreatedAsync()
        {
            var masterConnectionString = _connectionString.Replace("Database=messageappdb", "Database=postgres");

            try
            {
                _logger.LogInformation("Checking if database 'messageappdb' exists...");

                using (var masterConnection = new NpgsqlConnection(masterConnectionString))
                {
                    await masterConnection.OpenAsync();
                    var checkDbCommand = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'messageappdb'", masterConnection);
                    var exists = await checkDbCommand.ExecuteScalarAsync();

                    if (exists == null)
                    {
                        _logger.LogWarning("Database 'messageappdb' not found. Creating...");

                        var createDbCommand = new NpgsqlCommand("CREATE DATABASE messageappdb", masterConnection);
                        await createDbCommand.ExecuteNonQueryAsync();

                        _logger.LogInformation("Database 'messageappdb' created successfully.");
                    }
                    else
                    {
                        _logger.LogInformation("Database 'messageappdb' already exists.");
                    }
                }

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var createTableCommand = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS messages (
                        Id SERIAL PRIMARY KEY,
                        Text VARCHAR(128) NOT NULL,
                        CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        SequenceNumber INT UNIQUE NOT NULL
                    )", connection);

                    await createTableCommand.ExecuteNonQueryAsync();

                    _logger.LogInformation("Table 'messages' ensured.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database.");
            }
        }
    }
}