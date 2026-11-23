
using System;

namespace Poddavki.Core
{
    public enum PieceType
    {
        Empty,
        WhiteMan,
        BlackMan,
        WhiteKing,
        BlackKing
    }

    public class GameBoard
    {
        public PieceType[,] Cells = new PieceType[8, 8];

        public GameBoard()
        {
            ResetBoard();
        }

        public void ResetBoard()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Cells[i, j] = PieceType.Empty;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i + j) % 2 == 1)
                        Cells[i, j] = PieceType.BlackMan;
                }
            }

            for (int i = 5; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i + j) % 2 == 1)
                        Cells[i, j] = PieceType.WhiteMan;
                }
            }
        }
    }
}
