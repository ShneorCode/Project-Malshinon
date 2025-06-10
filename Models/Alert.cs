using System;

namespace Malshinon.Models
{
    public class Alert
    {
        public int AlertId { get; set; }
        public int TargetId { get; set; }
        public DateTime AlertTime { get; set; }
        public DateTime? TimeWindowStart { get; set; }
        public DateTime? TimeWindowEnd { get; set; }  
        public string Reason { get; set; }
    }
}