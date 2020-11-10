using ACCore.Logging;
using Serilog;
using Serilog.Context;
using System;

namespace ACCore.TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            LogContext.PushProperty("EnvUsername", "test");

            Log.Logger = AuditLogInitializer.CreateLoggerConfiguration().CreateLogger();

            using (var db = new ProjectDbContext())
            {

                var person = new Person()
                {
                    Id = Guid.NewGuid(),
                    Name = "John",
                    Title = "Junior Developer"
                };

                db.People.Add(person);
                db.SaveChanges();

                person.Title = "Senior Developer";

                db.People.Update(person);
                db.SaveChanges();

                db.People.Remove(person);
                db.SaveChanges();
            }
        }
    }
}
