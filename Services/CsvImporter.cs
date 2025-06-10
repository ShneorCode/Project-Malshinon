using Malshinon.Models;
using Malshinon.Utils;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Malshinon.Services
{
    public class CsvImporter
    {
        private readonly ReportService _reportService; // This already has access to personService and alertService indirectly
        // We do NOT need direct access to _reportService._personService or _reportService._dal
        // We will call public methods on _reportService instead.

        public CsvImporter(ReportService reportService)
        {
            _reportService = reportService;
        }

        public void ImportCsvReports(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Logger.Error("CsvImporter", $"CSV file not found: {filePath}");
                throw new FileNotFoundException($"CSV file not found at {filePath}");
            }

            int importedCount = 0;
            int errorCount = 0;

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                Logger.Info("CsvImporter", $"Starting CSV import from {filePath}. {lines.Length} lines found.");

                bool skipHeader = true;
                int startLine = skipHeader ? 1 : 0;

                for (int i = startLine; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = SplitCsvLine(line);

                    if (parts.Length < 3 || parts.Length > 4)
                    {
                        Logger.Error("CsvImporter", $"Invalid CSV line format (line {i + 1}): {line}");
                        errorCount++;
                        continue;
                    }

                    string reporterId = parts[0].Trim();
                    string targetId = parts[1].Trim();
                    string reportText = parts[2].Trim();
                    DateTime submissionTime = DateTime.Now;

                    if (parts.Length == 4 && DateTime.TryParse(parts[3].Trim(), out DateTime parsedTime))
                    {
                        submissionTime = parsedTime;
                    }

                    bool isReporterName = !IsLikelySecretCode(reporterId);
                    bool isTargetName = !IsLikelySecretCode(targetId);

                    try
                    {
                        // Use the public methods of ReportService to get/create persons
                        Person reporter = _reportService.GetOrCreatePerson(reporterId, isReporterName);
                        Person target = _reportService.GetOrCreatePerson(targetId, isTargetName);

                        if (reporter == null || target == null)
                        {
                            Logger.Error("CsvImporter", $"Failed to process line {i + 1}: Could not identify/create reporter or target for line: {line}");
                            errorCount++;
                            continue;
                        }

                        // Use the new public method SubmitImportedReport
                        _reportService.SubmitImportedReport(reporter, target, reportText, submissionTime);

                        importedCount++;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("CsvImporter", $"Error processing CSV line {i + 1}: {line}. Error: {ex.Message}");
                        Console.WriteLine($"Error processing CSV line {i + 1}: {line}. Error: {ex.Message}"); // Added console output for immediate feedback
                        errorCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("CsvImporter", $"General error during CSV import from {filePath}: {ex.Message}");
                Console.WriteLine($"Error during CSV import: {ex.Message}");
            }

            Logger.Info("CsvImporter", $"CSV import complete. Imported {importedCount} reports, {errorCount} errors.");
            Console.WriteLine($"CSV import finished. Imported {importedCount} reports, {errorCount} errors.");
        }

        private bool IsLikelySecretCode(string identifier)
        {
            return !identifier.Contains(" ");
        }

        private string[] SplitCsvLine(string line)
        {
            return line.Split(',');
        }
    }
}