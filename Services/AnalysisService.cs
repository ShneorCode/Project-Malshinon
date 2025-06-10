using Malshinon.Database;
using Malshinon.Models;
using Malshinon.Utils;
using System.Collections.Generic;

namespace Malshinon.Services
{
    public class AnalysisService
    {
        private readonly Dal _dal;

        private const int MinReportsForRecruit = 10;
        private const int MinAvgReportLengthForRecruit = 100;

        private const int MinReportsForDangerousTarget = 20; 

        public AnalysisService(Dal dal)
        {
            _dal = dal;
        }


        public List<Person> GetPotentialRecruits()
        {
            List<Person> recruits = new List<Person>();


            List<Person> allPeople = _dal.GetAllPeople(); 

            foreach (Person person in allPeople)
            {
                if (person.TotalReportsSubmitted >= MinReportsForRecruit &&
                    person.AverageReportLength >= MinAvgReportLengthForRecruit)
                {
                    recruits.Add(person);
                }
            }
            Logger.Info("AnalysisService", $"Identified {recruits.Count} potential recruits.");
            return recruits;
        }



        public List<Person> GetDangerousTargets()
        {
            List<Person> dangerousTargets = new List<Person>();


            List<Person> allPeople = _dal.GetAllPeople(); 

            foreach (Person person in allPeople)
            {
                int targetReportCount = _dal.GetReportCountByTargetId(person.PersonId);
                if (targetReportCount >= MinReportsForDangerousTarget)
                {
                    dangerousTargets.Add(person);
                }
            }
            Logger.Info("AnalysisService", $"Identified {dangerousTargets.Count} dangerous targets.");
            return dangerousTargets;
        }
    }
}

