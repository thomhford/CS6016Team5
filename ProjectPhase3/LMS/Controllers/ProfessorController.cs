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
using NuGet.ContentModel;
using static System.Formats.Asn1.AsnWriter;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For Testing Professor Controller:
//student  u9441588 pass 123
//Professor u9638158 pass 123

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
                        join students in db.Students on enrolled.Student equals students.UId into j1
                        from join1 in j1.DefaultIfEmpty()
                        join classes in db.Classes on enrolled.Class equals classes.ClassId into j2
                        from join2 in j2.DefaultIfEmpty()
                        where join2.Season == season && join2.Year == year
                        join courses in db.Courses on join2.Listing equals courses.CatalogId into j3
                        from join3 in j3.DefaultIfEmpty()
                        where join3.Department == subject && join3.Number == num
                        select new
                        {
                       
                            fname = join1 == null ? null: join1.FName,
                            lname = join1 == null ?null:  join1.LName,
                            uid = join1 == null ? null: join1.UId,
                            dob = join1.Dob,
                            grade = enrolled.Grade
                        };
            foreach (var q in query)
            {
                if (q.fname == null)
                {

                    return Json(Array.Empty<string>());

                }
            }

            return Json(query.ToArray());
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
         
            if (category == null)
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



                var query= from assmnt in assmtsQuery

                           select new  

                            {
                               aname = assmnt == null ? null : assmnt.Name,
                               cname =  assmnt.CategoryNavigation == null ? null: assmnt.CategoryNavigation.Name,
                               due = assmnt == null ? null : (DateTime?)assmnt.Due,
                             
                               submissions = assmnt.Submissions.Count()
                     

                           };
                foreach (var q in query)
                {
                    if (q.aname == null)
                    {

                        return Json(Array.Empty<string>());

                    }
                }


                return Json(query.ToArray());


            }
            else { //assignments in a specific category when category is specified


                var assmtsQuery = from courses in db.Courses
                                  join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                                  from join1 in j1.DefaultIfEmpty()
                                  where join1.Season == season && join1.Year == year && courses.Number == num && courses.Department == subject
                                  join assCat in db.AssignmentCategories on join1.ClassId equals assCat.InClass into j2
                                  from join2 in j2.DefaultIfEmpty()
                                  join asgnmt in db.Assignments on join2.CategoryId equals asgnmt.Category into j3
                                  from join3 in j3.DefaultIfEmpty()
                                  where join3.CategoryNavigation.Name == category
                                  select join3;
                var query = from assmnt in assmtsQuery


                            select new
                            {
                                aname = assmnt == null ? null: assmnt.Name,
                                cname = category,
                                due = assmnt == null ? null : (DateTime?)assmnt.Due,
                             
                                submissions =assmnt.Submissions.Count()
                             
                            
                            };

                foreach (var q in query)
                {
                    if (q.aname == null)
                    {

                        return Json(Array.Empty<string>());

                    }
                }

                return Json(query.ToArray());


            }



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
                        where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year

                        join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j2
                        from join2 in j2.DefaultIfEmpty()


                        select new
                        {
                           
                            name = join2==null?null: join2.Name,
                            weight = join2 == null ? null : (uint?)join2.Weight,

                        };

            foreach (var q in query)
            {
                if (q.name == null)
                {

                    return Json(Array.Empty<string>());

                }
            }

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

            try
            {
                var checkAssignmentCat = from courses in db.Courses
                                         join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                                         from join1 in j1.DefaultIfEmpty()
                                         where courses.Number == num && courses.Department == subject && join1.Season == season && join1.Year == year
                                         join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j2
                                       
                                         from join2 in j2.DefaultIfEmpty()
                                         where join2.Name == category

                                         select join2;

             

                

                if (checkAssignmentCat.Any())
                {

                    return Json(new { success = false });
                }

                var query = from courses in db.Courses
                            join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                            from join1 in j1.DefaultIfEmpty()
                            where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year
                            select join1;



                AssignmentCategory asc = new()
                {
                    Name = category,
                    Weight = (uint)catweight,
                    InClass = query.First().ClassId


                };
                db.AssignmentCategories.Add(asc);
                db.SaveChanges();
             
                return Json(new { success = true });

            }

            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return Json(new { success = false });
            }


        }
                  


