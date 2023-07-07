using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChessBrowser
{
    public class PgnReader
    {
        public PgnReader()
        {
        }

        public static List<ChessGame> ParsePgn(string filePath)
        {
            // Create a list to store the games to return
            List<ChessGame> games = new List<ChessGame>();

            // Read the file into a string
            string[] pgnData = File.ReadAllLines(filePath);

            List<string> gameData = new List<string>();
            StringBuilder currentGroup = new StringBuilder();

            foreach (string line in pgnData)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (currentGroup.Length > 0)
                    {
                        gameData.Add(currentGroup.ToString());
                        currentGroup.Clear();
                    }
                }
                else
                {
                    currentGroup.AppendLine(line);
                }
            }

            // Add the last group if it's not empty
            if (currentGroup.Length > 0)
            {
                gameData.Add(currentGroup.ToString());
            }

            // Parse each game
            for (int i = 0; i < gameData.Count; i += 2)
            {
                ChessGame game = ParsePgnHelper(gameData[i]);
                game.moves = gameData[i + 1];
                games.Add(game);
            }

            return games;
        }

        private static ChessGame ParsePgnHelper(string pgnData)
        {
            // Create variables to store the game data
            string eventName = string.Empty;
            string site = "?";
            string round = string.Empty;
            string moves = string.Empty;
            string whitePlayerName = string.Empty;
            string blackPlayerName = string.Empty;
            int whitePlayerElo = 0;
            int blackPlayerElo = 0;
            string result = string.Empty;
            DateTime date = DateTime.MinValue;
            DateTime eventDate = DateTime.MinValue;

            // Split the data into lines
            string[] lines = pgnData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    // Find the space between the tag and the value
                    int spaceIndex = line.IndexOf(' ');
                    // If there is a space, parse the tag
                    if (spaceIndex != -1)
                    {
                        // Remove the brackets and space from the tag and value
                        string tag = line[1..spaceIndex].Trim();
                        // Remove the quotes from the value
                        string value = line.Substring(spaceIndex + 2, line.Length - spaceIndex - 4).Trim();

                        // Parse the tag and store the value
                        switch (tag)
                        {
                            case "Event":
                                eventName = value;
                                break;
                            case "Site":
                                site = value;
                                break;
                            case "Date":
                                // Parse the date to a DateTime object
                                if (DateTime.TryParse(value, out DateTime parsedDate))
                                {
                                    date = parsedDate;
                                }
                                break;
                            case "EventDate":
                                // Parse the date to a DateTime object
                                if (DateTime.TryParse(value, out DateTime parsedEventDate))
                                {
                                    eventDate = parsedEventDate;
                                }
                                break;
                            case "Round":
                                round = value;
                                break;
                            case "White":
                                whitePlayerName = value;
                                break;
                            case "Black":
                                blackPlayerName = value;
                                break;
                            case "Result":
                                result = value switch
                                {
                                    "1-0" => "W",
                                    "0-1" => "B",
                                    _ => "D",
                                };
                                break;
                            case "WhiteElo":
                                // Parse the Elo to an int
                                if (int.TryParse(value, out int parsedWhiteElo))
                                {
                                    whitePlayerElo = parsedWhiteElo;
                                }
                                break;
                            case "BlackElo":
                                // Parse the Elo to an int
                                if (int.TryParse(value, out int parsedBlackElo))
                                {
                                    blackPlayerElo = parsedBlackElo;
                                }
                                break;
                        }
                    }
                }
            }
            return new ChessGame(eventName, site, round, moves, whitePlayerName, blackPlayerName, whitePlayerElo,
                     blackPlayerElo, result, date, eventDate);
        }
    }
}
