
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace Poddavki.Data
{
    public class GameRepository
    {
        private readonly string connectionString;

        public GameRepository(string databasePath = null)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                connectionString = DatabaseInitializer.ConnectionString;
            }
            else
            {
                string fullPath = Path.GetFullPath(databasePath);
                string directory = Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                connectionString =
                    "Data Source=" + fullPath + ";Version=3;";
            }

            EnsureSchema();
        }

        private SQLiteConnection OpenConnection()
        {
            SQLiteConnection connection =
                new SQLiteConnection(connectionString);

            connection.Open();

            using (SQLiteCommand command =
                new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                command.ExecuteNonQuery();
            }

            return connection;
        }

        private void EnsureSchema()
        {
            using (SQLiteConnection connection =
                OpenConnection())
            {
                string sql = @"
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
                    );

                    CREATE TABLE IF NOT EXISTS Moves
                    (
                        MoveId INTEGER PRIMARY KEY AUTOINCREMENT,
                        GameId INTEGER NOT NULL,
                        MoveNumber INTEGER NOT NULL,
                        MoveText TEXT NOT NULL,
                        FOREIGN KEY (GameId)
                            REFERENCES Games(GameId)
                            ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS CompletedGames
                    (
                        CompletedGameId INTEGER PRIMARY KEY AUTOINCREMENT,
                        OwnerLogin TEXT NOT NULL,
                        CompletedAt TEXT NOT NULL,
                        IsTwoPlayerMode INTEGER NOT NULL DEFAULT 0,
                        WhiteWon INTEGER NOT NULL,
                        ResultText TEXT NOT NULL,
                        SecondsElapsed INTEGER NOT NULL DEFAULT 0,
                        MoveCount INTEGER NOT NULL DEFAULT 0
                    );

                    CREATE TABLE IF NOT EXISTS CompletedMoves
                    (
                        CompletedMoveId INTEGER PRIMARY KEY AUTOINCREMENT,
                        CompletedGameId INTEGER NOT NULL,
                        MoveNumber INTEGER NOT NULL,
                        MoveText TEXT NOT NULL,
                        FOREIGN KEY (CompletedGameId)
                            REFERENCES CompletedGames(CompletedGameId)
                            ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS IX_CompletedGames_OwnerLogin
                    ON CompletedGames(OwnerLogin);

                    CREATE INDEX IF NOT EXISTS IX_Games_OwnerLogin
                    ON Games(OwnerLogin);";

                using (SQLiteCommand command =
                    new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<SaveState> GetAllSaves()
        {
            List<SaveState> saves = new List<SaveState>();

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(
                "SELECT * FROM Games ORDER BY GameId", connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    SaveState save = new SaveState
                    {
                        SaveName = reader["SaveName"].ToString(),

                        SaveDate = DateTime.Parse(
                            reader["SaveDate"].ToString(),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.RoundtripKind),

                        BoardData = JsonSerializer.Deserialize<List<int>>(
                            reader["BoardData"].ToString())
                            ?? new List<int>(),

                        LogData = JsonSerializer.Deserialize<List<string>>(
                            reader["LogData"].ToString())
                            ?? new List<string>(),

                        PlayerScore = Convert.ToInt32(reader["PlayerScore"]),
                        AiScore = Convert.ToInt32(reader["AiScore"]),
                        SecondsElapsed = Convert.ToInt32(reader["SecondsElapsed"]),
                        OwnerLogin = reader["OwnerLogin"].ToString(),

                        IsTwoPlayerMode =
                            Convert.ToInt32(reader["IsTwoPlayerMode"]) != 0,

                        MustContinueCapture =
                            Convert.ToInt32(reader["MustContinueCapture"]) != 0,

                        CapturePath = reader["CapturePath"].ToString(),
                        SelectedX = Convert.ToInt32(reader["SelectedX"]),
                        SelectedY = Convert.ToInt32(reader["SelectedY"]),

                        IsWhiteTurn =
                            Convert.ToInt32(reader["IsWhiteTurn"]) != 0,

                        IsAutoSave =
                            Convert.ToInt32(reader["IsAutoSave"]) != 0
                    };

                    saves.Add(save);
                }
            }

            return saves;
        }

        public void SaveAll(List<SaveState> saves)
        {
            if (saves == null)
                throw new ArgumentNullException(nameof(saves));

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(
                        "DELETE FROM Moves; DELETE FROM Games;",
                        connection, transaction))
                    {
                        command.ExecuteNonQuery();
                    }

                    foreach (SaveState save in saves)
                    {
                        InsertSave(connection, transaction, save);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void AddSave(SaveState save)
        {
            if (save == null)
                throw new ArgumentNullException(nameof(save));

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    InsertSave(connection, transaction, save);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void SavePendingGame(SaveState save)
        {
            if (save == null)
                throw new ArgumentNullException(nameof(save));

            save.IsAutoSave = true;

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(
                        @"DELETE FROM Games
                          WHERE IsAutoSave = 1
                          AND OwnerLogin = @owner COLLATE NOCASE
                          AND IsTwoPlayerMode = @mode",
                        connection, transaction))
                    {
                        command.Parameters.AddWithValue(
                            "@owner", save.OwnerLogin ?? "");

                        command.Parameters.AddWithValue(
                            "@mode", save.IsTwoPlayerMode ? 1 : 0);

                        command.ExecuteNonQuery();
                    }

                    InsertSave(connection, transaction, save);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public bool RemovePendingGame(
            string ownerLogin, bool isTwoPlayerMode)
        {
            if (string.IsNullOrWhiteSpace(ownerLogin))
                return false;

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(
                @"DELETE FROM Games
                  WHERE IsAutoSave = 1
                  AND OwnerLogin = @owner COLLATE NOCASE
                  AND IsTwoPlayerMode = @mode",
                connection))
            {
                command.Parameters.AddWithValue("@owner", ownerLogin);
                command.Parameters.AddWithValue(
                    "@mode", isTwoPlayerMode ? 1 : 0);

                return command.ExecuteNonQuery() > 0;
            }
        }

        private void InsertSave(
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            SaveState save)
        {
            if (save == null)
                throw new ArgumentNullException(nameof(save));

            string sql = @"
                INSERT INTO Games
                (
                    SaveName, SaveDate, BoardData, LogData,
                    PlayerScore, AiScore, SecondsElapsed,
                    OwnerLogin, IsTwoPlayerMode,
                    MustContinueCapture, CapturePath,
                    SelectedX, SelectedY, IsWhiteTurn, IsAutoSave
                )
                VALUES
                (
                    @name, @date, @board, @log,
                    @player, @ai, @seconds,
                    @owner, @twoPlayer,
                    @capture, @path,
                    @x, @y, @white, @auto
                );";

            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue(
                    "@name", save.SaveName ?? "Сохранение");

                command.Parameters.AddWithValue(
                    "@date", save.SaveDate.ToString("o"));

                command.Parameters.AddWithValue(
                    "@board", JsonSerializer.Serialize(
                        save.BoardData ?? new List<int>()));

                command.Parameters.AddWithValue(
                    "@log", JsonSerializer.Serialize(
                        save.LogData ?? new List<string>()));

                command.Parameters.AddWithValue("@player", save.PlayerScore);
                command.Parameters.AddWithValue("@ai", save.AiScore);
                command.Parameters.AddWithValue("@seconds", save.SecondsElapsed);
                command.Parameters.AddWithValue("@owner", save.OwnerLogin ?? "");

                command.Parameters.AddWithValue(
                    "@twoPlayer", save.IsTwoPlayerMode ? 1 : 0);

                command.Parameters.AddWithValue(
                    "@capture", save.MustContinueCapture ? 1 : 0);

                command.Parameters.AddWithValue(
                    "@path", save.CapturePath ?? "");

                command.Parameters.AddWithValue("@x", save.SelectedX);
                command.Parameters.AddWithValue("@y", save.SelectedY);

                command.Parameters.AddWithValue(
                    "@white", save.IsWhiteTurn ? 1 : 0);

                command.Parameters.AddWithValue(
                    "@auto", save.IsAutoSave ? 1 : 0);

                command.ExecuteNonQuery();
            }

            long gameId;

            using (SQLiteCommand command = new SQLiteCommand(
                "SELECT last_insert_rowid();",
                connection, transaction))
            {
                gameId = Convert.ToInt64(command.ExecuteScalar());
            }

            List<string> logs = save.LogData ?? new List<string>();

            for (int i = 0; i < logs.Count; i++)
            {
                using (SQLiteCommand command = new SQLiteCommand(
                    @"INSERT INTO Moves (GameId, MoveNumber, MoveText)
                      VALUES (@game, @number, @text)",
                    connection, transaction))
                {
                    command.Parameters.AddWithValue("@game", gameId);
                    command.Parameters.AddWithValue("@number", i + 1);
                    command.Parameters.AddWithValue("@text", logs[i]);

                    command.ExecuteNonQuery();
                }
            }
        }


        public long AddCompletedGame(
            SaveState save, bool whiteWon, string resultText)
        {
            if (save == null)
                throw new ArgumentNullException(nameof(save));

            if (string.IsNullOrWhiteSpace(save.OwnerLogin))
                throw new InvalidOperationException(
                    "Не указан пользователь партии.");

            List<string> moves = new List<string>();

            if (save.LogData != null)
            {
                foreach (string item in save.LogData)
                {
                    if (!string.IsNullOrWhiteSpace(item) &&
                        item != "Игра началась!")
                    {
                        moves.Add(item);
                    }
                }
            }

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction =
                connection.BeginTransaction())
            {
                try
                {
                    string sql = @"
                INSERT INTO CompletedGames
                (
                    OwnerLogin, CompletedAt, IsTwoPlayerMode,
                    WhiteWon, ResultText, SecondsElapsed, MoveCount
                )
                VALUES
                (
                    @owner, @date, @mode,
                    @whiteWon, @result, @seconds, @count
                );";

                    using (SQLiteCommand command =
                        new SQLiteCommand(sql, connection, transaction))
                    {
                        command.Parameters.AddWithValue(
                            "@owner", save.OwnerLogin);

                        command.Parameters.AddWithValue(
                            "@date", DateTime.Now.ToString("o"));

                        command.Parameters.AddWithValue(
                            "@mode", save.IsTwoPlayerMode ? 1 : 0);

                        command.Parameters.AddWithValue(
                            "@whiteWon", whiteWon ? 1 : 0);

                        command.Parameters.AddWithValue(
                            "@result", resultText);

                        command.Parameters.AddWithValue(
                            "@seconds", save.SecondsElapsed);

                        command.Parameters.AddWithValue(
                            "@count", moves.Count);

                        command.ExecuteNonQuery();
                    }

                    long gameId;

                    using (SQLiteCommand command =
                        new SQLiteCommand(
                            "SELECT last_insert_rowid();",
                            connection, transaction))
                    {
                        gameId = Convert.ToInt64(
                            command.ExecuteScalar());
                    }

                    for (int i = 0; i < moves.Count; i++)
                    {
                        using (SQLiteCommand command =
                            new SQLiteCommand(
                                @"INSERT INTO CompletedMoves
                          (CompletedGameId, MoveNumber, MoveText)
                          VALUES (@game, @number, @text)",
                                connection, transaction))
                        {
                            command.Parameters.AddWithValue(
                                "@game", gameId);

                            command.Parameters.AddWithValue(
                                "@number", i + 1);

                            command.Parameters.AddWithValue(
                                "@text", moves[i]);

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();

                    return gameId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public List<CompletedGame> GetCompletedGames(string ownerLogin)
        {
            List<CompletedGame> games =
                new List<CompletedGame>();

            if (string.IsNullOrWhiteSpace(ownerLogin))
                return games;

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(
                @"SELECT * FROM CompletedGames
          WHERE OwnerLogin = @owner COLLATE NOCASE
          ORDER BY CompletedAt DESC",
                connection))
            {
                command.Parameters.AddWithValue("@owner", ownerLogin);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        games.Add(new CompletedGame
                        {
                            Id = Convert.ToInt64(
                                reader["CompletedGameId"]),

                            OwnerLogin = reader["OwnerLogin"].ToString(),

                            CompletedAt = DateTime.Parse(
                                reader["CompletedAt"].ToString(),
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.RoundtripKind),

                            IsTwoPlayerMode =
                                Convert.ToInt32(reader["IsTwoPlayerMode"]) != 0,

                            WhiteWon =
                                Convert.ToInt32(reader["WhiteWon"]) != 0,

                            ResultText = reader["ResultText"].ToString(),

                            SecondsElapsed =
                                Convert.ToInt32(reader["SecondsElapsed"]),

                            MoveCount =
                                Convert.ToInt32(reader["MoveCount"])
                        });
                    }
                }
            }

            return games;
        }

        public List<string> GetCompletedMoves(long gameId)
        {
            List<string> moves = new List<string>();

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(
                @"SELECT MoveText FROM CompletedMoves
          WHERE CompletedGameId = @id
          ORDER BY MoveNumber",
                connection))
            {
                command.Parameters.AddWithValue("@id", gameId);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        moves.Add(reader["MoveText"].ToString());
                }
            }

            return moves;
        }

    }
}
