using System;
using System.Data;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BIC_FHTW.DiscordBot.Services;
using Microsoft.Data.Sqlite;

namespace BIC_FHTW.WebApp.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
        
    public DatabaseService()
    {
        _connectionString = $"Data Source={Environment.GetEnvironmentVariable("DATABASE_FILENAME")}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
            
        var createTableCommand = connection.CreateCommand();
        createTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                DiscordId TEXT PRIMARY KEY,
                Email TEXT NOT NULL,
                Pending BOOLEAN NOT NULL,
                Token TEXT NOT NULL
            );";
        createTableCommand.ExecuteNonQuery();
    }

    public void AddUser(string discordId, string email, string token)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = @"
            INSERT INTO Users (DiscordId, Email, Pending, Token)
            VALUES ($DiscordId, $Email, $Pending, $Token);";

        insertCommand.Parameters.AddWithValue("$DiscordId", discordId);
        insertCommand.Parameters.AddWithValue("$Email", email);
        insertCommand.Parameters.AddWithValue("$Pending", true);
        insertCommand.Parameters.AddWithValue("$Token", token);
        insertCommand.ExecuteNonQuery(); 
    }
    
    public string GenerateUniqueToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        
        /* OBSOLETE
         * 'RNGCryptoServiceProvider is obsolete.
         * To generate a random number, use one of the RandomNumberGenerator static methods instead.'
        using var rng = new RNGCryptoServiceProvider();
        var randomBytes = new byte[32];
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        */
    }
    
    public async Task<(ulong DiscordId, string EmailAddress)?> GetUserByToken(string token)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText =
            @"SELECT DiscordId, EmailAddress
          FROM Users
          WHERE Token = $token";
        command.Parameters.AddWithValue("$token", token);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);

        if (!await reader.ReadAsync()) return null;
        var discordId = (ulong)reader.GetInt64(0);
        var emailAddress = reader.GetString(1);
        return (discordId, emailAddress);
    }
    
    public void UpdateUserStatus(string token, bool isPending)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var updateCommand = connection.CreateCommand();
        updateCommand.CommandText = @"
            UPDATE Users
            SET Pending = $Pending
            WHERE Token = $Token;";

        updateCommand.Parameters.AddWithValue("$Pending", isPending);
        updateCommand.Parameters.AddWithValue("$Token", token);
        updateCommand.ExecuteNonQuery();
    }
}