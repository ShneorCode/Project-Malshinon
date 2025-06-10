using System;

namespace Malshinon.Models
{
    public class Report
    {
        public int ReportId { get; set; }
        public int ReporterId { get; set; }
        public int TargetId { get; set; }
        public string ReportText { get; set; }
        public DateTime SubmissionTime { get; set; }
    }
}