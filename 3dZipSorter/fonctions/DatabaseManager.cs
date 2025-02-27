using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace _3dZipSorter.Database
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager(string databasePath)
        {
            if (!File.Exists(databasePath))
            {
                CreateDatabase(databasePath);
            }
            _connectionString = $"Data Source={databasePath}";
        }

        private void CreateDatabase(string databasePath)
        {
            using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS ArchiveSortingRules (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Extension TEXT NOT NULL UNIQUE,
                        DestinationFile TEXT NOT NULL
                    );
                ";
                command.ExecuteNonQuery();

                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS FileSortingRules (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nom TEXT NOT NULL UNIQUE,
                        Categorie TEXT NOT NULL,
                        Cible TEXT NOT NULL,
                    );
                ";
                command.ExecuteNonQuery();
            }
        }

        public void InsertOrUpdateArchiveSortingRules(string extension, string destinationFile)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO ArchiveSortingRules (Extension, DestinationFile)
                    VALUES ($extension, $destinationFile)
                    ON CONFLICT(Extension) DO UPDATE SET DestinationFile = $destinationFile;
                ";
                command.Parameters.AddWithValue("$extension", extension);
                command.Parameters.AddWithValue("$destinationFile", destinationFile);
                command.ExecuteNonQuery();
            }
        }
        public void UpdateArchiveSortingRulesById(int id, string extension, string destinationFile)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE ArchiveSortingRules
                    SET Extension = $extension, DestinationFile = $destinationFile
                    WHERE Id = $id;
                ";
                command.Parameters.AddWithValue("$id", id);
                command.Parameters.AddWithValue("$extension", extension);
                command.Parameters.AddWithValue("$destinationFile", destinationFile);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteArchiveSortingRuleById(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
            DELETE FROM ArchiveSortingRules
            WHERE Id = $id;
        ";
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }
        }

        public string GetArchiveSortingRuleByExtension(string extension)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT DestinationFile FROM ArchiveSortingRules WHERE Extension = $extension";
                command.Parameters.AddWithValue("$extension", extension);

                return command.ExecuteScalar()?.ToString() ?? string.Empty;
            }
        }

        public Dictionary<string, string> GetAllArchiveSortingRules()
        {
            var rules = new Dictionary<string, string>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT DestinationFile FROM ArchiveSortingRules";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string extension = reader.GetString(0);
                        string category = reader.GetString(1);
                        rules[extension] = category;
                    }
                }
            }
            return rules;
        }
    }
}
