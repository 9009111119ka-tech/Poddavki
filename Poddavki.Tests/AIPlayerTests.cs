using Microsoft.VisualStudio.TestTools.UnitTesting;
using Poddavki.Core;
using Poddavki.AI;

namespace Poddavki.Tests
{
    [TestClass]
    public class AIPlayerTests
    {
        [TestMethod]
        public void GetBestMove_OnInitialBoard_ReturnsLegalBlackMove()
        {
            GameBoard board = new GameBoard();
            AIPlayer ai = new AIPlayer();

            AIMove move = ai.GetBestMove(board, 2);

            Assert.IsNotNull(move);

            PieceType piece = board.Cells[move.FromX, move.FromY];

            Assert.IsTrue(
                piece == PieceType.BlackMan ||
                piece == PieceType.BlackKing);

            Assert.AreEqual(
                PieceType.Empty,
                board.Cells[move.ToX, move.ToY]);
        }


        [TestMethod]
        public void GetBestMove_WhenCaptureAvailable_ShouldChooseCapture()
        {
            GameBoard board = new GameBoard();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    board.Cells[x, y] = PieceType.Empty;
                }
            }

            board.Cells[2, 1] = PieceType.BlackMan;
            board.Cells[3, 2] = PieceType.WhiteMan;

            AIPlayer ai = new AIPlayer();

            AIMove move = ai.GetBestMove(board, 2);

            Assert.IsNotNull(move);
            Assert.AreEqual(2, move.FromX);
            Assert.AreEqual(1, move.FromY);
            Assert.AreEqual(4, move.ToX);
            Assert.AreEqual(3, move.ToY);
            Assert.AreEqual(1, move.Captured.Count);
        }
        [TestMethod]
        public void GetBestMove_WhenNoLegalMoves_ReturnsNull()
        {
            GameBoard board = new GameBoard();

            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    board.Cells[x, y] = PieceType.Empty;

            board.Cells[7, 0] = PieceType.BlackMan;

            AIPlayer ai = new AIPlayer();

            AIMove move = ai.GetBestMove(board, 2);

            Assert.IsNull(move);
        }

        [TestMethod]
        public void GetBestMove_WhenTwoCapturesAvailable_ShouldCaptureBoth()
        {
            GameBoard board = new GameBoard();

            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    board.Cells[x, y] = PieceType.Empty;

            board.Cells[2, 1] = PieceType.BlackMan;
            board.Cells[3, 2] = PieceType.WhiteMan;
            board.Cells[5, 4] = PieceType.WhiteMan;

            AIPlayer ai = new AIPlayer();

            AIMove move = ai.GetBestMove(board, 2);

            Assert.IsNotNull(move);
            Assert.AreEqual(2, move.Captured.Count);
            Assert.AreEqual(3, move.Path.Count);
            Assert.AreEqual(6, move.ToX);
            Assert.AreEqual(5, move.ToY);
        }
    }
}
