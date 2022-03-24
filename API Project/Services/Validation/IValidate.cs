using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Services.Validation
{
    interface IValidate
    {
        // All methods return 0 if successful.
        // They count the errors so > 0 is a fail.
        int alphabetValidation(string userInput);       // Valdidate strings. Firstname, Username, Pasword, Email Address, Surname inputs

        int numberValidation(string userInput);         // Validate numbers. id input

        int emailValidation(string userInput);          // Validate email addresses

        public int dateTimeUtcValidation(DateTime date);    // Validate DateTime UTC format

    }
}
