
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poddavki.Data;

namespace Poddavki.Tests
{
    [TestClass]
    public class GameRepositoryTests
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
        public void SavePendingGame_ShouldReplacePreviousAutoSave()
        {
            SaveState oldSave = new SaveState
            {
                SaveName = "Старое сохранение",
                OwnerLogin = "player1",
                IsAutoSave = true
            };

            repository.AddSave(oldSave);

            SaveState newSave = new SaveState
            {
                SaveName = "Новое сохранение",
                OwnerLogin = "player1",
                IsAutoSave = true
            };

            repository.SavePendingGame(newSave);

            List<SaveState> loaded = repository.GetAllSaves();

            Assert.AreEqual(1, loaded.Count);
            Assert.AreEqual("Новое сохранение", loaded[0].SaveName);
        }

        [TestMethod]
        public void RemovePendingGame_ShouldDeleteAutoSave()
        {
            SaveState save = new SaveState
            {
                SaveName = "Незавершённая партия",
                OwnerLogin = "player1",
                IsAutoSave = true,
                IsTwoPlayerMode = false
            };

            repository.AddSave(save);

            bool removed = repository.RemovePendingGame(
                "player1", false);

            Assert.IsTrue(removed);
            Assert.AreEqual(0, repository.GetAllSaves().Count);
        }

        [TestMethod]
        public void SaveAll_ShouldReplaceExistingSaves()
        {
            repository.AddSave(new SaveState
            {
                SaveName = "Старое сохранение",
                OwnerLogin = "player1"
            });

            repository.SaveAll(new List<SaveState>
    {
        new SaveState
        {
            SaveName = "Первое",
            OwnerLogin = "player1"
        },
        new SaveState
        {
            SaveName = "Второе",
            OwnerLogin = "player1"
        }
    });

            List<SaveState> saves = repository.GetAllSaves();

            Assert.AreEqual(2, saves.Count);
            Assert.AreEqual("Первое", saves[0].SaveName);
            Assert.AreEqual("Второе", saves[1].SaveName);
        }

        [TestMethod]
        public void RemovePendingGame_ShouldKeepManualSaves()
        {
            repository.AddSave(new SaveState
            {
                SaveName = "Ручное сохранение",
                OwnerLogin = "player1",
                IsAutoSave = false
            });

            repository.SavePendingGame(new SaveState
            {
                SaveName = "Автосохранение",
                OwnerLogin = "player1",
                IsTwoPlayerMode = false
            });

            repository.SavePendingGame(new SaveState
            {
                SaveName = "Игра вдвоём",
                OwnerLogin = "player1",
                IsTwoPlayerMode = true
            });

            repository.RemovePendingGame("player1", false);

            List<SaveState> saves = repository.GetAllSaves();

            Assert.AreEqual(2, saves.Count);
            Assert.IsTrue(saves.Exists(s =>
                s.SaveName == "Ручное сохранение"));
            Assert.IsTrue(saves.Exists(s =>
                s.SaveName == "Игра вдвоём"));
        }

        [TestMethod]
        public void AddSave_Null_ShouldThrowException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => repository.AddSave(null));
        }
    }

}
