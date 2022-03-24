using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API_Project.Services.Validation
{
    public class Validate : IValidate
    {
        // Blacklists
        // It's much better to have a whitelist of only allowing certain user behaviour.
        // However, that's not possible here as a user can enter a wide number of inputs for a username, email or password.
        // The "badWords" list is a blacklist of known sql commands we don't want to allow.
        // Words like "add" and "char" will sometimes blacklist good behaviour like the names "mADDy" and "CHARlie". "Grant" is also a name and a sql command.

        // Parameterised queries
        // This blacklist should be used with parameterised queries in the SQL CRUD commands.
        // Parameterised queries don't allow user input to be sent as text, it's sent as parameters which stops any commands running.
        // Easily spotted by the @ symbol.
        //      string sql = "select ALL from MyTable where UserID=@UserID and pwd=@pwd";
        // Linq/Entity framework uses parameterised queries.


        // A list of bad words and SQL symbols that could be used in an SQL injection attack.
        // Also includes XSS scripting keywords, for injecting html and javascript into the page.
        List<string> badWords = new List<string>()
        { 
            // Passwords, Usernames (and now email addresses RFC 5322) may contain special characters !@#$%^&*()
            // SQL commands
            " add ",      // Add is a difficult one. Can't be "add" or it will match Maddy, need to include spaces.
            "add ",
            " add",
            "alert",
            "alter",
            "begin",
            "body",
            "cast",
            "char",
            "checkpoint",
            "click",
            "cookie",
            "commit",
            "create",
            "cursor",
            "database",
            "delete",
            "describe",
            "deny",
            "document",
            "drop",
            "error",
            "exec",
            "execute",
            "focus",
            "footer",
            "fetch",
            "from",
            "form",
            "grant ",
            " grant",
            "group",
            "header",
            "href",
            "html",
            "img",
            "index",
            "insert",
            "json",
            "join",
            "kill",
            "like ",
            " like",
            "link",
            "load",
            "localhost",
            "null",
            "onmouse",
            "onload",
            "onchange",
            "open",
            "order",
            "password",
            "replace",
            "rollback",
            "savepoint",
            "script",
            "select",
            "section",
            "set",
            "show",
            "string",
            "storage",
            "submit",
            "svg",
            "table",
            "then",
            "truncate",
            "update",
            "use",
            "value",
            "where",
            ".css",
            ".exe",
            ".htm",
            ".js",
            ".ps",
            ".py",
            "fuck",         // Swears
            "shit",
            "cunt",
            "bitch",
            "whore",
            "slut",
            "bastard",
        };



        // Validates fields that should only contain alphabet charcaters
        public int alphabetValidation(string userInput)
        {
            int answer = 0;

            Console.WriteLine(Environment.NewLine);


            // Check length and for blanks
            // Allowing a long query opens up space for SQL injection attacks.
            if (userInput == "" || userInput.Length > 50 || userInput == null)
            {
                answer = answer + 1;    // one added to answer to show fail.
            }

            /*
             * No Need to check for characters only, username can be characters and numbers.
             * 
            // Check for characters only

            // Regex
            // ^ = start at beginning of the string
            // [a-zA-Z] = match any letters lowercase a-z or Uppercase A-Z
            // + = match more than once occurence of this
            // A space is \s. Needed for street names "Main St" and middle names "David John"
            // ? = optional match. the first group of letters need to be matched but the space and the second group are optional
            // $ = end of the string

            if (Regex.IsMatch(userInput, @"^[a-zA-Z]+\s?[a-zA-Z]+$"))
            {

                Console.WriteLine(String.Format("Regex Validation Passed...." + userInput), "Info");
            }
            else
            {
                Console.WriteLine(String.Format("Regex Validation Failed. Input is not only characters..." + userInput), "Error");
                answer = answer + 1;    // one added to answer to show fail.
            }

            */

            string lowercaseInput = userInput.ToLower();            // Convert string to lowercase


            // Check for bad words & SQL injection

            foreach (string word in badWords)
            {
                // Cycle through bad word list
                if (lowercaseInput.Contains(word))
                {
                    answer = answer + 1;    // one added to answer to show fail.
                }
            }


            return answer;
        }




        // Validates fields that should only contain numbers
        // Convert to string first so we can check its length
        // int can be -2,147,483,648 to 2,147,483,647 but our program will not use anywhere near this range. We also don't want user sending extremely long numbers.
        // Check int isn't more than 8 digits.
        public int numberValidation(string userInput)
        {

            int answer = 0;     // answer=0 is a Pass, answer > 0 is a Fail.

            // Check length and for blanks

            if (userInput == "" || userInput.Length > 8)
            {
                answer = answer + 1;    // one added to answer to show fail.
            }


            // Check for integers only

            // Regex
            // ^ = start atbeginning of the string
            // [0-9] = match any numbers 0-9
            // + = match more than once occurence of this
            // $ = end of the string
            if (Regex.IsMatch(userInput, @"^[0-9]+$"))
            {

                //Console.WriteLine(String.Format("Regex Validation Passed...." + userInput), "Info");
            }
            else
            {
                answer = answer + 1;    // one added to answer to show fail.
            }

            string lowercaseInput = userInput.ToLower();            // Convert string to lowercase

            // Check for bad words & SQL injection

            foreach (string word in badWords)
            {                                                       // Cycle through bad world list
                if (lowercaseInput.Contains(word))
                {
                    answer = answer + 1;                               // one added to answer to show fail.
                }
            }


            return answer;
        }





        // Validates email addresses.
        // Accepts subdomains you@subdomain.you.com
        // Doesn't accept ..com, two @s, you.com7 
        public int emailValidation(string userInput)
        {

            int answer = 0;     // answer=0 is a Pass, answer > 0 is a Fail.

            // Check length and for blanks

            if (userInput == "" || userInput.Length > 50)
            {
                answer = answer + 1;    // one added to answer to show fail.
            }


            // Check for correct email address

            // Regex
            // Matches email addresses with subdomains, @ symbol, avoids double dots etc
            // Found online: https://www.rhyous.com/2010/06/15/csharp-email-regular-expression/
            if (Regex.IsMatch(userInput, @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z"))
            {

                //Console.WriteLine(String.Format("Email Validation Passed...." + userInput), "Info");
            }
            else
            {
                answer = answer + 1;    // one added to answer to show fail.
            }


            string lowercaseInput = userInput.ToLower();            // Convert string to lowercase


            // Check for bad words & SQL injection

            foreach (string word in badWords)
            {                                                       // Cycle through bad world list
                if (lowercaseInput.Contains(word))
                {
                    answer = answer + 1;                               // one added to answer to show fail.
                }
            }


            return answer;
        }



        // Validate DateTime
        // Check it's in Utc format
        public int dateTimeUtcValidation(DateTime date)
        {
            int answer = 0;     // answer=0 is a Pass, answer > 0 is a Fail.

            // Check format of date time is in Utc
            if (date.Kind != DateTimeKind.Utc)
            {
                // Not in Utc format
                answer = answer + 1;
                return answer;
            }

            string lowercaseInput = date.ToString().ToLower();            // Convert to lowercase string

            // Check for bad words & SQL injection
            foreach (string word in badWords)
            {                                                       // Cycle through bad world list
                if (lowercaseInput.Contains(word))
                {
                    answer = answer + 1;                               // one added to answer to show fail.
                }
            }

            return answer;

        }

    }
}
