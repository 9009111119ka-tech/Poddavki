
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poddavki.Data;

namespace Poddavki.Tests
{
    [TestClass]
    public class CompletedGameHistoryTests
    {
        private string testFile;
        private GameRepository repository;

        [TestInitialize]
        public void Setup()
        {
            testFile = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString() + ".db");

            repository = new GameRepository(testFile);
        }

        [TestCleanup]
        public void Cleanup()
        {
            string[] files =
            {
                testFile,
                testFile + "-journal",
                testFile + "-wal",
                testFile + "-shm"
            };

            foreach (string file in files)
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
        }

        [TestMethod]
        public void AddCompletedGame_ShouldSaveGameAndMoves()
        {
            SaveState save = new SaveState
            {
                OwnerLogin = "player1",
                IsTwoPlayerMode = true,
                SecondsElapsed = 125,
                LogData = new List<string>
                {
                    "Игра началась!",
                    "Белые: C3-D4",
                    "Чёрные: F6-E5",
                    "Белые: D4-F6"
                }
            };

            long id = repository.AddCompletedGame(
                save, true, "Победа белых");

            List<CompletedGame> games =
                repository.GetCompletedGames("player1");

            Assert.AreEqual(1, games.Count);
            Assert.AreEqual("Победа белых", games[0].ResultText);
            Assert.IsTrue(games[0].IsTwoPlayerMode);
            Assert.AreEqual(3, games[0].MoveCount);

            List<string> moves =
                repository.GetCompletedMoves(id);

            Assert.AreEqual(3, moves.Count);
            Assert.AreEqual("Белые: C3-D4", moves[0]);
            Assert.AreEqual("Белые: D4-F6", moves[2]);
        }

        [TestMethod]
        public void GetCompletedGames_ShouldReturnOnlyRequestedUser()
        {
            repository.AddCompletedGame(
                new SaveState
                {
                    OwnerLogin = "player1"
                },
                true,
                "Победа");

            repository.AddCompletedGame(
                new SaveState
                {
                    OwnerLogin = "player2"
                },
                false,
                "Поражение");

            List<CompletedGame> games =
                repository.GetCompletedGames("PLAYER1");

            Assert.AreEqual(1, games.Count);
            Assert.AreEqual("player1", games[0].OwnerLogin);
        }

        [TestMethod]
        public void AddCompletedGame_WithoutOwner_ShouldThrowException()
        {
            SaveState save = new SaveState
            {
                OwnerLogin = ""
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => repository.AddCompletedGame(
                    save, true, "Победа"));
        }

        [TestMethod]
        public void GetCompletedMoves_UnknownGame_ShouldReturnEmptyList()
        {
            List<string> moves =
                repository.GetCompletedMoves(-1);

            Assert.AreEqual(0, moves.Count);
        }

        [TestMethod]
        public void AddCompletedGame_Null_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => repository.AddCompletedGame(
                    null, true, "Победа"));
        }
    }

}
