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
                    using (var command = new MySqlCommand(insertGameQuery, conn))
                    {
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
        //private static int GetOrCreatePlayerId(MySqlConnection connection, string playerName, int playerElo)
        //{
        //    string insertPlayerQuery = "INSERT INTO Players (Name, Elo, pID) VALUES (@Name, @Elo, LAST_INSERT_ID()) " +
        //        "ON DUPLICATE KEY UPDATE Elo = GREATEST(Elo, @Elo)";
        //    using var command = new MySqlCommand(insertPlayerQuery, connection);
        //    command.Connection = connection;
        //    command.Parameters.AddWithValue("@Name", playerName);
        //    command.Parameters.AddWithValue("@Elo", playerElo);
        //    command.ExecuteNonQuery();
        //    int playerId = (int)command.LastInsertedId;

        //    return playerId;
        //}
        private static int GetOrCreatePlayerId(MySqlConnection connection, string playerName, int playerElo)
        {
            int playerId = 0;

            string selectPlayerQuery = "SELECT pID FROM Players WHERE Name = @Name";
            using (var command = new MySqlCommand(selectPlayerQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", playerName);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        playerId = reader.GetInt32(0);
                    }
                }
            }

            if (playerId == 0)
            {
                string insertPlayerQuery = "INSERT INTO Players (Name, Elo) VALUES (@Name, @Elo)";
                using (var command = new MySqlCommand(insertPlayerQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", playerName);
                    command.Parameters.AddWithValue("@Elo", playerElo);
                    command.ExecuteNonQuery();
                    playerId = (int)command.LastInsertedId;
                }
            }
            else
            {
                string updatePlayerQuery = "UPDATE Players SET Elo = GREATEST(Elo, @Elo) WHERE pID = @PlayerId";
                using (var command = new MySqlCommand(updatePlayerQuery, connection))
                {
                    command.Parameters.AddWithValue("@Elo", playerElo);
                    command.Parameters.AddWithValue("@PlayerId", playerId);
                    command.ExecuteNonQuery();
                }
            }

            return playerId;
        }

        private static int GetOrCreateEventId(MySqlConnection connection, string eventName, string site, DateTime eventDate)
        {
            int eventId = 0;

            // Check if the Event already exists in the Event table
            string selectEventQuery = "SELECT eID FROM Events WHERE Name = @Name AND Site = @Site AND Date = @Date";
            using (var command = new MySqlCommand(selectEventQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", eventName);
                command.Parameters.AddWithValue("@Site", site);
                command.Parameters.AddWithValue("@Date", eventDate);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read()) // Check if eID is present. 
                    {
                        eventId = reader.GetInt32(0);
                    }
                }
            }

            // If the event doesn't exist, insert event into Events table
            if (eventId == 0)
            {
                string insertEventQuery = "INSERT IGNORE INTO Events (Name, Site, Date) VALUES (@Name, @Site, @Date)";
                using (var command = new MySqlCommand(insertEventQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", eventName);
                    command.Parameters.AddWithValue("@Site", site);
                    command.Parameters.AddWithValue("@Date", eventDate);
                    command.ExecuteNonQuery();
                    eventId = (int)command.LastInsertedId;
                }
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
