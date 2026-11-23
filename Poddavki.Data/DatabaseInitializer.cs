
using System;
using System.IO;
using System.Data.SQLite;

namespace Poddavki.Data
{
    public static class DatabaseInitializer
    {
        public static string DatabasePath
        {
            get
            {
                return Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "poddavki.db");
            }
        }

        public static string ConnectionString
        {
            get
            {
                return "Data Source=" + DatabasePath + ";Version=3;";
            }
        }

        public static void Initialize()
        {
            using (SQLiteConnection connection =
                new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command =
                    new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
                {
                    command.ExecuteNonQuery();
                }

                string usersTable = @"
                    CREATE TABLE IF NOT EXISTS Users
                    (
                        Login TEXT PRIMARY KEY COLLATE NOCASE,
                        PasswordHash TEXT NOT NULL,
                        Salt TEXT NOT NULL,
                        Wins INTEGER NOT NULL DEFAULT 0,
                        Losses INTEGER NOT NULL DEFAULT 0,
                        Draws INTEGER NOT NULL DEFAULT 0
                    );";

                string gamesTable = @"
                    CREATE TABLE IF NOT EXISTS Games
                    (
                        GameId INTEGER PRIMARY KEY AUTOINCREMENT,
                        SaveName TEXT NOT NULL,
                        SaveDate TEXT NOT NULL,
                        BoardData TEXT NOT NULL,
                        LogData TEXT NOT NULL,
                        PlayerScore INTEGER NOT NULL DEFAULT 0,
                        AiScore INTEGER NOT NULL DEFAULT 0,
                        SecondsElapsed INTEGER NOT NULL DEFAULT 0,
                        OwnerLogin TEXT NOT NULL DEFAULT '',
                        IsTwoPlayerMode INTEGER NOT NULL DEFAULT 0,
                        MustContinueCapture INTEGER NOT NULL DEFAULT 0,
                        CapturePath TEXT NOT NULL DEFAULT '',
                        SelectedX INTEGER NOT NULL DEFAULT -1,
                        SelectedY INTEGER NOT NULL DEFAULT -1,
                        IsWhiteTurn INTEGER NOT NULL DEFAULT 1,
                        IsAutoSave INTEGER NOT NULL DEFAULT 0
                    );";

                string movesTable = @"
                    CREATE TABLE IF NOT EXISTS Moves
                    (
                        MoveId INTEGER PRIMARY KEY AUTOINCREMENT,
                        GameId INTEGER NOT NULL,
                        MoveNumber INTEGER NOT NULL,
                        MoveText TEXT NOT NULL,
                        FOREIGN KEY (GameId)
                            REFERENCES Games(GameId)
                            ON DELETE CASCADE
                    );";

                string index = @"
                    CREATE INDEX IF NOT EXISTS
                    IX_Games_OwnerLogin
                    ON Games(OwnerLogin);";

                using (SQLiteCommand command =
                    new SQLiteCommand(usersTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand command =
                    new SQLiteCommand(gamesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand command =
                    new SQLiteCommand(movesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand command =
                    new SQLiteCommand(index, connection))
                {
                    command.ExecuteNonQuery();
                }

                string migrationTable = @"
                    CREATE TABLE IF NOT EXISTS MigrationHistory
                    (
                        MigrationId TEXT PRIMARY KEY,
                        AppliedAt TEXT NOT NULL
                    );";

                using (SQLiteCommand command =
                    new SQLiteCommand(migrationTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
