using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {       
            try{
                // Get all classes that a student is enrolled in
                var query = from courses in db.Courses
                            join classes in db.Classes on courses.CatalogId equals classes.Listing
                            join enrollments in db.Enrolleds on classes.ClassId equals enrollments.Class
                            where enrollments.Student == uid
                            select new 
                            {
                                subject = courses.Department,
                                number = courses.Number,
                                name = courses.Name,
                                season = classes.Season,
                                year = classes.Year,
                                grade = enrollments.Grade ?? "--"
                            };
                return Json(query.ToArray());
            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.Message);
                return Json(null);
            }    
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {

            var assmtsQuery = from courses in db.Courses
                              join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                              from join1 in j1.DefaultIfEmpty()
                              where join1.Season == season && join1.Year == year && courses.Number == num && courses.Department == subject
                              join assCat in db.AssignmentCategories on join1.ClassId equals assCat.InClass into j2
                              from join2 in j2.DefaultIfEmpty()
                              join asgnmt in db.Assignments on join2.CategoryId equals asgnmt.Category into j3
                              from join3 in j3.DefaultIfEmpty()
                              select join3;

           

                var query = from assmnt in assmtsQuery
                            join submsns in db.Submissions on new { a = assmnt.AssignmentId, b = uid } equals new { a = submsns.Assignment, b = submsns.Student } into j1
                            from join1 in j1.DefaultIfEmpty()

                            select new

                            {
                                aname = assmnt== null ? null : assmnt.Name,
                                cname = assmnt == null ? null : assmnt.CategoryNavigation.Name,
                                due = assmnt== null ? null : (DateTime?)assmnt.Due,
                                score = join1 == null ? null : (uint?)join1.Score

                                //get all submissions with submission content  =>  submission has been made




                            };

            
            
            foreach(var q in query) {
                if (q.aname == null) {

                    return Json(Array.Empty<string>());

                }
            }



            return Json(query.ToArray());





        }

        //try{
        //    // Get all assignments from a given course
        //    var assignmentQuery = from courses in db.Courses
        //                join classes in db.Classes on courses.CatalogId equals classes.Listing
        //                join assignments in db.Assignments on classes.Listing equals assignments.Category
        //                where courses.Department == subject && courses.Number == num && classes.Season == season && classes.Year == year
        //                select assignments;

        //    var query = from q in assignmentQuery  // assignmentQuery holds the assignments for the class
        //                join s in db.Submissions
        //                on new { A = q.AssignmentId, B = uid } equals new { A = s.Assignment, B = s.Student } into joined
        //                from j in joined.DefaultIfEmpty()
        //                select new 
        //                {
        //                    aname = q.Name,
        //                    cname = q.Category,
        //                    due = q.Due,
        //                    score = j == null ? null : (uint?)j.Score // if j is null, then the student has not submitted to this assignment
        //                };
        //    return Json(query.ToArray());
        //}
        //catch(Exception e){
        //    System.Diagnostics.Debug.WriteLine(e.Message);
        //    return Json(null);
        //}
    //}

        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
                  string category, string asgname, string uid, string contents)
        {
            try
            {
                // Get the assignment
                var assignmentQuery = from courses in db.Courses
                    join classes in db.Classes on courses.CatalogId equals classes.Listing
                    join assignments in db.Assignments on classes.Listing equals assignments.Category
                    join assignmentCategories in db.AssignmentCategories on assignments.Category equals assignmentCategories.CategoryId
                    where courses.Department == subject && courses.Number == num && classes.Season == season && classes.Year == year &&
                        assignments.Name == asgname && assignmentCategories.Name == category
                    select assignments;

                // Get the submission
                var query = from q in assignmentQuery
                             join s in db.Submissions
                             on new { A = q.AssignmentId, B = uid } equals new { A = s.Assignment, B = s.Student } into joined
                             from j in joined.DefaultIfEmpty()
                             select j;

                // If the student has not submitted to this assignment, create a new submission
                if (!query.Any())
                {
                    Submission newSubmission = new()
                    {
                        Assignment = assignmentQuery.First().AssignmentId,
                        Student = uid,
                        SubmissionContents = contents,
                        Time = DateTime.Now,
                        Score = 0
                    };
                    db.Submissions.Add(newSubmission);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                // Otherwise, update the submission
                else
                {
                    Submission submission = query.First();
                    submission.SubmissionContents = contents;
                    submission.Time = DateTime.Now;
                    db.SaveChanges();
                    return Json(new { success = true });
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {    
            try{      
                // Check if the student is already enrolled in the class specified by subject, num, season, and year
                var enrollmentCheck = from courses in db.Courses
                            join classes in db.Classes on courses.CatalogId equals classes.Listing
                            join enrollments in db.Enrolleds on classes.ClassId equals enrollments.Class
                            where enrollments.Student == uid && courses.Department == subject && courses.Number == num && classes.Season == season && classes.Year == year
                            select enrollments;

                // If the student is already enrolled, return false
                if(enrollmentCheck.Any()){
                    return Json(new { success = false });
                }

                // Get the class
                var query = from courses in db.Courses
                            join classes in db.Classes on courses.CatalogId equals classes.Listing
                            where courses.Department == subject && courses.Number == num && classes.Season == season && classes.Year == year
                            select classes;

                // Create a new enrollment
                Enrolled newEnrollment = new()
                {
                    Class = query.First().ClassId,
                    Student = uid,
                    Grade = "--"
                };
                db.Enrolleds.Add(newEnrollment);
                db.SaveChanges();
                return Json(new { success = true });

            }
            catch(Exception e){
                System.Diagnostics.Debug.WriteLine(e.Message);
                return Json(new { success = false});
            }
        }

        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {   
            try
            {  
                // Check if the student is enrolled in any classes
                var checkEnrollment = db.Enrolleds.Where(e => e.Student == uid);
                // If the student is not enrolled in any classes, return 0.0
                if (!checkEnrollment.Any())
                {
                    return Json(new { gpa = 0.0 });
                }
     
                // Get every grade for every class that the student is enrolled in
                var query = from enrollments in db.Enrolleds
                            join classes in db.Classes on enrollments.Class equals classes.ClassId
                            join courses in db.Courses on classes.Listing equals courses.CatalogId
                            where enrollments.Student == uid
                            select new 
                            {
                                grade = enrollments.Grade,
                            };

                // Calculate the GPA
                double totalPoints = 0.0;
                int numClasses = 0;
                foreach (var grade in query)
                {
                    switch (grade.grade)
                    {
                        case "A":
                            totalPoints += 4.0;
                            numClasses++;
                            break;
                        case "A-":
                            totalPoints += 3.7;
                            numClasses++;
                            break;
                        case "B+":
                            totalPoints += 3.3;
                            numClasses++;
                            break;
                        case "B":
                            totalPoints += 3.0;
                            numClasses++;
                            break;
                        case "B-":
                            totalPoints += 2.7;
                            numClasses++;
                            break;
                        case "C+":
                            totalPoints += 2.3;
                            numClasses++;
                            break;
                        case "C":
                            totalPoints += 2.0;
                            numClasses++;
                            break;
                        case "C-":
                            totalPoints += 1.7;
                            numClasses++;
                            break;
                        case "D+":
                            totalPoints += 1.3;
                            numClasses++;
                            break;
                        case "D":
                            totalPoints += 1.0;
                            numClasses++;
                            break;
                        case "D-":
                            totalPoints += 0.7;
                            numClasses++;
                            break;
                        case "E":
                            totalPoints += 0.0;
                            numClasses++;
                            break;
                        case "--":
                            // Do nothing. Don't count this class in the GPA
                            break;
                        default:
                            // Also do nothing
                            break;
                    }
                }
                if (numClasses == 0)
                {
                    return Json(new { gpa = 0.0 });
                }
                double gpa = totalPoints / numClasses;
                return Json(new { gpa });
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return Json(null);
            }
        }
                
        /*******End code to modify********/

    }
}

