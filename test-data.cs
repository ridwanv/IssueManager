using System;
using System.Data.SQLite;

var connectionString = "Data Source=src/Server.UI/BlazorDashboardDb.db";
using var connection = new SQLiteConnection(connectionString);
connection.Open();

// Check if Issues table exists and has data
var command = new SQLiteCommand("SELECT COUNT(*) FROM Issues", connection);
try 
{
    var count = (long)command.ExecuteScalar();
    Console.WriteLine($"Number of issues in database: {count}");
    
    // Get table info
    command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table'", connection);
    using var reader = command.ExecuteReader();
    Console.WriteLine("Tables in database:");
    while (reader.Read())
    {
        Console.WriteLine($"  - {reader["name"]}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}