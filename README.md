# AuthProject
Authentication system in C# ASP.net and React. Use it as a template to build an app.

# Features:

JWT Tokens used to authenticate users.

User password complexity - minimum 8 characters, a number and a special character.

Check for weak passwords. No 'abc', '123' or 'qwerty' allowed.

User account will be locked for 1 hour after 7 failed login attempts.

SQL injection protection. - Parameterised databases queries and validates input for works like 'select', 'drop' or 'delete'.

User input validation - both server side (api) and client aside (react).

Admin Section to view all users and search for a specific user to update.

Users displayed in a table. Pagination used to show 10 records per page. Only request 10 records at a time from the API.

Website is responsive and scales to Tablets and Mobile phone screen sizes.

Users can reset their passwords.

Verification emails sent to users before allowing them to sign in.

Newsletter subscription via www.sendinblue.com API.

Verification email success triggers adding user to the newsletter.

Logging (MailKit) (text and json files) used for both the API and admin website.


