using Malshinon.Database;
using Malshinon.Models;
using Malshinon.Utils;
using System;
using System.Collections.Generic;

namespace Malshinon.Services
{
    public class AlertService
    {
        private readonly Dal _dal;
        private readonly PersonService _personService; 

        public AlertService(Dal dal, PersonService personService)
        {
            _dal = dal;
            _personService = personService;
        }


        public void CheckAndGenerateAlerts(int targetId)
        {
            Person target = _personService.GetPersonById(targetId);
            if (target == null) return; 

            string targetName = target.FullName ?? target.SecretCode;

            int totalReports = _dal.GetReportCountByTargetId(targetId);
            if (totalReports >= 20)
            {
                string reason = $"Target '{targetName}' mentioned in {totalReports} reports (threshold 20).";
                CreateAlert(targetId, reason, null, null);
                return; 
            }

            TimeSpan fifteenMinutes = TimeSpan.FromMinutes(15);
            List<Report> recentReports = _dal.GetRecentReportsByTargetId(targetId, fifteenMinutes);

            if (recentReports.Count >= 3)
            {

                DateTime oldestReportInWindow = DateTime.Now;
                DateTime newestReportInWindow = DateTime.Now;

                if (recentReports.Count > 0)
                {
                    newestReportInWindow = recentReports[0].SubmissionTime;
                    oldestReportInWindow = recentReports[recentReports.Count - 1].SubmissionTime;
                }

                string reason = $"Target '{targetName}' mentioned {recentReports.Count} times in last 15 minutes (threshold 3).";
                CreateAlert(targetId, reason, oldestReportInWindow, newestReportInWindow);
            }
        }

        private void CreateAlert(int targetId, string reason, DateTime? windowStart, DateTime? windowEnd)
        {
            Alert newAlert = new Alert
            {
                TargetId = targetId,
                AlertTime = DateTime.Now,
                Reason = reason,
                TimeWindowStart = windowStart,
                TimeWindowEnd = windowEnd
            };
            _dal.AddAlert(newAlert);
            Logger.Info("AlertService", $"Alert created for target ID {targetId}: {reason}");
        }

        public List<Alert> GetAllAlerts()
        {
            return _dal.GetAllAlerts();
        }
    }
}