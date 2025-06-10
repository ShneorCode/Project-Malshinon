using Malshinon.Database;
using Malshinon.Services;
using Malshinon.Utils;
using MySql.Data.MySqlClient; 
using System;
using System.IO;

namespace Malshinon
{
    class Program
    {
        private const string ConnectionString = "Server=localhost;Port=3306;Database=malshinon;Uid=root;Pwd=;";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; 

            Logger.Initialize(ConnectionString);

            Console.WriteLine("--- Starting Malshinon Application ---");

            try
            {
                using (MySqlConnection testConnection = new MySqlConnection(ConnectionString))
                {
                    testConnection.Open();
                    Console.WriteLine("Successfully connected to MySQL database!");
                }

                Dal dal = new Dal(ConnectionString);
                PersonService personService = new PersonService(dal);
                AlertService alertService = new AlertService(dal, personService);
                ReportService reportService = new ReportService(dal, personService, alertService);
                CsvImporter csvImporter = new CsvImporter(reportService);
                AnalysisService analysisService = new AnalysisService(dal);
                MenuService menuService = new MenuService(reportService, personService, csvImporter, analysisService, alertService);

                menuService.DisplayMainMenu();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"\nERROR: Could not connect to the database or a database operation failed.");
                Console.WriteLine($"Please check your connection string, MySQL server status, and database schema.");
                Console.WriteLine($"Details: {ex.Message}");
                Logger.Error("Program", $"Database connection/operation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn unexpected application error occurred: {ex.Message}");
                Logger.Error("Program", $"Unhandled application error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }
}