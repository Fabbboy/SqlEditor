using Microsoft.Data.Sqlite;
using System;

namespace sqleditor
{

    public enum ColumnType
    {
        Text,
        Integer,
        Real,
        Blob
    }
    public class ColumnHandle
    {
        public string ColumnName { get; set; }
        public ColumnType ColumnType { get; set; }
        public string ColumnValue { get; set; }

        public ColumnHandle(string columnName, ColumnType columnType, string columnValue)
        {
            ColumnName = columnName;
            ColumnType = columnType;
            ColumnValue = columnValue;
        }
    }
    public class TableHandle
    {
        public string TableName { get; set; }

        public List<ColumnHandle> Columns { get; set; }

        public TableHandle(string tableName)
        {
            TableName = tableName;
            Columns = new List<ColumnHandle>();
        }
    }

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

        public static List<ColumnHandle> GetTableColumns(string tableName)
        {
            if (Connection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }

            var columns = new List<ColumnHandle>();

            var command = Connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info({tableName});";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var columnName = reader["name"].ToString() ?? string.Empty;
                var columnType = reader["type"].ToString()?.ToLower() switch
                {
                    "text" => ColumnType.Text,
                    "integer" => ColumnType.Integer,
                    "real" => ColumnType.Real,
                    "blob" => ColumnType.Blob,
                    _ => throw new InvalidOperationException($"Unknown column type: {reader["type"]}")
                };

                columns.Add(new ColumnHandle(columnName, columnType, string.Empty)); // No value yet
            }

            return columns;
        }

        public static List<Dictionary<string, string>> GetTableData(string tableName)
        {
            if (Connection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }

            var data = new List<Dictionary<string, string>>();

            var command = Connection.CreateCommand();
            command.CommandText = $"SELECT * FROM {tableName};";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString();
                    row.Add(columnName, value ?? string.Empty);
                }
                data.Add(row);
            }

            return data;
        }

        public static TableHandle GetTableInfo(string tableName)
        {
            var table = new TableHandle(tableName);

            table.Columns = GetTableColumns(tableName);

            var data = GetTableData(tableName);
            foreach (var row in data)
            {
                foreach (var column in table.Columns)
                {
                    if (row.ContainsKey(column.ColumnName))
                    {
                        column.ColumnValue = row[column.ColumnName];
                    }
                }
            }

            return table;
        }
    

    public static List<string> GetAllTables()
        {
            if (Connection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }

            var tables = new List<string>();

            var command = Connection.CreateCommand();
            command.CommandText = @"
        SELECT name 
        FROM sqlite_master 
        WHERE type='table' 
        AND name NOT LIKE 'sqlite_%' -- Exclude system tables
        ORDER BY name;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0)); // Add table name to the list
            }

            return tables;
        }
        public static List<TableHandle> GetAllTablesWithColumns()
        {
            var tables = GetAllTables();
            var tablesWithColumns = new List<TableHandle>();
            foreach (var column in tables)
            {
                tablesWithColumns.Add(GetTableInfo(column));
            }

            return tablesWithColumns;
        }
        public static void DeleteTable(TableHandle tableHandle)
        {
            if (Connection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }

            if (tableHandle == null || string.IsNullOrWhiteSpace(tableHandle.TableName))
            {
                throw new ArgumentException("Invalid table handle or table name.");
            }

            var command = Connection.CreateCommand();
            command.CommandText = $"DROP TABLE IF EXISTS {tableHandle.TableName};";

            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine($"Table '{tableHandle.TableName}' deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting table '{tableHandle.TableName}': {ex.Message}");
                throw;
            }
        }

    }
}
 