using Malshinon.Database;
using Malshinon.Models;
using Malshinon.Utils;
using System;

namespace Malshinon.Services
{
    public class PersonService
    {
        private readonly Dal _dal;

        public PersonService(Dal dal)
        {
            _dal = dal;
        }


        public Person GetOrCreatePerson(string identifier, bool isName)
        {
            Person person = null;
            if (isName)
            {
                person = _dal.GetPersonByName(identifier);
                if (person == null)
                {
                    Logger.Info("PersonService", $"Person '{identifier}' (name) not found. Creating new.");
                    person = _dal.CreateNewPerson(identifier);
                }
            }
            else 
            {
                person = _dal.GetPersonByCode(identifier);
                if (person == null)
                {
                    
                    Logger.Info("PersonService", $"Person with code '{identifier}' not found. Creating new (code-only).");
                    person = _dal.CreateNewPerson();
                    person.SecretCode = identifier; 
    
                }
            }
            return person;
        }


        public string GetSecretCodeByName(string fullName)
        {
            Person person = _dal.GetPersonByName(fullName);
            if (person != null)
            {
                Logger.Info("PersonService", $"Secret code retrieved for '{fullName}'.");
                return person.SecretCode;
            }
            Logger.Warn("PersonService", $"Secret code not found for name: '{fullName}'.");
            return null;
        }

        public void UpdatePersonAnalytics(int personId, int totalReports, double averageLength)
        {
            _dal.UpdatePersonAnalytics(personId, totalReports, averageLength);
        }

        public Person GetPersonById(int personId)
        {
            return _dal.GetPersonById(personId);
        }
    }
}