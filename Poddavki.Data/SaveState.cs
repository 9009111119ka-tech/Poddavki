using System;
using System.Collections.Generic;

namespace Poddavki.Data
{
    public class SaveState
    {
        public string SaveName { get; set; }
        public DateTime SaveDate { get; set; }
        public List<int> BoardData { get; set; } = new List<int>();
        public List<string> LogData { get; set; } = new List<string>();
        public int PlayerScore { get; set; }
        public int AiScore { get; set; }
        public int SecondsElapsed { get; set; }
        public string OwnerLogin { get; set; } = "";
        public bool IsTwoPlayerMode { get; set; }
        public bool MustContinueCapture { get; set; }
        public string CapturePath { get; set; } = "";
        public int SelectedX { get; set; } = -1;
        public int SelectedY { get; set; } = -1;
        public bool IsWhiteTurn { get; set; } = true;
        public bool IsAutoSave { get; set; }
    }
}
