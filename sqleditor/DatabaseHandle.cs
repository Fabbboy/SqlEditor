using Microsoft.Data.Sqlite;

namespace sqleditor
{
    public static class GlobalDatabase
    {
        public static SqliteConnection? Connection { get; set; }

        public static void OpenDatabase(string DatabasePath)
        {
            if (DatabasePath is null)
            {
                throw new InvalidOperationException("DatabasePath is not set");
            }
            Connection = new SqliteConnection($"Data Source={DatabasePath}");
            Connection.Open();
        }
    }
}
