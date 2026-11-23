
using System;

namespace Poddavki.Data
{
    public class CompletedGame
    {
        public long Id { get; set; }

        public string OwnerLogin { get; set; }

        public DateTime CompletedAt { get; set; }

        public bool IsTwoPlayerMode { get; set; }

        public bool WhiteWon { get; set; }

        public string ResultText { get; set; }

        public int SecondsElapsed { get; set; }

        public int MoveCount { get; set; }

        public string ModeName
        {
            get { return IsTwoPlayerMode ? "Вдвоём" : "С ИИ"; }
        }

        public string DurationText
        {
            get
            {
                return TimeSpan.FromSeconds(
                    Math.Max(0, SecondsElapsed)
                ).ToString(@"hh\:mm\:ss");
            }
        }
    }
}
