using System;
using System.Collections.Generic;
using Malshinon.Models; 

namespace Malshinon.Services
{
    public class MenuService
    {
        private readonly ReportService _reportService;
        private readonly PersonService _personService;
        private readonly CsvImporter _csvImporter;
        private readonly AnalysisService _analysisService;
        private readonly AlertService _alertService;

        public MenuService(ReportService reportService, PersonService personService,
                           CsvImporter csvImporter, AnalysisService analysisService,
                           AlertService alertService)
        {
            _reportService = reportService;
            _personService = personService;
            _csvImporter = csvImporter;
            _analysisService = analysisService;
            _alertService = alertService;
        }

        public void DisplayMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- Malshinon - Community Intel Reporting System ---");
                Console.WriteLine("1. Submit New Report");
                Console.WriteLine("2. Get Secret Code by Name");
                Console.WriteLine("3. Import Reports from CSV");
                Console.WriteLine("4. View Analysis Dashboard");
                Console.WriteLine("5. View All Alerts");
                Console.WriteLine("6. Exit");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        SubmitReportFlow();
                        break;
                    case "2":
                        GetSecretCodeByNameFlow();
                        break;
                    case "3":
                        ImportCsvFlow();
                        break;
                    case "4":
                        DisplayAnalysisDashboard();
                        break;
                    case "5":
                        DisplayAllAlerts();
                        break;
                    case "6":
                        Console.WriteLine("Exiting Malshinon App. Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        private void SubmitReportFlow()
        {
            Console.Clear();
            Console.WriteLine("--- Submit New Report ---");

            Console.Write("Enter Reporter's Name or Secret Code: ");
            string reporterIdentifier = Console.ReadLine().Trim();
            Console.Write("Is this a (N)ame or (C)ode? (N/C): ");
            string reporterIdTypeInput = Console.ReadLine().Trim().ToUpper();
            bool isReporterName = reporterIdTypeInput == "N";

            Console.Write("Enter Target's Name or Secret Code: ");
            string targetIdentifier = Console.ReadLine().Trim();
            Console.Write("Is this a (N)ame or (C)ode? (N/C): ");
            string targetIdTypeInput = Console.ReadLine().Trim().ToUpper();
            bool isTargetName = targetIdTypeInput == "N";

            Console.Write("Enter Report Text: ");
            string reportText = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(reporterIdentifier) || string.IsNullOrWhiteSpace(targetIdentifier) || string.IsNullOrWhiteSpace(reportText))
            {
                Console.WriteLine("Error: All fields are required.");
                return;
            }

            try
            {
                _reportService.SubmitReport(reporterIdentifier, isReporterName, targetIdentifier, isTargetName, reportText);
                Console.WriteLine("Report submitted successfully!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Submission failed: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Submission failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during report submission: {ex.Message}");
            }
        }

        private void GetSecretCodeByNameFlow()
        {
            Console.Clear();
            Console.WriteLine("--- Get Secret Code by Name ---");
            Console.Write("Enter Full Name: ");
            string fullName = Console.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                Console.WriteLine("Error: Name cannot be empty.");
                return;
            }

            string secretCode = _personService.GetSecretCodeByName(fullName);
            if (secretCode != null)
            {
                Console.WriteLine($"Secret Code for '{fullName}': {secretCode}");
            }
            else
            {
                Console.WriteLine($"Person '{fullName}' not found in the system.");
            }
        }

        private void ImportCsvFlow()
        {
            Console.Clear();
            Console.WriteLine("--- Import Reports from CSV ---");
            Console.Write("Enter path to CSV file (e.g., Data/example.csv): ");
            string filePath = Console.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("Error: File path cannot be empty.");
                return;
            }

            try
            {
                _csvImporter.ImportCsvReports(filePath);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during CSV import: {ex.Message}");
            }
        }

        private void DisplayAnalysisDashboard()
        {
            Console.Clear();
            Console.WriteLine("--- Analysis Dashboard ---");

            Console.WriteLine("\n--- Potential Recruits (Tattletales) ---");
            List<Person> recruits = _analysisService.GetPotentialRecruits();
            if (recruits.Count == 0)
            {
                Console.WriteLine("No potential recruits found yet.");
            }
            else
            {
                foreach (var recruit in recruits)
                {
                    Console.WriteLine($"- Name: {recruit.FullName ?? "[N/A]"}, Code: {recruit.SecretCode}, Reports: {recruit.TotalReportsSubmitted}, Avg Length: {recruit.AverageReportLength:F2}");
                }
            }

            Console.WriteLine("\n--- Dangerous/High-Priority Targets ---");
            List<Person> dangerousTargets = _analysisService.GetDangerousTargets();
            if (dangerousTargets.Count == 0)
            {
                Console.WriteLine("No dangerous targets identified yet.");
            }
            else
            {
                foreach (var target in dangerousTargets)
                {
                    Console.WriteLine($"- Name: {target.FullName ?? "[N/A]"}, Code: {target.SecretCode}");
                }
            }
        }

        private void DisplayAllAlerts()
        {
            Console.Clear();
            Console.WriteLine("--- All Triggered Alerts ---");
            List<Alert> alerts = _alertService.GetAllAlerts();

            if (alerts.Count == 0)
            {
                Console.WriteLine("No alerts have been triggered yet.");
            }
            else
            {
                foreach (var alert in alerts)
                {
                    Person target = _personService.GetPersonById(alert.TargetId);
                    string targetName = target?.FullName ?? target?.SecretCode ?? "[Unknown]";
                    string timeWindow = alert.TimeWindowStart.HasValue && alert.TimeWindowEnd.HasValue
                                        ? $" (Window: {alert.TimeWindowStart.Value:HH:mm:ss} - {alert.TimeWindowEnd.Value:HH:mm:ss})"
                                        : "";
                    Console.WriteLine($"- Alert ID: {alert.AlertId}");
                    Console.WriteLine($"  Target: {targetName} (ID: {alert.TargetId})");
                    Console.WriteLine($"  Time: {alert.AlertTime:yyyy-MM-dd HH:mm:ss}{timeWindow}");
                    Console.WriteLine($"  Reason: {alert.Reason}");
                    Console.WriteLine("----------------------------------");
                }
            }
        }
    }
}