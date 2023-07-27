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
using static System.Formats.Asn1.AsnWriter;
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
                        join students in db.Students on enrolled.Student equals students.UId into j1
                        from join1 in j1.DefaultIfEmpty()
                        join classes in db.Classes on enrolled.Class equals classes.ClassId into j2
                        from join2 in j2.DefaultIfEmpty()
                        where  join2.Season == season && join2.Year == year
                        join courses in db.Courses on join2.Listing equals courses.CatalogId into j3
                        from join3 in j3.DefaultIfEmpty()
                        where join3.Department == subject && join3.Number == num


                        select new
                        {


                            fname = join1.FName,
                            lname = join1.LName,
                            uid = join1.UId,
                            dob = join1.Dob,
                            grade = enrolled.Grade

                        };
            System.Diagnostics.Debug.WriteLine("MyClasses in Prof: " + query.ToString());
            System.Diagnostics.Debug.WriteLine("MyClasses in Prof: " + query.ToArray()[0]);


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
            //TODO; not working properly will be working on it does not select specific assignments when
            //category is given
            if (category == null) {


                var assmtsQuery = from courses in db.Courses
                                  join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                                  from join1 in j1.DefaultIfEmpty()
                                  where join1.Season == season && join1.Year == year && courses.Number == num && courses.Department == subject
                                  join assCat in db.AssignmentCategories on join1.ClassId equals assCat.InClass into j2
                                  from join2 in j2.DefaultIfEmpty()
                                  join asgnmt in db.Assignments on join2.CategoryId equals asgnmt.Category
                                  select asgnmt;
              


                var query = from courses in db.Courses
                            join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                            from join1 in j1.DefaultIfEmpty()
                            where join1.Season == season && join1.Year == year && courses.Number == num && courses.Department == subject 


                            join assCat in db.AssignmentCategories on join1.ClassId equals assCat.InClass into j2
                            from join2 in j2.DefaultIfEmpty()
                            join asgnmt in db.Assignments on join2.CategoryId equals asgnmt.Category //into j3
                      


                             select new  

                            {
                                aname = asgnmt.Name,
                                cname = join2.Name,
                                due = asgnmt.Due, 
                                 submissions = (from q in assmtsQuery
                                                join subs in db.Submissions on q.AssignmentId equals subs.Assignment into joined
                                                from joined1 in joined.DefaultIfEmpty()
                                                join stds in db.Students on joined1.Student equals stds.UId into joined2
                                                from joined3 in joined2.DefaultIfEmpty()
                                                where joined1.SubmissionContents!=null
                                                select joined3).Count()
                              

                             };

                foreach(var itm in query) {
                    System.Diagnostics.Debug.WriteLine("Get Assignments In Category in Prof: " +" Assgnmt name: "+ itm.aname+ " asCatName: "+ itm.cname);
                }

                return Json(query.ToArray());


            }
            else {


                var assmtsQuery = from courses in db.Courses
                                  join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                                  from join1 in j1.DefaultIfEmpty()
                                  where join1.Season == season && join1.Year == year && courses.Number == num && courses.Department == subject
                                  join assCat in db.AssignmentCategories on join1.ClassId equals assCat.InClass into j2
                                  from join2 in j2.DefaultIfEmpty()
                                  join asgnmt in db.Assignments on join2.CategoryId equals asgnmt.Category into j3
                                  from join3 in j3.DefaultIfEmpty()
                                  where join3.Name == category
                                  select join3;

                var query = from courses in db.Courses
                            join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                            from join1 in j1.DefaultIfEmpty()
                            where join1.Season == season && join1.Year == year && courses.Number == num && courses.Department == subject
                            join assCat in db.AssignmentCategories on join1.ClassId equals assCat.InClass into j2
                            from join2 in j2.DefaultIfEmpty()
                            join asgnmt in db.Assignments on join2.CategoryId equals asgnmt.Category into j3
                            from join3 in j3.DefaultIfEmpty()
                            where join3.Name == category



                            select new
                            {
                                aname = join2.Name,
                                cname = category,
                                due = join3.Due,
                                submissions = (from q in assmtsQuery
                                              join subs in db.Submissions on q.AssignmentId equals subs.Assignment into joined
                                              from joined1 in joined.DefaultIfEmpty()
                                              join stds in db.Students on joined1.Student equals stds.UId into joined2
                                              from joined3 in joined2.DefaultIfEmpty()
                                              where joined1.SubmissionContents!=null
                                              select joined3).Count()
                            
                            };

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
                  

        //helper method to score 0 to all students when new assignment is created
        private void UpdateScoreDefault(string subject, int num, string season, int year, string category, string asgname) {
            var query = from courses in db.Courses
                        join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                        from join1 in j1.DefaultIfEmpty()
                        where courses.Number == num && courses.Department == subject && join1.Season == season && join1.Year == year

                        join asCatg in db.AssignmentCategories on join1.ClassId equals asCatg.InClass into j2
                        from join2 in j2.DefaultIfEmpty()
                        where join2.Name == category
                        join asgmnt in db.Assignments on join2.CategoryId equals asgmnt.Category into j3
                        from join3 in j3.DefaultIfEmpty()
                        where join3.Name == asgname
                        join enrolled in db.Enrolleds on join1.ClassId equals enrolled.Class into j4
                        from join4 in j4.DefaultIfEmpty()
                        join students in db.Students on join4.Student equals students.UId into j5

                        from join5 in j5.DefaultIfEmpty()
                        join submsns in db.Submissions on new {a = join3.AssignmentId ,b =join5.UId} equals new {a=submsns.Assignment, b= submsns.Student} into j6
                        from join6 in j6.DefaultIfEmpty()
                        select join6;

            foreach(var s in query) {
                Submission enrld = s;
                s.Score = 0;
                db.SaveChanges();

            }
          

        }


        //helper method to grade a submission, changes the score from default 0 to actul score
        private void UpdateScoreActual(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var query = from courses in db.Courses
                        join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                        from join1 in j1.DefaultIfEmpty()
                        where courses.Number == num && courses.Department == subject && join1.Season == season && join1.Year == year

                        join asCatg in db.AssignmentCategories on join1.ClassId equals asCatg.InClass into j2
                        from join2 in j2.DefaultIfEmpty()
                        where join2.Name == category
                        join asgmnt in db.Assignments on join2.CategoryId equals asgmnt.Category into j3
                        from join3 in j3.DefaultIfEmpty()
                        where join3.Name == asgname
                        join enrolled in db.Enrolleds on join1.ClassId equals enrolled.Class into j4
                        from join4 in j4.DefaultIfEmpty()
                        join students in db.Students on join4.Student equals students.UId into j5

                        from join5 in j5.DefaultIfEmpty()
                        join submsns in db.Submissions on new { a = join3.AssignmentId, b = uid } equals new { a = submsns.Assignment, b = submsns.Student } into j6
                        from join6 in j6.DefaultIfEmpty()
                        select join6;

            foreach (var s in query)
            {
                Submission enrld = s;
                
             
                s.Score = (uint)score;
                db.SaveChanges();

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

                UpdateScoreDefault(subject, num, season, year, category, asgname);  //update score to 0



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

            try
            {
                var query = from courses in db.Courses
                            join classes in db.Classes on courses.CatalogId equals classes.Listing into j1
                            from join1 in j1.DefaultIfEmpty()
                            where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year

                            join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j2
                          
                            from join2 in j2.DefaultIfEmpty()
                            where join2.Name == category
                            join assgnmts in db.Assignments on join2.CategoryId equals assgnmts.AssignmentId into j3
                           
                            from join3 in j3.DefaultIfEmpty()
                            where join3.Name == asgname
                            join submission in db.Submissions on join3.AssignmentId equals submission.Assignment into j4
                          
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




        //helper method to calculate student's new grade for the class after an assignment is graded

        private void UpdateGrade(string subject, int num, string season, int year, string uid) {


            var queryGetAssCtg = from courses in db.Courses
                        join classes in db.Classes on courses.CatalogId equals classes.Listing
                        into j1
                        from join1 in j1.DefaultIfEmpty()
                        where courses.Department == subject && courses.Number == num && join1.Season == season && join1.Year == year
                        join assgnmtCat in db.AssignmentCategories on join1.ClassId equals assgnmtCat.InClass into j3
                        from join3 in j3.DefaultIfEmpty()
                        select join3;

            if (queryGetAssCtg.Any()) {
                int maxCtgW = 0;

                float ttlScr = 0;
                int ttlPpnts = 0;
                float finalGrd = 0;
                string lttrGrd = "";
                foreach (var ctg in queryGetAssCtg)
                {
                    var ctgWght = ctg.Weight;

                    var asmnts = ctg.Assignments;
                    if (asmnts.Any())
                    {

                        foreach (var s in asmnts)
                        {


                            var getScrs = from en in db.Enrolleds
                                          join sb in db.Submissions on new
                                          {
                                              a = s.AssignmentId,
                                              b = uid
                                          } equals new
                                          {
                                              a = sb.Assignment,
                                              b = sb.Student
                                          } into j1
                                          from join1 in j1.DefaultIfEmpty()
                                          select join1;

                            ttlScr += getScrs.First().Score;
                            ttlPpnts += (int)s.MaxPoints;
                      
                        }
                        maxCtgW += (int)ctgWght;

                    }

                }

                if (maxCtgW > 0) {
                    float scaleFctr = 100 / (float)maxCtgW;
                    float finalScr = ttlScr /ttlPpnts * 100;

                    finalGrd = finalScr * scaleFctr;


                }
               

                
                if (finalGrd >= 93 && finalGrd <= 100) {
                    lttrGrd+="A";

                }
                else if (finalGrd >= 90) {
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
                else if (finalGrd >=0)
                {
                    lttrGrd += "E";
                }

                var getGrade = from ct in queryGetAssCtg
                               join enrlmnt in db.Enrolleds on new
                               {
                                   a = ct.InClass,
                                   b = uid
                               } equals new
                               {
                                   a = enrlmnt.Class,
                                   b = enrlmnt.Student
                               } into j1
                               from join1 in j1.DefaultIfEmpty()
                               select join1;

                Enrolled e = getGrade.First();
                e.Grade = lttrGrd;

                db.SaveChanges();


            }


//            87 - 89 B +  77 - 79 C +  67 - 69 D +
//93 - 100 A    83 - 86 B   73 - 76 C   63 - 66 D   0 - 59 E
//90 - 92 A -  80 - 82 B -   70 - 72 C -   60 - 62 D -




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
            try {

                UpdateScoreActual(subject, num, season, year, category, asgname, uid, score);
                UpdateGrade(subject, num, season, year, uid);

                return Json(new { success = true });

            }


            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return Json(new { success = false });
            }
           
           

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