private void UpdateGradeAfterNewAssignment(string subject, int num, string season, int year) {

            var geclassQuery = from c in db.Classes
                               where c.ListingNavigation.DepartmentNavigation.Subject == subject
                               where c.Season == season
                               where c.Year == year
                               where c.Listing == num
                               select c;



            var getAllEnrolledStdntQuery = from en in db.Enrolleds

                                           where en.ClassNavigation.ListingNavigation.Department == subject
                                           where en.ClassNavigation.ListingNavigation.Number == num
                                           where en.ClassNavigation.Season == season
                                           where en.ClassNavigation.Year == year

                                           where en.Class == geclassQuery.First().ClassId
                                           select en;
            if (getAllEnrolledStdntQuery.Any()) {

                foreach(var enrolment in getAllEnrolledStdntQuery.ToList()) {
                    string uid = enrolment.Student;

                    UpdateGrade(subject, num, season, year, uid);
                }
            }
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

            try
            {
                var checkAssignment = from courses in db.Courses
                                      join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                                      from join1 in j1.DefaultIfEmpty()
                                      where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year


                                      join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j2
                                      from join2 in j2.DefaultIfEmpty()
                                      join assgnmts in db.Assignments on join2.CategoryId equals assgnmts.AssignmentId into j3

                                      from join3 in j3.DefaultIfEmpty()
                                      where join3.Name == category
                                      select join3;
                if (checkAssignment.Any())
                {

                    return Json(new { success = false });
                }

                var query = from courses in db.Courses
                            join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                            from join1 in j1.DefaultIfEmpty()
                            where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year


                            join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j2
                            from join2 in j2.DefaultIfEmpty()
                            where join2.Name == category
                            select join2;


                Assignment assmnt = new()
                {
                    Category = query.First().CategoryId,
                    Name = asgname,
                    Due = asgdue,
                    Contents = asgcontents,
                    MaxPoints = (uint)asgpoints

                };



                db.Assignments.Add(assmnt); //assignment created
                db.SaveChanges();

             
                UpdateGradeAfterNewAssignment(subject, num, season, year);

                return Json(new { success = true });

            }

            catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine(e.Message);
            return Json(new { success = false });
        }
}



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
            var query = from B in db.Submissions
                        where B.AssignmentNavigation.CategoryNavigation.InClassNavigation.ListingNavigation.Department == subject
                        where B.AssignmentNavigation.CategoryNavigation.InClassNavigation.Season == season
                        where B.AssignmentNavigation.CategoryNavigation.InClassNavigation.Year == year
                        where B.AssignmentNavigation.CategoryNavigation.Name == category
                        where B.AssignmentNavigation.CategoryNavigation.InClassNavigation.ListingNavigation.Number == num
                        where B.AssignmentNavigation.Name == asgname
                        select new
                        {
                            fname = B.StudentNavigation.FName,
                            lname = B.StudentNavigation.LName,
                            uid = B.StudentNavigation.UId,
                            time = B.Time,
                            score = B.Score

                        };

           
            if (query.Any()) {

               
                return Json(query.ToArray());


            }
          

            return Json(Array.Empty<string>());

        }






        //helper method to calculate student's new grade for the class after an assignment is graded

        private void UpdateGrade(string subject, int num, string season, int year, string uid)
        {


            var getAssCatQuery = from e in db.Enrolleds
                                 where e.ClassNavigation.ListingNavigation.Department == subject
                                 where e.ClassNavigation.ListingNavigation.Number == num
                                 where e.ClassNavigation.Season == season
                                 where e.ClassNavigation.Year == year
                                 where e.Student == uid

                                 select e.ClassNavigation.AssignmentCategories;



            if (getAssCatQuery.Any())
            {

                int maxCtgW = 0;

                float ttlScr = 0;
                int ttlPpnts = 0;
                float finalGrd = 0;
                string lttrGrd = "";
                int countCtg = 0;

                foreach (var c in getAssCatQuery.ToList())
                {

                    var getAssignmentsQuery = from a in db.Assignments
                                              where a.CategoryNavigation.InClassNavigation.ListingNavigation.Department == subject

                                              where a.CategoryNavigation.InClassNavigation.ListingNavigation.Number == num
                                              where a.CategoryNavigation.InClassNavigation.Season == season
                                              where a.CategoryNavigation.InClassNavigation.Year == year

                                              where a.Category == c.ToArray()[countCtg].CategoryId
                                              select a;


                    if (getAssignmentsQuery.Any())
                    {



                        foreach (var asm in getAssignmentsQuery.ToList())
                        {

                            var getSubmissionQuery = from s in db.Submissions
                                                     where s.AssignmentNavigation.CategoryNavigation.InClassNavigation.ListingNavigation.Department == subject
                                                     where s.AssignmentNavigation.CategoryNavigation.InClassNavigation.ListingNavigation.Number == num
                                                     where s.AssignmentNavigation.CategoryNavigation.InClassNavigation.Season == season
                                                     where s.AssignmentNavigation.CategoryNavigation.InClassNavigation.Year == year
                                                     where s.AssignmentNavigation.CategoryNavigation.CategoryId == asm.CategoryNavigation.CategoryId
                                                     where s.Assignment == asm.AssignmentId
                                                     where s.Student == uid
                                                     select s;

                            if (getAssignmentsQuery.Any())
                            {

                                ttlScr += getSubmissionQuery.First().Score;
                            }
                            else
                            {
                                ttlScr += 0;

                            }
                            ttlPpnts += (int)asm.MaxPoints;


                        }
                        maxCtgW += (int)c.ToArray()[countCtg].Weight;
                        countCtg++;

                    }

                }



                float scaleFctr = 100 / (float)maxCtgW;
                finalGrd = (ttlScr / ttlPpnts) * 100 * scaleFctr;
                lttrGrd = ConvertToLetterGrade(finalGrd);



                var getEnrolledQuery = from e in db.Enrolleds
                                       where e.ClassNavigation.ListingNavigation.Department == subject
                                       where e.ClassNavigation.ListingNavigation.Number == num
                                       where e.ClassNavigation.Season == season
                                       where e.ClassNavigation.Year == year
                                       where e.Student == uid
                                       select e;
                if (getEnrolledQuery.Any())
                {
                    getEnrolledQuery.First().Grade = lttrGrd;
                    db.SaveChanges();

                }


            }
        }
        

        


        private static string ConvertToLetterGrade(float finalGrd) {
            string lttrGrd = "";
            if (finalGrd >= 93 )
            {
                lttrGrd += "A";

            }
            else if (finalGrd >= 90)
            {
                lttrGrd += "A-";
            }
            else if (finalGrd >= 87)
            {
                lttrGrd += "B+";
            }
            else if (finalGrd >= 83)
            {
                lttrGrd += "B";
            }
            else if (finalGrd >= 80)
            {
                lttrGrd += "B-";
            }
            else if (finalGrd >= 77)
            {
                lttrGrd += "C+";
            }
            else if (finalGrd >= 73)
            {
                lttrGrd += "C";
            }
            else if (finalGrd >= 70)
            {
                lttrGrd += "C-";
            }
            else if (finalGrd >= 67)
            {
                lttrGrd += "D+";
            }
            else if (finalGrd >= 63)
            {
                lttrGrd += "D";
            }
            else if (finalGrd >= 60)
            {
                lttrGrd += "D-";
            }
            else if (finalGrd >= 0)
            {
                lttrGrd += "E";
            }

            return lttrGrd;

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
            

                var query = from B in db.Submissions
                            where B.AssignmentNavigation.CategoryNavigation.InClassNavigation.ListingNavigation.Department == subject
                            where B.AssignmentNavigation.CategoryNavigation.InClassNavigation.Season == season
                            where B.AssignmentNavigation.CategoryNavigation.InClassNavigation.Year == year
                            where B.AssignmentNavigation.CategoryNavigation.Name == category
                            where B.AssignmentNavigation.CategoryNavigation.InClassNavigation.ListingNavigation.Number == num
                            where B.AssignmentNavigation.Name == asgname
                            where B.Student == uid
                            select B;
                query.First().Score = (uint)score;
                db.SaveChanges();
                UpdateGrade(subject, num, season, year, uid);

                return Json(new { success = true });

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
                            subject = join1==null? null:join1.Department,
                            number = join1 == null ? null : (uint?)join1.Number,
                            name = join1 == null ? null : join1.Name,
                            season = classes == null ? null : classes.Season,
                            year = classes == null ? null : (uint?)classes.Year
                        };
            foreach (var q in query)
            {
                if (q.subject == null)
                {

                    return Json(Array.Empty<string>());

                }
            }


            return Json(query.ToArray());
        }


        
        /*******End code to modify********/
    }
}

