using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API_Project.Data
{
    public class StrongPassword
    {

        // Check if the user's password meets complexity requirements
        // Passwords must have a length of 8+ characters, a mix of lower an duppercase characters, a number and a special character.

        public bool Check(string password)
        {

            // Check for special code to keep password the same
            // Client may send this to say we only want to edit email or username fields and not change the exisiting password.
            if (password.Contains("#keepSame#"))
            {
                return true;
            }

            // (?=.*[a-z])  lowercase letters
            // (?=.*[A-Z])  uppercase letters
            // (?=.*[0-9])  numbers
            // (?=.*[!@#$%^&*])  special characters
            // .{8,}        eight or more
            string pattern = "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*]).{8,}$";

            // No match, return false.
            if (Regex.IsMatch(password, pattern) == false)
            {
                return false;
            }

            // Check the password doesn't contain the word "password", "pass", "qwerty", "abc",  "test" or "123"...
            if (password.ToLower().Contains("password") || password.ToLower().Contains("pass") || password.ToLower().Contains("qwerty") || password.ToLower().Contains("abc") || password.ToLower().Contains("test") || password.ToLower().Contains("123"))
            {
                return false;
            }

            // So the worst the password could be is something like <dictionary_word>456!
            // John012! or Hello12! or 5678Me!@  would pass. But that's really the user's fault...
            return true;
        }
    }
}
