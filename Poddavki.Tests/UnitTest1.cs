
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poddavki.Core;

namespace Poddavki.Tests
{
    [TestClass]
    public class GameBoardTests
    {
        [TestMethod]
        public void NewBoard_ShouldContainTwelveBlackAndWhitePieces()
        {
            GameBoard board = new GameBoard();

            int whiteCount = 0;
            int blackCount = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board.Cells[i, j] == PieceType.WhiteMan)
                        whiteCount++;

                    if (board.Cells[i, j] == PieceType.BlackMan)
                        blackCount++;
                }
            }

            Assert.AreEqual(12, whiteCount);
            Assert.AreEqual(12, blackCount);
        }

        [TestMethod]
        public void NewBoard_ShouldHaveEmptyMiddleRows()
        {
            GameBoard board = new GameBoard();

            for (int i = 3; i < 5; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Assert.AreEqual(
                        PieceType.Empty,
                        board.Cells[i, j]);
                }
            }
        }

        [TestMethod]
        public void ResetBoard_ShouldRestoreInitialPosition()
        {
            GameBoard board = new GameBoard();

            board.Cells[0, 1] = PieceType.Empty;
            board.ResetBoard();

            Assert.AreEqual(
                PieceType.BlackMan,
                board.Cells[0, 1]);
        }

        [TestMethod]
        public void ResetBoard_ShouldRestoreAllPieces()
        {
            GameBoard board = new GameBoard();

            board.Cells[0, 1] = PieceType.Empty;
            board.Cells[7, 6] = PieceType.BlackKing;

            board.ResetBoard();

            int white = 0;
            int black = 0;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (board.Cells[x, y] == PieceType.WhiteMan)
                        white++;

                    if (board.Cells[x, y] == PieceType.BlackMan)
                        black++;
                }
            }

            Assert.AreEqual(12, white);
            Assert.AreEqual(12, black);
            Assert.AreEqual(PieceType.WhiteMan, board.Cells[7, 6]);
            Assert.AreEqual(PieceType.BlackMan, board.Cells[0, 1]);
        }
    }
}
