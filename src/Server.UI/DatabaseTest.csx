using Microsoft.Data.Sqlite;
using System;

var connectionString = "Data Source=BlazorDashboardDb.db";

try
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();
    
    Console.WriteLine("Database connection successful!");
    
    // Check for agents
    var agentCommand = connection.CreateCommand();
    agentCommand.CommandText = "SELECT COUNT(*) FROM AspNetUsers WHERE Discriminator = 'Agent'";
    var agentCount = agentCommand.ExecuteScalar();
    Console.WriteLine($"Number of agents in database: {agentCount}");
    
    // Check for conversations
    var conversationCommand = connection.CreateCommand();
    conversationCommand.CommandText = "SELECT COUNT(*) FROM Conversations";
    var conversationCount = conversationCommand.ExecuteScalar();
    Console.WriteLine($"Number of conversations in database: {conversationCount}");
    
    // Check tables
    var tablesCommand = connection.CreateCommand();
    tablesCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
    using var reader = tablesCommand.ExecuteReader();
    Console.WriteLine("Tables in database:");
    while (reader.Read())
    {
        Console.WriteLine($"  - {reader["name"]}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
}
