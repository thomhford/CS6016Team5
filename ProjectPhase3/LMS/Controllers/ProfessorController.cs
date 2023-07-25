using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from enrolled in db.Enrolleds
                        join classes in db.Classes on enrolled.Class equals classes.ClassId into j1
                       
                        from join1 in j1.DefaultIfEmpty()
                        join courses in db.Courses on join1.ClassId equals courses.CatalogId 
                        where courses.Number == num  && join1.Season == season && join1.Year==year && courses.Department==subject
                        join students in db.Students on enrolled.Student equals students.UId
                        select new
                        {
                            fname = students.FName,
                            lname = students.LName,
                            uid = students.UId,
                            dob = students.Dob,
                            grade = enrolled.Grade

                        };




            //        var query =
            //from person in people
            //join cat in cats on person equals cat.Owner
            //join dog in dogs on new
            //{
            //    Owner = person,
            //    Letter = cat.Name.Substring(0, 1)
            //} equals new
            //{
            //    dog.Owner,
            //    Letter = dog.Name.Substring(0, 1)
            //}
            //select new
            //{
            //    CatName = cat.Name,
            //    DogName = dog.Name
            //};



            //var query=




            //from courses in db.Courses
            //join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
            //where courses.Department == subject && courses.Number == num

            //from join1 in j1.DefaultIfEmpty()
            //join enrolled in db.Enrolleds on join1.ClassId equals enrolled.Class into j2



            //where join1.Season == season && join1.Year == year
            //from join2 in j2.DefaultIfEmpty()

            //join students in db.Students on join2.Student equals students.UId into j3
            //from join3 in j3.DefaultIfEmpty()

            //select new
            //{
            //    fname = join3.FName,
            //    lname = join3.LName,
            //    uid = join3.UId,
            //    dob = join3.Dob,
            //    grade = join2.Grade

            //};

            return Json(null);
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {

            //var query1 = from courses in db.Courses
            //            join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
            //            from join1 in j1.DefaultIfEmpty()

            //            join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j2
            //            where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year
            //            from join2 in j2.DefaultIfEmpty()
            //            join assgnmts in db.Assignments on join2.CategoryId equals assgnmts.AssignmentId into j3
            //            where join2.Name == category
            //            from join3 in j3.DefaultIfEmpty()

            //            select new
            //            {
            //                aname = join2.Name,
            //                cname = category,
            //                due = join3.Due,
            //                submissions = from students in db.Students
            //                                join submissions in db.Submissions on students.UId equals submissions.Student
            //                                where submissions.Assignment == join3.AssignmentId
            //                                select students.UId.Count()


            //            };




            //var query2 = from courses in db.Courses
            //            join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
            //            from join1 in j1.DefaultIfEmpty()

            //            join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j2
            //            where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year
            //            from join2 in j2.DefaultIfEmpty()
            //            join assgnmts in db.Assignments on join2.CategoryId equals assgnmts.AssignmentId into j3
            //            from join3 in j3.DefaultIfEmpty()


            //            select new
            //            {
            //                aname = join2.Name,
            //                cname = join3.Name,
            //                due = join3.Due,
            //                submissions = from students in db.Students
            //                                join submissions in db.Submissions on students.UId equals submissions.Student
            //                                where submissions.Assignment == join3.AssignmentId
            //                                select students.UId.Count()
            //            };


            //if (category != null) {
            //    return Json(query2.ToArray());
            //}



            //else {

            //    return Json(query1.ToArray());
            //}


            return Json(null);


        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {

            var query = from courses in db.Courses
                         join classes in db.Classes on courses.CatalogId equals classes.Listing
                         into j1
                         from join1 in j1.DefaultIfEmpty()

                         join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j2
                         where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year
                         from join2 in j2.DefaultIfEmpty()
                        

                         select new
                         {
                             name = join2.Name,
                             weight = join2.Weight,
                            
                         };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {

            //try
            //{
            //    var checkAssignmentCat = from courses in db.Courses
            //                             join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
            //                             from join1 in j1.DefaultIfEmpty()

            //                             join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass
            //                             where assgnmtCat.Name == category && courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year
            //                             select assgnmtCat;
            //    if (checkAssignmentCat.Any())
            //    {

            //        return Json(new { success = false });
            //    }

            //    var query = from courses in db.Courses
            //                join classes in db.Classes on courses.CatalogId equals classes.Listing
            //                where courses.Department == subject && courses.Number == num && classes.Season == season && classes.Year == year
            //                select classes;



            //    AssignmentCategory asc = new()
            //    {
            //        Name = category,
            //        Weight = (uint)catweight,
            //        InClass = query.First().ClassId


            //    };
            //    db.AssignmentCategories.Add(asc);
            //    db.SaveChanges();
            //    return Json(new { success = true });

            //}

            //catch (Exception e)
            //{
            //    System.Diagnostics.Debug.WriteLine(e.Message);
            //    return Json(new { success = false });
            //}


            return Json(new { success = false });

        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {

            //try
            //{
            //    var checkAssignment = from courses in db.Courses
            //                          join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
            //                          from join1 in j1.DefaultIfEmpty()

            //                          join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j2
            //                          where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year
            //                          from join2 in j2.DefaultIfEmpty()
            //                          join assgnmts in db.Assignments on join2.CategoryId equals assgnmts.AssignmentId into j3

            //                          from join3 in j3.DefaultIfEmpty()
            //                          where join3.Name == category
            //                          select join3;
            //    if (checkAssignment.Any())
            //    {

            //        return Json(new { success = false });
            //    }

            //    var query = from courses in db.Courses
            //                join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
            //                from join1 in j1.DefaultIfEmpty()

            //                join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j2
            //                where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year
            //                from join2 in j2.DefaultIfEmpty()
            //                select join2;


            //    Assignment assmnt = new()
            //    {
            //        Category = query.First().CategoryId,
            //        Name = asgname,
            //        Due = asgdue,
            //        Contents = asgcontents,
            //        MaxPoints =(uint) asgpoints

            //    };



            //db.Assignments.Add(assmnt);
            //db.SaveChanges();

        

            return Json(new { success = false });

        //}

        //catch (Exception e)
        //{
        //    System.Diagnostics.Debug.WriteLine(e.Message);
        //    return Json(new { success = false });
        //}
}





            //return Json(new { success = false });
        


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {

            try {
                var query = from courses in db.Courses
                            join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                            from join1 in j1.DefaultIfEmpty()

                            join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j2
                            where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year
                            from join2 in j2.DefaultIfEmpty()
                            join assgnmts in db.Assignments on join2.CategoryId equals assgnmts.AssignmentId into j3
                            where join2.Name == category
                            from join3 in j3.DefaultIfEmpty()
                            join submission in db.Submissions on join3.AssignmentId equals submission.Assignment into j4
                            where join3.Name == asgname
                            from join4 in j4.DefaultIfEmpty()
                            join students in db.Students on join4.Student equals students.UId into j5
                            from join5 in j5.DefaultIfEmpty()
                            select new
                            {
                                fname = join5.FName,
                                lname = join5.LName,
                                uid = join5.UId,
                                time = join4.Time,
                                score = join4.Score


                            };
                return Json(query.ToArray());





            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return Json(null);
            }

        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            return Json(new { success = false });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from classes in db.Classes
                        join courses in db.Courses on classes.Listing equals courses.CatalogId into j1
                        from join1 in j1.DefaultIfEmpty()
                        join departments in db.Departments on join1.Department equals departments.Subject
                        where classes.TaughtBy ==uid
                   
                        select new
                        {
                            subject = join1.Department,
                            number = join1.Number,
                            name = join1.Name,
                            season = classes.Season,
                            year = classes.Year
                        };
            return Json(query.ToArray());
        }


        
        /*******End code to modify********/
    }
}

