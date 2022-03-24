using API_Project.Models;                   // To use Character Model
using Microsoft.EntityFrameworkCore;        // For DbContext inheritance
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace API_Project.Data
{
    // Entity Install
    /*
        Microsoft.EntityFrameworkCore.5.0.10

        Microsoft.EntityFrameworkCore.SqlServer.5.0.10

        Microsoft.EntityFrameworkCore.Design.5.0.10

        Install Entity Command Line Tools (to do migrations).
        Tools > ComandLine > Developer PS
        C:/..projecturl.. > dotnet tool install --global dotnet-ef
     */


    // Entity Code First setup
    // Entity is the object relational mapper that talks to the database
    // Connection string is in appsettings.json


    // Migrations
    // Need to do an intial create to set up the database.
    // Tools > CommandLine > Dev Powershell
    /*
        PS C:\CSharpProjects\1 - dotnet5\API Project (Entity, SQL, Auth)> cd "API Project"
        PS C:\CSharpProjects\1 - dotnet5\API Project (Entity, SQL, Auth)\API Project> dotnet ef migrations add InitalCreate     
        Build started...
        Build succeeded.
        PS C:\CSharpProjects\1 - dotnet5\API Project (Entity, SQL, Auth)\API Project> dotnet ef database update
        Build started...
        Build succeeded.
     */

    // If you added User Table after creating the datatbase...
    // cd "API Project"
    // dotnet ef migrations add User                    dotnet ef migrations add <name_the_migration>
    // dotnet ef database update

    public class DataContext : DbContext
    {
        public IConfiguration _config { get; }

        //Constructor
        public DataContext(DbContextOptions<DataContext> options, IConfiguration configuration) : base(options)
        {
            _config = configuration;
        }


        // Entity needs to know the model of the database so it can set up the tables

        // User's Table - Authentication, username, password etc
        public DbSet<User> Users { get; set; }


        // OnModelCreating is a FluentAPI method
        // Overriding it allows you to set default values for properties when creating the database entry.
        // Easier and more readable to use this rather than annotaions everywhere throughout your code.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Data Seeding

            // Auto Create Default Admin Account
            // TangoTime7!

            byte[] adminPassHash;
            byte[] adminPassSalt;

            byte[] userPassHash;
            byte[] userPassSalt;

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                adminPassSalt = hmac.Key;
                adminPassHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_config.GetValue<string>("AdminLogin:Password")));

                userPassSalt = hmac.Key;
                userPassHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(_config.GetValue<string>("AdminLogin:Password")));
            }


            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Email = _config.GetValue<string>("AdminLogin:Email"), EmailVerified = true, EmailVerifyToken = null, DateEmailVerifySent = DateTime.UtcNow, Username = _config.GetValue<string>("AdminLogin:Username"), PasswordHash = adminPassHash, PasswordSalt = adminPassSalt, Role = "admin", LoginTries = 0, AccountLocked = false, DateLocked = DateTime.UtcNow, Newsletter = false },
                new User { Id = 2, Email = "basic_test@test1222.com", EmailVerified = true, EmailVerifyToken = null, DateEmailVerifySent = DateTime.UtcNow, Username = "basic_test", PasswordHash = userPassHash, PasswordSalt = userPassSalt, Role = "basic", LoginTries = 0, AccountLocked = false, DateLocked = DateTime.UtcNow, Newsletter = false },
                new User { Id = 3, Email = "premium_test@test122.com", EmailVerified = true, EmailVerifyToken = null, DateEmailVerifySent = DateTime.UtcNow, Username = "premium_test", PasswordHash = userPassHash, PasswordSalt = userPassSalt, Role = "premium", LoginTries = 0, AccountLocked = false, DateLocked = DateTime.UtcNow, Newsletter = false }
            ); ; ;


            // Set default User role as basic.
            // Alternatively in User.cs use     public string Role { get; set; } = "basic";
            modelBuilder.Entity<User>()
                .Property(User => User.Role).HasDefaultValue("basic");
        }


    }
}
