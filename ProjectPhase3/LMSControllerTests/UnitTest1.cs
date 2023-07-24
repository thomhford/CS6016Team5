using LMS.Controllers;
using LMS.Models.LMSModels;
using LMS_CustomIdentity.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LMSControllerTests
{
    public class UnitTest1
    {
        [Fact]
        public void GetDepartmentsTest()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new(MakeTinyDB());

            var allDepts = ctrl.GetDepartments() as JsonResult;

            dynamic x = allDepts.Value;

            Assert.Equal(1, x.Length);
            Assert.Equal("CS", x[0].subject);
        }

        [Fact]
        public void GetCatalogTest()
        {
            CommonController ctrl = new(MakeTinyDB());

            var allCourses = ctrl.GetCatalog() as JsonResult;

            dynamic x = allCourses.Value;

            Assert.Equal(1, x.Length);
            Assert.Equal("CS", x[0].cname);
        }


        /// <summary>
        /// Make a very tiny in-memory database, containing just one department
        /// and nothing else.
        /// </summary>
        /// <returns></returns>
        LMSContext MakeTinyDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Administrators.Add(new Administrator { UId = "00000000", FName = "Test", LName = "Admin", Dob = new DateOnly(2000, 1, 1) });
            db.Professors.Add(new Professor { UId = "11111111", FName = "Test", LName = "Professor", Dob = new DateOnly(2000, 1, 1), WorksIn = "KSoC" });
            db.Students.Add(new Student { UId = "22222222", FName = "Test", LName = "Student", Dob = new DateOnly(2000, 1, 1), Major = "CS" });

            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });
            db.Courses.Add(new Course { Number = 1000, Name = "CS", Department = "KSoC" });
            db.Classes.Add(new Class { Season = "Fall", Year = 2019, Location = "EC 105", StartTime = new TimeOnly(10, 0, 0), EndTime = new TimeOnly(11, 0, 0), Listing = 1000, TaughtBy = "11111111" });

            // TODO: add more objects to the test database

            db.SaveChanges();

            return db;
        }

        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }

    }
}