using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ChessBrowser
{
    internal class Queries
    {

        /// <summary>
        /// This function runs when the upload button is pressed.
        /// Given a filename, parses the PGN file, and uploads
        /// each chess game to the user's database.
        /// </summary>
        /// <param name="PGNfilename">The path to the PGN file</param>
        internal static async Task InsertGameData(string PGNfilename, MainPage mainPage)
        {
            // This will build a connection string to your user's database on atr,
            // assuimg you've typed a user and password in the GUI
            string connection = mainPage.GetConnectionString();
            // Load and parse the PGN file
            List<ChessGame> gameList = PgnReader.ParsePgn(PGNfilename);

            // Use this to tell the GUI's progress bar how many total work steps there are
            // For example, one iteration of your main upload loop could be one work step
            mainPage.SetNumWorkItems(gameList.Count);


            using MySqlConnection conn = new MySqlConnection(connection);
            try
            {
                // Open a connection
                conn.Open();

                // iterate through data and generate appropriate insert commands
                foreach (ChessGame chessGame in gameList)
                {
                    // Insert data into the Events table
                    int eID = GetOrCreateEventId(conn, chessGame.eventName, chessGame.site, chessGame.eventDate);

                    // Insert data into the Players table
                    int whitePlayerID = GetOrCreatePlayerId(conn, chessGame.whitePlayerName, chessGame.whitePlayerElo);
                    int blackPlayerID = GetOrCreatePlayerId(conn, chessGame.blackPlayerName, chessGame.blackPlayerElo);

                    // Insert data into the Games table
                    string insertGameQuery = "INSERT IGNORE INTO Games (Round, Result, Moves, BlackPlayer, WhitePlayer, eID) " +
                        "VALUES (@Round, @Result, @Moves, @BlackPlayer, @WhitePlayer, @eID)";
                    using (var command = new MySqlCommand(insertGameQuery))
                    {
                        command.Connection = conn; // Assign the connection object
                        command.Parameters.AddWithValue("@Round", chessGame.round);
                        command.Parameters.AddWithValue("@Result", chessGame.result);
                        command.Parameters.AddWithValue("@Moves", chessGame.moves);
                        command.Parameters.AddWithValue("@BlackPlayer", blackPlayerID);
                        command.Parameters.AddWithValue("@WhitePlayer", whitePlayerID);
                        command.Parameters.AddWithValue("@eID", eID);
                        command.ExecuteNonQuery();
                    }

                    // Use this inside a loop to tell the GUI that one work step has completed:
                    await mainPage.NotifyWorkItemCompleted();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private static int GetOrCreatePlayerId(MySqlConnection connection, string playerName, int playerElo)
        {
            string insertPlayerQuery = "INSERT INTO Players (Name, Elo, pID) VALUES (@Name, @Elo, LAST_INSERT_ID()) " +
                "ON DUPLICATE KEY UPDATE Elo = GREATEST(Elo, @Elo)";
            using var command = new MySqlCommand(insertPlayerQuery, connection);
            command.Connection = connection;
            command.Parameters.AddWithValue("@Name", playerName);
            command.Parameters.AddWithValue("@Elo", playerElo);
            command.ExecuteNonQuery();
            int playerId = (int)command.LastInsertedId;

            return playerId;
        }

        private static int GetOrCreateEventId(MySqlConnection connection, string eventName, string site, DateTime eventDate)
        {
            int eventId = 0;

            // Check if the Event already exists in the Event table
            string selectPlayerQuery = "SELECT eID FROM Events WHERE Name = @Name AND SITE = @Site and Date = @Date";
            using (var command = new MySqlCommand(selectPlayerQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", eventName);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    eventId = reader.GetInt32(0);
                }
            }

            // If the event doesn't exist, insert event into Events table
            if (eventId == 0)
            {
                string insertEventQuery = "INSERT IGNORE INTO Events (Name, Site, Date, eID) VALUES (@Name, @Site, @Date, LAST_INSERT_ID())";
                using var command = new MySqlCommand(insertEventQuery, connection);
                command.Connection = connection;
                command.Parameters.AddWithValue("@Name", eventName);
                command.Parameters.AddWithValue("@Site", site);
                command.Parameters.AddWithValue("@Date", eventDate);
                command.ExecuteNonQuery();
                eventId = (int)command.LastInsertedId;
            }

            return eventId;
        }


        /// <summary>
        /// Queries the database for games that match all the given filters.
        /// The filters are taken from the various controls in the GUI.
        /// </summary>
        /// <param name="white">The white player, or null if none</param>
        /// <param name="black">The black player, or null if none</param>
        /// <param name="opening">The first move, e.g. "1.e4", or null if none</param>
        /// <param name="winner">The winner as "W", "B", "D", or null if none</param>
        /// <param name="useDate">True if the filter includes a date range, False otherwise</param>
        /// <param name="start">The start of the date range</param>
        /// <param name="end">The end of the date range</param>
        /// <param name="showMoves">True if the returned data should include the PGN moves</param>
        /// <returns>A string separated by newlines containing the filtered games</returns>
        internal static string PerformQuery(string white, string black, string opening,
          string winner, bool useDate, DateTime start, DateTime end, bool showMoves,
          MainPage mainPage)
        {
            //comment
            // This will build a connection string to your user's database on atr,
            // assuimg you've typed a user and password in the GUI
            string connection = mainPage.GetConnectionString();

            // Build up this string containing the results from your query
            string parsedResult = "";

            // Use this to count the number of rows returned by your query
            // (see below return statement)
            int numRows = 0;

            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    // Open a connection
                    conn.Open();
                    MySqlCommand cmd1 = conn.CreateCommand();


                    string templateQuery1 = @"select* from(select Name as pName,eID,Result from(select eID, BlackPlayer as pID, Result from Games where(
                                        WhitePlayer in(select pID from Players where Name = @name and Result = @ResultFilter) )limit 5)
                                        as EventOpon natural join Players) as Temp natural join Events";

                    /* string templateQuery = @"select* from(select Name as pName,eID,Result from(select eID, BlackPlayer as pID, Result from Games where
                                                 WhitePlayer in(select pID from Players where p.name = @name) @ResultFilter)
                                                 as EventOpon natural join Players) as Temp natural join Events + @DateFilter";
                    */

                    cmd1.Parameters.AddWithValue("@name", white);
                    cmd1.Parameters.AddWithValue("@ResultFilter", winner);
                    // cmd1.Parameters.AddWithValue("@DateFilter", "where date >" + date);

                    cmd1.CommandText = templateQuery1;
                    using (MySqlDataReader myReader = cmd1.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            //Console.WriteLine(myReader["SSN"] + "_____" + myReader["Name"]);

                            parsedResult += "EventName:" + myReader["Name"] + "\n" + "OPONETName: " + myReader["pName"] + "\n" + "Results: " + myReader["Result"] + "\n";
                           // Console.WriteLine(myReader["Name"] + "_____" + myReader["pName"] + "---" + myReader["eID"] + "----" + myReader["Result"]);
                        }
                       // Console.WriteLine("Done");
                       // Console.ReadLine();
                    }
                





                    // TODO:
                    //       Generate and execute an SQL command,
                    //       then parse the results into an appropriate string and return it.
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

            return numRows + " results\n" + parsedResult;
        }
    }
}
