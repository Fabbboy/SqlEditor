using Microsoft.Data.Sqlite;
using System;

namespace sqleditor
{
    public static class GlobalDatabase
    {
        public static SqliteConnection? Connection { get; set; }

        private static bool DoesCredentialsTableExist()
        {
            if (Connection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }

            var command = Connection.CreateCommand();
            command.CommandText = @"
                SELECT name 
                FROM sqlite_master 
                WHERE type='table' AND name='__sqleditor';";

            return command.ExecuteScalar() != null;
        }

        public static void CreateCredentialsTable()
        {
            if (Connection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }

            var command = Connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS __sqleditor (
                    username TEXT NOT NULL,
                    password TEXT NOT NULL
                );

                INSERT INTO __sqleditor (username, password) VALUES ('admin', 'admin');";
            command.ExecuteNonQuery();

            Console.WriteLine("Default credentials created: Username = admin, Password = admin");
        }

        private static void CheckCredentials(string? username, string? password)
        {
            if (Connection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username or password cannot be empty.");
            }

            if (!DoesCredentialsTableExist())
            {
                throw new InvalidOperationException("Credentials table does not exist. Initialize the database.");
            }

            var command = Connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(1) 
                FROM __sqleditor 
                WHERE username = @username AND password = @password;";
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);

            var userExists = (long)command.ExecuteScalar() > 0;

            if (!userExists)
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
        }

        public static void OpenDatabase(string databasePath, string? username, string? password)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new InvalidOperationException("Database path is not provided.");
            }

            Connection = new SqliteConnection($"Data Source={databasePath}");
            Connection.Open();

            if (!DoesCredentialsTableExist())
            {
                CreateCredentialsTable();
                throw new InvalidOperationException("Credentials table was missing. Default credentials have been created. admin, admin");
            }

            CheckCredentials(username, password);
        }
    }
}
