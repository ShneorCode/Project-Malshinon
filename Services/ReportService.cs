using Malshinon.Database;
using Malshinon.Models;
using Malshinon.Utils;
using System;
using System.Collections.Generic; // Add this using directive for List<Report> if needed elsewhere

namespace Malshinon.Services
{
    public class ReportService
    {
        private readonly Dal _dal;
        private readonly PersonService _personService;
        private readonly AlertService _alertService;

        public ReportService(Dal dal, PersonService personService, AlertService alertService)
        {
            _dal = dal;
            _personService = personService;
            _alertService = alertService;
        }

        // Existing SubmitReport method
        public void SubmitReport(string reporterIdentifier, bool isReporterName,
                                 string targetIdentifier, bool isTargetName,
                                 string reportText)
        {
            // Input Validation
            if (string.IsNullOrWhiteSpace(reportText))
            {
                Logger.Error("ReportService", "Report text cannot be empty.");
                throw new ArgumentException("Report text cannot be empty.");
            }

            // Get or create reporter and target
            Person reporter = _personService.GetOrCreatePerson(reporterIdentifier, isReporterName);
            Person target = _personService.GetOrCreatePerson(targetIdentifier, isTargetName);

            if (reporter == null || target == null)
            {
                Logger.Error("ReportService", "Failed to identify or create reporter/target.");
                throw new InvalidOperationException("Could not identify or create reporter/target.");
            }

            // Add the report
            Report newReport = new Report
            {
                ReporterId = reporter.PersonId,
                TargetId = target.PersonId,
                ReportText = reportText,
                SubmissionTime = DateTime.Now // Submission time is DateTime.Now for manual reports
            };
            _dal.AddReport(newReport);
            Logger.Info("ReportService", $"Report submitted by '{reporter.SecretCode}' about '{target.SecretCode}'.");

            // Update reporter analytics
            int totalReportsByReporter = _dal.GetReportCountByReporterId(reporter.PersonId);
            double avgLengthByReporter = _dal.GetAverageReportLengthByReporterId(reporter.PersonId);
            _personService.UpdatePersonAnalytics(reporter.PersonId, totalReportsByReporter, avgLengthByReporter);
            Logger.Info("ReportService", $"Updated analytics for reporter ID {reporter.PersonId}.");


            // Check for alerts on the target
            _alertService.CheckAndGenerateAlerts(target.PersonId);
        }

        // --- NEW METHODS FOR CSV IMPORTER ---

        /// <summary>
        /// Submits a report with a specific submission time (for CSV import).
        /// This method requires pre-identified reporter and target Person objects.
        /// </summary>
        public void SubmitImportedReport(Person reporter, Person target, string reportText, DateTime submissionTime)
        {
            if (reporter == null || target == null || string.IsNullOrWhiteSpace(reportText))
            {
                throw new ArgumentException("Reporter, target, and report text cannot be empty for imported report.");
            }

            Report newReport = new Report
            {
                ReporterId = reporter.PersonId,
                TargetId = target.PersonId,
                ReportText = reportText,
                SubmissionTime = submissionTime
            };
            _dal.AddReport(newReport);
            Logger.Info("ReportService", $"Imported report by '{reporter.SecretCode}' about '{target.SecretCode}' at {submissionTime}.");

            // Update reporter analytics (after adding report)
            int totalReportsByReporter = _dal.GetReportCountByReporterId(reporter.PersonId);
            double avgLengthByReporter = _dal.GetAverageReportLengthByReporterId(reporter.PersonId);
            _personService.UpdatePersonAnalytics(reporter.PersonId, totalReportsByReporter, avgLengthByReporter);

            // Check for alerts on the target (after adding report)
            _alertService.CheckAndGenerateAlerts(target.PersonId);
        }

        // Helper methods to expose PersonService functionality for CSV import if needed
        public Person GetOrCreatePerson(string identifier, bool isName)
        {
            return _personService.GetOrCreatePerson(identifier, isName);
        }

        public int GetReportCountByReporterId(int reporterId)
        {
            return _dal.GetReportCountByReporterId(reporterId);
        }

        public double GetAverageReportLengthByReporterId(int reporterId)
        {
            return _dal.GetAverageReportLengthByReporterId(reporterId);
        }

        public void UpdatePersonAnalytics(int personId, int totalReports, double averageLength)
        {
            _personService.UpdatePersonAnalytics(personId, totalReports, averageLength);
        }

        public void CheckAndGenerateAlerts(int targetId)
        {
            _alertService.CheckAndGenerateAlerts(targetId);
        }
    }
}