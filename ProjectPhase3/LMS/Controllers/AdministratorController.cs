﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
            {
                // Data validation
                if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(name))
                {
                    return Json(new { success = false });
                }
                // Check if a department with the same subject already exists
                try
                {
                    var existingDepartment = db.Departments.FirstOrDefault(d => d.Subject == subject);
                    if (existingDepartment != null)
                    {
                        return Json(new { success = false });
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);;
                    return Json(new { success = false });
                }

                // Create a new department
                try
                {
                    Department newDepartment = new()
                    {
                        Subject = subject,
                        Name = name
                    };
                    db.Departments.Add(newDepartment);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);;
                    return Json(new { success = false });
                }
            }

        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            // Pull all courses from the database in the given department
            try
            {
                var query =
                    from course in db.Courses
                    where course.Department == subject
                    select new
                    {
                        number = course.Number,
                        name = course.Name
                    };
                return Json(query.ToArray());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);;
                return Json(null);
            }
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            // Pull all professors from the database in the given department
            try
            {
                var query =
                    from professor in db.Professors
                    where professor.WorksIn == subject
                    select new
                    {
                        lname = professor.LName,
                        fname = professor.FName,
                        uid = professor.UId
                    };
                return Json(query.ToArray());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);;
                return Json(null);
            } 
        }

        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {         
            // Data validation 
            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(name) || number <= 0)
                {
                    return Json(new { success = false });
                }
            // Check if a course with the same number and subject already exists
            try
            {
                var query =
                    from course in db.Courses
                    where course.Department == subject && course.Number == number
                    select course;
                if (query.Any())
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);;
                return Json(new { success = false });
            }

            // Create a new course
            try
            {
                Course newCourse = new()
                {
                    Department = subject,
                    Number = (uint)number,
                    Name = name
                };
                db.Courses.Add(newCourse);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);;
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {        
            // Data validation
            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(season) || string.IsNullOrEmpty(location) || string.IsNullOrEmpty(instructor) || start >= end)
            {
                return Json(new { success = false });
            }

            try
            {
                // Check if a class with the same course and semester already exists
                var existingClass =
                    from course in db.Courses
                    join classOffering in db.Classes on course.CatalogId equals classOffering.Listing
                    where course.Department == subject && course.Number == number && classOffering.Season == season && classOffering.Year == year
                    select classOffering;

                if (existingClass.Any()){
                    return Json(new { success = false });
                }
                // Check if another class occupies the same location during any time within the start-end range in the same semester
                var existingClassAtLocation =
                    from classOffering in db.Classes
                    where classOffering.Season == season && classOffering.Year == year && classOffering.Location == location
                    select classOffering;

                if (existingClassAtLocation.Any())
                {
                    foreach (var classOffering in existingClassAtLocation)
                    {
                        if (classOffering.StartTime < TimeOnly.FromDateTime(end) && classOffering.EndTime > TimeOnly.FromDateTime(start))
                        {
                            return Json(new { success = false });
                        }
                    }
                }
                // Check if the professor is teaching another class at the same time
                var existingClassWithProfessor =
                    from classOffering in db.Classes
                    where classOffering.Season == season && classOffering.Year == year && classOffering.TaughtBy == instructor
                    select classOffering;

                if (existingClassWithProfessor.Any()){
                    foreach (var classOffering in existingClassWithProfessor)
                    {
                        if (classOffering.StartTime < TimeOnly.FromDateTime(end) && classOffering.EndTime > TimeOnly.FromDateTime(start))
                        {
                            return Json(new { success = false });
                        }
                    }
                }

                // Get CatalogId of the course to add as class listing
                var catalogIdQuery =
                    from course in db.Courses
                    where course.Department == subject && course.Number == number
                    select course.CatalogId;

                // Create a new class offering
                Class newClass = new()
                {
                    Season = season,
                    Year = (uint)year,
                    Location = location,
                    StartTime = TimeOnly.FromDateTime(start),
                    EndTime = TimeOnly.FromDateTime(end),
                    Listing = catalogIdQuery.First(),
                    TaughtBy = instructor
                };

                db.Classes.Add(newClass);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);;
                return Json(new { success = false });
            }
        }

        /*******End code to modify********/

    }
}

