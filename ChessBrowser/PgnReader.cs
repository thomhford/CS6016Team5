using System;
using System.Collections.Generic;
using System.IO;

namespace ChessBrowser
{
    public class PgnReader
    {
        public PgnReader()
        {
        }

        public static List<ChessGame> ParsePgn(string filePath)
        {
            List<ChessGame> games = new List<ChessGame>();

            string pgnData = File.ReadAllText(filePath);
            string[] gameData = pgnData.Split(new[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string gamePgnData in gameData)
            {
                ChessGame game = ParsePgnHelper(gamePgnData);
                games.Add(game);
            }

            return games;
        }

        private static ChessGame ParsePgnHelper(string pgnData)
        {
            string eventName = string.Empty;
            string site = "?";
            int round = 0;
            string moves = string.Empty;
            string whitePlayerName = string.Empty;
            string blackPlayerName = string.Empty;
            int whitePlayerElo = 0;
            int blackPlayerElo = 0;
            string result = string.Empty;
            DateTime date = DateTime.MinValue;
            DateTime eventDate = DateTime.MinValue;

            string[] lines = pgnData.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            bool movesStarted = false; // Flag to track if the moves section has started
            List<string> moveList = new List<string>(); // List to store the moves

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    // If the line is blank, toggle the movesStarted flag
                    movesStarted = !movesStarted;
                    moveList = new List<string>();
                    continue;
                }
                if (!movesStarted)
                {
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        int colonIndex = trimmedLine.IndexOf(' ');
                        if (colonIndex != -1)
                        {
                            string tag = trimmedLine[1..colonIndex].Trim();
                            string value = trimmedLine.Substring(colonIndex + 1, trimmedLine.Length - colonIndex - 2).Trim();

                            switch (tag)
                            {
                                case "Event":
                                    eventName = value;
                                    break;
                                case "Site":
                                    site = value;
                                    break;
                                case "Date":
                                    if (DateTime.TryParse(value, out DateTime parsedDate))
                                    {
                                        date = parsedDate;
                                    }
                                    break;
                                case "EventDate":
                                    if (DateTime.TryParse(value, out DateTime parsedEventDate))
                                    {
                                        eventDate = parsedEventDate;
                                    }
                                    break;
                                case "Round":
                                    if (int.TryParse(value, out int parsedRound))
                                    {
                                        round = parsedRound;
                                    }
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
                                    if (int.TryParse(value, out int parsedWhiteElo))
                                    {
                                        whitePlayerElo = parsedWhiteElo;
                                    }
                                    break;
                                case "BlackElo":
                                    if (int.TryParse(value, out int parsedBlackElo))
                                    {
                                        blackPlayerElo = parsedBlackElo;
                                    }
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    // Process moves
                    moveList.Add(trimmedLine);
                }
            }
            moves = string.Join(" ", moveList);
            Console.WriteLine(moves);
            return new ChessGame(eventName, site, round, moves, whitePlayerName, blackPlayerName, whitePlayerElo, blackPlayerElo, result, date, eventDate);
        }
    }
}
