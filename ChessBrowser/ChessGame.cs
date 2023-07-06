using System;

namespace ChessBrowser
{
    // Represents one instance of a Chess game, including Event, Site, Round,
    // Moves, Both player's Names, Both player's ELO rating, Result,
    // and Event date.
    public class ChessGame
    {
        // Member variables
        public string eventName;
        public string site;
        public string round;
        public string moves;
        public string whitePlayerName;
        public string blackPlayerName;
        public int whitePlayerElo;
        public int blackPlayerElo;
        public string result;
        public DateTime date;
        public DateTime eventDate;

        public ChessGame(string eventName, string site, string round,
            string moves, string whitePlayerName, string blackPlayerName,
            int whitePlayerElo, int blackPlayerElo, string result,
            DateTime date, DateTime eventDate)
        {
            // Set member variables from constructor parameters
            this.eventName = eventName;
            this.site = site;
            this.round = round;
            this.moves = moves;
            this.whitePlayerName = whitePlayerName;
            this.blackPlayerName = blackPlayerName;
            this.whitePlayerElo = whitePlayerElo;
            this.blackPlayerElo = blackPlayerElo;
            this.result = result;
            this.date = date;
            this.eventDate = eventDate;
        }
    }
}
