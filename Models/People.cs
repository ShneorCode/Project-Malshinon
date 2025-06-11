namespace Malshinon.Models
{
    public class Person
    {
        public int PersonId { get; set; }
        public string SecretCode { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        //public DateTime UpdatedAt { get; set; }
        public int TotalReportsSubmitted { get; set; }
        public double AverageReportLength { get; set; }
    }
}