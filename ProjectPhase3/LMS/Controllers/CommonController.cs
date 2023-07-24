using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {          
            // Get all departments from the database
            try{
                var query =
                    from departments in db.Departments
                    select new
                    {
                        name = departments.Name,
                        subject = departments.Subject
                    };
                return Json(query.ToArray());
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.Message);;
                return Json(null);
            }
        }

        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            try
            {
                var query =
                    from departments in db.Departments
                    select new
                    {
                        subject = departments.Subject,
                        dname = departments.Name,
                        courses = (
                            from courses in db.Courses
                            where courses.Department == departments.Subject
                            select new
                            {
                                number = courses.Number,
                                cname = courses.Name
                            }
                        ).ToList() // Convert the inner query to IEnumerable
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
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {      
            // Get all class offerings of a specific course from the database
            try{
                var query =
                    from courses in db.Courses
                    join classes in db.Classes on courses.CatalogId equals classes.Listing
                    join professors in db.Professors on classes.TaughtBy equals professors.UId
                    where courses.Department == subject && courses.Number == number
                    select new
                    {
                        season = classes.Season,
                        year = classes.Year,
                        location = classes.Location,
                        start = classes.StartTime,
                        end = classes.EndTime,
                        fname = professors.FName,
                        lname = professors.LName
                    };
                return Json(query.ToArray());
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.Message);;
                return Json(null);
            }  
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {    
            // Get the assignment contents from the database
            try{
                var query =
                    from courses in db.Courses
                    join classes in db.Classes on courses.CatalogId equals classes.Listing
                    join assignmentCategories in db.AssignmentCategories on classes.ClassId equals assignmentCategories.InClass
                    join a in db.Assignments on assignmentCategories.CategoryId equals a.Category
                    where courses.Department == subject && courses.Number == num && classes.Season == season && classes.Year == year &&
                        assignmentCategories.Name == category && a.Name == asgname
                    select new
                    {
                        contents = a.Contents
                    };
                return Content(query.ToArray()[0].contents);
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.Message);;
                return Content("");
            }        
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {       
            // Get the submission text from the database (if it exists)
            try{
                var query =
                    from courses in db.Courses
                    join classes in db.Classes on courses.CatalogId equals classes.Listing
                    join assignmentCategories in db.AssignmentCategories on classes.ClassId equals assignmentCategories.InClass
                    join assignments in db.Assignments on assignmentCategories.CategoryId equals assignments.Category
                    join submissions in db.Submissions on assignments.AssignmentId equals submissions.Assignment
                    where courses.Department == subject && courses.Number == num && classes.Season == season && classes.Year == year &&
                        assignmentCategories.Name == category && assignments.Name == asgname && submissions.Student == uid
                    select new
                    {
                        contents = submissions.SubmissionContents
                    };
                return Content(query.ToArray()[0].contents);
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.Message);;
                return Content("");
            }
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            try
            {
                // Search for the UId in each table separately
                var adminQuery =
                    from admin in db.Administrators
                    where admin.UId == uid
                    select new
                    {
                        fname = admin.FName,
                        lname = admin.LName,
                        uid = admin.UId
                    };

                var professorQuery =
                    from professor in db.Professors
                    where professor.UId == uid
                    select new
                    {
                        fname = professor.FName,
                        lname = professor.LName,
                        uid = professor.UId,
                        department = (
                            from d in db.Departments
                            where d.Subject == professor.WorksIn
                            select d.Name
                        ).FirstOrDefault()
                    };

                var studentQuery =
                    from student in db.Students
                    where student.UId == uid
                    select new
                    {
                        fname = student.FName,
                        lname = student.LName,
                        uid = student.UId,
                        department = (
                            from d in db.Departments
                            where d.Subject == student.Major
                            select d.Name
                        ).FirstOrDefault()
                    };

                // Check if the user exists in each table and create the appropriate response
                if (adminQuery.Any())
                {
                    return Json(adminQuery.First());
                }
                else if (professorQuery.Any())
                {
                    return Json(professorQuery.First());
                }
                else if (studentQuery.Any())
                {
                    return Json(studentQuery.First());
                }
                else
                {
                    // If the UId doesn't exist in any table, return {success: false}
                    return Json(new { success = false });
                }
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

