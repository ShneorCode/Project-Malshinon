
using Malshinon.Database;
using Malshinon.Models;
using Malshinon.Utils;
using System;
using System.Collections.Generic;

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

        public void SubmitReport(string reporterIdentifier, bool isReporterName,
                                 string targetIdentifier, bool isTargetName,
                                 string reportText)
        {
            if (string.IsNullOrWhiteSpace(reportText))
            {
                Logger.Error("ReportService", "Report text cannot be empty.");
                throw new ArgumentException("Report text cannot be empty.");
            }

            Person reporter = _personService.GetOrCreatePerson(reporterIdentifier, isReporterName);
            Person target = _personService.GetOrCreatePerson(targetIdentifier, isTargetName);

            if (reporter == null || target == null)
            {
                Logger.Error("ReportService", "Failed to identify or create reporter/target.");
                throw new InvalidOperationException("Could not identify or create reporter/target.");
            }

            Report newReport = new Report
            {
                ReporterId = reporter.PersonId,
                TargetId = target.PersonId,
                ReportText = reportText,
                SubmissionTime = DateTime.Now
            };
            _dal.AddReport(newReport);

            string reporterNameForLog = reporter.FullName ?? reporter.SecretCode;
            string targetNameForLog = target.FullName ?? target.SecretCode;    
            Logger.Info("ReportService", $"Report submitted by '{reporterNameForLog}' (Code: {reporter.SecretCode}) about '{targetNameForLog}' (Code: {target.SecretCode}). Report: \"{reportText}\"");

            int totalReportsByReporter = _dal.GetReportCountByReporterId(reporter.PersonId);
            double avgLengthByReporter = _dal.GetAverageReportLengthByReporterId(reporter.PersonId);
            _personService.UpdatePersonAnalytics(reporter.PersonId, totalReportsByReporter, avgLengthByReporter);
            Logger.Info("ReportService", $"Updated analytics for reporter ID {reporter.PersonId}.");

            _alertService.CheckAndGenerateAlerts(target.PersonId);
        }

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

            string reporterNameForLog = reporter.FullName ?? reporter.SecretCode; 
            string targetNameForLog = target.FullName ?? target.SecretCode;    
            Logger.Info("ReportService", $"Imported report by '{reporterNameForLog}' (Code: {reporter.SecretCode}) about '{targetNameForLog}' (Code: {target.SecretCode}) at {submissionTime}. Report: \"{reportText}\"");


            int totalReportsByReporter = _dal.GetReportCountByReporterId(reporter.PersonId);
            double avgLengthByReporter = _dal.GetAverageReportLengthByReporterId(reporter.PersonId);
            _personService.UpdatePersonAnalytics(reporter.PersonId, totalReportsByReporter, avgLengthByReporter);

            _alertService.CheckAndGenerateAlerts(target.PersonId);
        }

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