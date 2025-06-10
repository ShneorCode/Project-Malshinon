using MySql.Data.MySqlClient;
using System;
using System.IO;

namespace Malshinon.Utils
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Logs", "log.txt");
        private static string _connectionString;

        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
            string logDirectory = Path.GetDirectoryName(LogFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public static void Log(string level, string activityType, string description)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] [{activityType}] {description}";

            try
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file log: {ex.Message}");
                Console.WriteLine(logEntry);
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO AppLogs (log_time, log_level, activity_type, description) VALUES (@time, @level, @type, @desc)";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@time", DateTime.Now);
                    cmd.Parameters.AddWithValue("@level", level);
                    cmd.Parameters.AddWithValue("@type", activityType);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to database log: {ex.Message}");
                Console.WriteLine($"Failed DB log entry: {logEntry}");
            }
        }

        public static void Info(string activityType, string description)
        {
            Log("INFO", activityType, description);
        }

        public static void Error(string activityType, string description)
        {
            Log("ERROR", activityType, description);
        }

        public static void Warn(string activityType, string description)
        {
            Log("WARN", activityType, description);
        }
    }
}