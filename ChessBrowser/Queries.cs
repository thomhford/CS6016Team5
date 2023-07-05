using Microsoft.Maui.Controls;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
                    string insertEventQuery = "INSERT INTO Events (Name, Site, Date) VALUES (@Name, @Site, @Date)"; // TODO: Need to add eID to correspond with games...
                    // Cannot add or update a child row: a foreign key constraint fails (`Team5ChessProject`.`Games`, CONSTRAINT `Games_ibfk_3` FOREIGN KEY (`eID`) REFERENCES `Events` (`eID`))
                    using (var command = new MySqlCommand(insertEventQuery, conn))
                    {
                        command.Connection = conn; // Assign the connection object
                        command.Parameters.AddWithValue("@Name", chessGame.eventName);
                        command.Parameters.AddWithValue("@Site", chessGame.site);
                        command.Parameters.AddWithValue("@Date", chessGame.eventDate);
                    }

                    // Insert data into the Players table
                    int whitePlayerID = GetOrCreatePlayerId(conn, chessGame.whitePlayerName, chessGame.whitePlayerElo);
                    int blackPlayerID = GetOrCreatePlayerId(conn, chessGame.blackPlayerName, chessGame.blackPlayerElo);

                    // Insert data into the Games table
                    string insertGameQuery = "INSERT INTO Games (Round, Result, Moves, BlackPlayer, WhitePlayer, eID) " + // TODO: Need to relate eID with Event...
                        "VALUES (@Round, @Result, @Moves, @BlackPlayer, @WhitePlayer, LAST_INSERT_ID())";
                    using (var command = new MySqlCommand(insertGameQuery))
                    {
                        command.Connection = conn; // Assign the connection object
                        command.Parameters.AddWithValue("@Round", chessGame.round);
                        command.Parameters.AddWithValue("@Result", chessGame.result);
                        command.Parameters.AddWithValue("@Moves", chessGame.moves);
                        command.Parameters.AddWithValue("@BlackPlayer", blackPlayerID);
                        command.Parameters.AddWithValue("@WhitePlayer", whitePlayerID);
                        command.ExecuteNonQuery();
                    }

                    // Use this inside a loop to tell the GUI that one work step has completed:
                    await mainPage.NotifyWorkItemCompleted();
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
        private static int GetOrCreatePlayerId(MySqlConnection connection, string playerName, int playerElo)
        {
            int playerId = 0;

            // Check if the player already exists in the Players table
            string selectPlayerQuery = "SELECT pID, Elo FROM Players WHERE Name = @Name";
            using (var command = new MySqlCommand(selectPlayerQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", playerName);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    playerId = reader.GetInt32(0);
                    int existingElo = reader.GetInt32(1);

                    // Update Elo if the new Elo is higher
                    if (playerElo > existingElo)
                    {
                        string updatePlayerQuery = "UPDATE Players SET Elo = @Elo WHERE pID = @PlayerId";
                        using (var updateCommand = new MySqlCommand(updatePlayerQuery, connection))
                        {
                            updateCommand.Connection = connection;
                            updateCommand.Parameters.AddWithValue("@Elo", playerElo);
                            updateCommand.Parameters.AddWithValue("@PlayerId", playerId);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Keep the original Elo
                        playerElo = existingElo;
                    }
                }
            }

            // If the player doesn't exist, insert them into the Players table
            if (playerId == 0)
            {
                string insertPlayerQuery = "INSERT INTO Players (Name, Elo) VALUES (@Name, @Elo)";
                using var command = new MySqlCommand(insertPlayerQuery, connection);
                command.Connection = connection;
                command.Parameters.AddWithValue("@Name", playerName);
                command.Parameters.AddWithValue("@Elo", playerElo);
                command.ExecuteNonQuery();

                playerId = (int)command.LastInsertedId;
            }

            return playerId;
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
