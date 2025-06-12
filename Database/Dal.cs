using Malshinon.Models;
using Malshinon.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data; 

namespace Malshinon.Database
{
    public class Dal
    {
        private readonly string _connectionString;

        public Dal(string connectionString)
        {
            _connectionString = connectionString;
            //Logger.Info("DAL", "DAL initialized with connection string.");
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }


        public Person GetPersonById(int personId)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM People WHERE person_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", personId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapToPerson(reader);
                    }
                }
            }
            return null;
        }

        public Person GetPersonByCode(string secretCode)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM People WHERE secret_code = @code";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@code", secretCode);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapToPerson(reader);
                    }
                }
            }
            return null;
        }

        public Person GetPersonByName(string fullName)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM People WHERE full_name = @name";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", fullName);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapToPerson(reader);
                    }
                }
            }
            return null;
        }

        public Person CreateNewPerson(string fullName = null)
        {
            string secretCode = CodeGenerator.GenerateSecretCode();
            while (GetPersonByCode(secretCode) != null)
            {
                secretCode = CodeGenerator.GenerateSecretCode();
            }

            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO People (secret_code, full_name) VALUES (@code, @name)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@code", secretCode);
                cmd.Parameters.AddWithValue("@name", (object)fullName ?? DBNull.Value);
                cmd.ExecuteNonQuery();

                long newId = cmd.LastInsertedId;
                Logger.Info("DAL", $"New person created: ID={newId}, Code={secretCode}, Name={fullName}");
                return GetPersonById((int)newId);
            }
        }

        public void UpdatePersonAnalytics(int personId, int totalReports, double averageLength)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "UPDATE People SET total_reports_submitted = @total, average_report_length = @avg WHERE person_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@total", totalReports);
                cmd.Parameters.AddWithValue("@avg", averageLength);
                cmd.Parameters.AddWithValue("@id", personId);
                cmd.ExecuteNonQuery();
                Logger.Info("DAL", $"Updated analytics for person ID: {personId}");
            }
        }


        public void AddReport(Report report)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO Reports (reporter_id, target_id, report_text, submission_time) VALUES (@reporterId, @targetId, @reportText, @submissionTime)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@reporterId", report.ReporterId);
                cmd.Parameters.AddWithValue("@targetId", report.TargetId);
                cmd.Parameters.AddWithValue("@reportText", report.ReportText);
                cmd.Parameters.AddWithValue("@submissionTime", report.SubmissionTime);
                cmd.ExecuteNonQuery();
                Logger.Info("DAL", $"Report added: Reporter ID {report.ReporterId}, Target ID {report.TargetId}");
            }
        }

        public List<Report> GetReportsByTargetId(int targetId)
        {
            List<Report> reports = new List<Report>();
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Reports WHERE target_id = @targetId ORDER BY submission_time DESC";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@targetId", targetId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reports.Add(MapToReport(reader));
                    }
                }
            }
            return reports;
        }

        public List<Report> GetRecentReportsByTargetId(int targetId, TimeSpan timeWindow)
        {
            List<Report> reports = new List<Report>();
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                DateTime windowStart = DateTime.Now.Subtract(timeWindow);
                string query = "SELECT * FROM Reports WHERE target_id = @targetId AND submission_time >= @windowStart ORDER BY submission_time DESC";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@targetId", targetId);
                cmd.Parameters.AddWithValue("@windowStart", windowStart);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reports.Add(MapToReport(reader));
                    }
                }
            }
            return reports;
        }

        public int GetReportCountByTargetId(int targetId)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Reports WHERE target_id = @targetId";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@targetId", targetId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int GetReportCountByReporterId(int reporterId)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Reports WHERE reporter_id = @reporterId";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@reporterId", reporterId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public double GetAverageReportLengthByReporterId(int reporterId)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT AVG(LENGTH(report_text)) FROM Reports WHERE reporter_id = @reporterId";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@reporterId", reporterId);
                object result = cmd.ExecuteScalar();
                if (result == DBNull.Value) return 0.0;
                return Convert.ToDouble(result);
            }
        }


        public void AddAlert(Alert alert)
        {
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO Alerts (target_id, alert_time, time_window_start, time_window_end, reason) VALUES (@targetId, @alertTime, @windowStart, @windowEnd, @reason)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@targetId", alert.TargetId);
                cmd.Parameters.AddWithValue("@alertTime", alert.AlertTime);
                cmd.Parameters.AddWithValue("@windowStart", (object)alert.TimeWindowStart ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@windowEnd", (object)alert.TimeWindowEnd ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@reason", alert.Reason);
                cmd.ExecuteNonQuery();
                Logger.Info("DAL", $"Alert generated for Target ID {alert.TargetId}: {alert.Reason}");
            }
        }

        public List<Alert> GetAllAlerts()
        {
            List<Alert> alerts = new List<Alert>();
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Alerts ORDER BY alert_time DESC";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        alerts.Add(MapToAlert(reader));
                    }
                }
            }
            return alerts;
        }

        private Person MapToPerson(MySqlDataReader reader)
        {
            return new Person
            {
                PersonId = reader.GetInt32("person_id"),
                SecretCode = reader.GetString("secret_code"),
                FullName = reader.IsDBNull(reader.GetOrdinal("full_name")) ? null : reader.GetString("full_name"),
                CreatedAt = reader.GetDateTime("created_at"),
                //UpdatedAt = reader.GetDateTime("updated_at"),
                TotalReportsSubmitted = reader.GetInt32("total_reports_submitted"),
                AverageReportLength = reader.GetDouble("average_report_length")
            };
        }

        private Report MapToReport(MySqlDataReader reader)
        {
            return new Report
            {
                ReportId = reader.GetInt32("report_id"),
                ReporterId = reader.GetInt32("reporter_id"),
                TargetId = reader.GetInt32("target_id"),
                ReportText = reader.GetString("report_text"),
                SubmissionTime = reader.GetDateTime("submission_time")
            };
        }

        private Alert MapToAlert(MySqlDataReader reader)
        {
            return new Alert
            {
                AlertId = reader.GetInt32("alert_id"),
                TargetId = reader.GetInt32("target_id"),
                AlertTime = reader.GetDateTime("alert_time"),
                TimeWindowStart = reader.IsDBNull(reader.GetOrdinal("time_window_start")) ? (DateTime?)null : reader.GetDateTime("time_window_start"),
                TimeWindowEnd = reader.IsDBNull(reader.GetOrdinal("time_window_end")) ? (DateTime?)null : reader.GetDateTime("time_window_end"),
                Reason = reader.GetString("reason")
            };
        }



        public List<Person> GetAllPeople()
        {
            List<Person> people = new List<Person>();
            using (MySqlConnection connection = GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM People";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        people.Add(MapToPerson(reader));
                    }
                }
            }
            return people;
        }

    }
}