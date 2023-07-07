using MySqlConnector;

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
            //testParser(gameList);

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
       
        private static int GetOrCreatePlayerId(MySqlConnection connection, string playerName, int playerElo)
        {
            // playerId variable to store the player's ID if they exist in the Players table
            int playerId = 0;

            // Check if the player already exists in the Players table
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

            // If the player doesn't exist, insert player into Players table
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
            // If the player does exist, update the player's Elo if the new Elo is higher
            else
            {
                // Update the player's Elo if the new Elo is higher
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
            // eventId variable to store the event's ID if it exists in the Events table
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

        static void testParser(List<ChessGame> gameList)
        {
            foreach (ChessGame game in gameList)
            {
                Console.WriteLine("Event Name: {0}", game.eventName);
                Console.WriteLine("Site: {0}", game.site);
                Console.WriteLine("Round: {0}", game.round);
                Console.WriteLine("Moves: {0}", string.Join(" ", game.moves));
                Console.WriteLine("White Player Name: {0}", game.whitePlayerName);
                Console.WriteLine("Black Player Name: {0}", game.blackPlayerName);
                Console.WriteLine("White Player Elo: {0}", game.whitePlayerElo);
                Console.WriteLine("Black Player Elo: {0}", game.blackPlayerElo);
                Console.WriteLine("Result: {0}", game.result);
                Console.WriteLine("Date: {0}", game.date);
                Console.WriteLine("Event Date: {0}", game.eventDate);
                Console.WriteLine();
                Console.WriteLine();
            }
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

                    // Create the query string
                    string query = "SELECT Games.*, Players.Name AS WhitePlayerName, Players.Elo AS WhitePlayerElo, " + // Select all columns from the Games table
                                    "Players_1.Name AS BlackPlayerName, Players_1.Elo AS BlackPlayerElo, " + 
                                    "Events.Name AS EventName, Events.Site, Events.Date " + 
                                    "FROM Games " + 
                                    "JOIN Players ON Games.WhitePlayer = Players.pID " + // Join the Players table twice to get the white and black player names and Elos
                                    "JOIN Players AS Players_1 ON Games.BlackPlayer = Players_1.pID " + 
                                    "JOIN Events ON Games.eID = Events.eID " + // Join the Events table to get the event name, site, and date
                                    "WHERE "; 

                    // Create the list of parameters to prevent SQL injection attacks
                    List<MySqlParameter> parameters = new List<MySqlParameter>();

                    // Add the parameters to the query string and the list of parameters if they are not null
                    if (white != null)
                    {
                        query += "Players.Name = @WhitePlayer AND "; 
                        parameters.Add(new MySqlParameter("@WhitePlayer", white));
                    }
                    if (black != null)
                    {
                        query += "Players_1.Name = @BlackPlayer AND ";
                        parameters.Add(new MySqlParameter("@BlackPlayer", black));
                    }
                    if (opening != null)
                    {
                        query += "Moves LIKE @Opening AND ";
                        parameters.Add(new MySqlParameter("@Opening", opening + "%"));
                    }
                    if (winner != null)
                    {
                        query += "Result = @Winner AND ";
                        parameters.Add(new MySqlParameter("@Winner", winner));
                    }
                    if (useDate)
                    {
                        query += "Events.Date BETWEEN @StartDate AND @EndDate AND ";
                        parameters.Add(new MySqlParameter("@StartDate", start.Date));
                        parameters.Add(new MySqlParameter("@EndDate", end.Date));
                    }
                    query += "1 = 1"; // This is a hack to make the query work if no filters are selected

                    // Execute the query
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    // Add the parameters to the command
                    cmd.Parameters.AddRange(parameters.ToArray());
                    // Read the results
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    // Parse the results
                    if (showMoves)
                    {
                        while (rdr.Read())
                        {
                            numRows++;
                            parsedResult += "\n" + "Event: " + rdr["EventName"] + 
                                            "\n" + "Site: " + rdr["Site"] + 
                                            "\n" + "Date: " + rdr["Date"] + 
                                            "\n" + "Round: " + rdr["Round"]  + 
                                            "\n" + "White: " + rdr["WhitePlayerName"] + "(" + rdr["WhitePlayerElo"] + ")" +
                                            "\n" + "Black: " + rdr["BlackPlayerName"] +  "(" + rdr["BlackPlayerElo"] + ")" + 
                                            "\n" + "Result: " + rdr["Result"] + 
                                            "\n" + "Moves: " + rdr["Moves"];
                        }
                    }
                    else
                    {
                        while (rdr.Read())
                        {
                            numRows++;
                            parsedResult += "\n" + "Event: " + rdr["EventName"] +
                                            "\n" + "Site: " + rdr["Site"] +
                                            "\n" + "Date: " + rdr["Date"] +
                                            "\n" + "Round: " + rdr["Round"] +
                                            "\n" + "White: " + rdr["WhitePlayerName"] + " (" + rdr["WhitePlayerElo"] + ")" +
                                            "\n" + "Black: " + rdr["BlackPlayerName"] + " (" + rdr["BlackPlayerElo"] + ")" +
                                            "\n" + "Result: " + rdr["Result"] + "\n";
                        }
                    }

                    // Close the connection
                    conn.Close();
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
