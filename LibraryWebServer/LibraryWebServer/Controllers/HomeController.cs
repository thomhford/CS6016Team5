using LibraryWebServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "TestProject1" )]
namespace LibraryWebServer.Controllers
{
    public class HomeController : Controller
    {

        // WARNING:
        // This very simple web server is designed to be as tiny and simple as possible
        // This is NOT the way to save user data.
        // This will only allow one user of the web server at a time (aside from major security concerns).
        private static string user = "";
        private static int card = -1;

        private readonly LibraryContext db;
        public HomeController(LibraryContext _db)
        {
            db = _db;
        }

        /// <summary>
        /// Given a Patron name and CardNum, verify that they exist and match in the database.
        /// If the login is successful, sets the global variables "user" and "card"
        /// </summary>
        /// <param name="name">The Patron's name</param>
        /// <param name="cardnum">The Patron's card number</param>
        /// <returns>A JSON object with a single field: "success" with a boolean value:
        /// true if the login is accepted, false otherwise.
        /// </returns>
        [HttpPost]
        public IActionResult CheckLogin( string name, int cardnum )
        {
            bool loginSuccessful = false;

            // Query the database to see if name and cardnum exist and match
            var query =
                from p in db.Patrons
                where p.Name == name && p.CardNum == cardnum
                select p;
            //System.Diagnostics.Debug.WriteLine(query);

            // If they do, set loginSuccessful = true
            if ( query.Count() == 1 ){ // if there is only one match
                loginSuccessful = true;
            }
            if ( !loginSuccessful )
            {
                return Json( new { success = false } );
            }
            else
            {
                user = name;
                card = cardnum;
                return Json( new { success = true } );
            }
        }


        /// <summary>
        /// Logs a user out. This is implemented for you.
        /// </summary>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult LogOut()
        {
            user = "";
            card = -1;
            return Json( new { success = true } );
        }

        /// <summary>
        /// Returns a JSON array representing all known books.
        /// Each book should contain the following fields:
        /// {"isbn" (string), "title" (string), "author" (string), "serial" (uint?), "name" (string)}
        /// Every object in the list should have isbn, title, and author.
        /// Books that are not in the Library's inventory (such as Dune) should have a null serial.
        /// The "name" field is the name of the Patron who currently has the book checked out (if any)
        /// Books that are not checked out should have an empty string "" for name.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult AllTitles()
        {
            // Query the database to get all books with the book information and if checkout the name of the person who checked it out
            var query =
                from title in db.Titles // get all titles
                join books in db.Inventory on title.Isbn equals books.Isbn into titleBooks // join with inventory
                from book in titleBooks.DefaultIfEmpty() // if there is no match, return default value
                join checkedOutBooks in db.CheckedOut on book.Serial equals checkedOutBooks.Serial into booksCheckedOut // join with checked out
                from checkedOutBook in booksCheckedOut.DefaultIfEmpty() // if there is no match, return default value
                join patrons in db.Patrons on checkedOutBook.CardNum equals patrons.CardNum into checkedOutBookPatrons // join with patrons
                from patron in checkedOutBookPatrons.DefaultIfEmpty() // if there is no match, return default value
                select new // create new object with the fields we want
                {
                    isbn = title.Isbn, 
                    title = title.Title,
                    author = title.Author,
                    serial = book != null ? book.Serial : 0,
                    name = patron != null ? patron.Name : ""
                };
            return Json(query.ToArray());

        }

        /// <summary>
        /// Returns a JSON array representing all books checked out by the logged in user 
        /// The logged in user is tracked by the global variable "card".
        /// Every object in the array should contain the following fields:
        /// {"title" (string), "author" (string), "serial" (uint) (note this is not a nullable uint) }
        /// Every object in the list should have a valid (non-null) value for each field.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult ListMyBooks()
        {
            var query =
                from books in db.Inventory // get all books
                join checkedOut in db.CheckedOut on books.Serial equals checkedOut.Serial // join with checked out
                where checkedOut.CardNum == card // where the card number matches the logged in user
                from titles in db.Titles // join with titles
                where books.Isbn == titles.Isbn // where the isbn matches
                select new // create new object with the fields we want
                {
                    title = titles.Title,
                    author = titles.Author,
                    serial = books.Serial
                };
            return Json( query.ToArray() );
        }


        /// <summary>
        /// Updates the database to represent that
        /// the given book is checked out by the logged in user (global variable "card").
        /// In other words, insert a row into the CheckedOut table.
        /// You can assume that the book is not currently checked out by anyone.
        /// </summary>
        /// <param name="serial">The serial number of the book to check out</param>
        /// <returns>success</returns>
        [HttpPost]
        public ActionResult CheckOutBook( int serial )
        {
            // You may have to cast serial to a (uint)
            var newCheckout = new CheckedOut
            {
                Serial = (uint)serial,
                CardNum = (uint)card
            };
            db.CheckedOut.Add(newCheckout);
            db.SaveChanges();
            return Json( new { success = true } );
        }

        /// <summary>
        /// Returns a book currently checked out by the logged in user (global variable "card").
        /// In other words, removes a row from the CheckedOut table.
        /// You can assume the book is checked out by the user.
        /// </summary>
        /// <param name="serial">The serial number of the book to return</param>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult ReturnBook( int serial )
        {
            // You may have to cast serial to a (uint)
            var query =
                from checkedOut in db.CheckedOut
                where checkedOut.Serial == (uint)serial && checkedOut.CardNum == card
                select checkedOut;
            db.CheckedOut.Remove(query.First());
            db.SaveChanges();
            return Json( new { success = true } );
        }


        /*******************************************/
        /****** Do not modify below this line ******/
        /*******************************************/


        public IActionResult Index()
        {
            if ( user == "" && card == -1 )
                return View( "Login" );

            return View();
        }


        /// <summary>
        /// Return the Login page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            user = "";
            card = -1;

            ViewData["Message"] = "Please login.";

            return View();
        }

        /// <summary>
        /// Return the MyBooks page.
        /// </summary>
        /// <returns></returns>
        public IActionResult MyBooks()
        {
            if ( user == "" && card == -1 )
                return View( "Login" );

            return View();
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache( Duration = 0, Location = ResponseCacheLocation.None, NoStore = true )]
        public IActionResult Error()
        {
            return View( new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier } );
        }
    }
}