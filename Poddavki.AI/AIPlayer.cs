
using System;
using System.Collections.Generic;
using Poddavki.Core;

namespace Poddavki.AI
{
    public class AIMove
    {
        public int FromX, FromY, ToX, ToY;
        public List<(int X, int Y)> Path;
        public List<(int X, int Y)> Captured;

        public AIMove(int fx, int fy, int tx, int ty)
        {
            FromX = fx;
            FromY = fy;
            ToX = tx;
            ToY = ty;

            Path = new List<(int, int)>
            {
                (fx, fy),
                (tx, ty)
            };

            Captured = new List<(int, int)>();
        }
    }

    public class AIPlayer
    {
        private readonly int[] dx = { -1, -1, 1, 1 };
        private readonly int[] dy = { -1, 1, -1, 1 };

        public AIMove GetBestMove(GameBoard board, int depth)
        {
            List<AIMove> moves = GetAllMoves(board, false);

            if (moves.Count == 0)
                return null;

            AIMove bestMove = null;
            int bestScore = int.MinValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            foreach (AIMove move in moves)
            {
                GameBoard temp = CloneBoard(board);
                ExecuteMove(temp, move);

                int score = Minimax(
                    temp, depth - 1, alpha, beta, true);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }

                alpha = Math.Max(alpha, bestScore);
            }

            return bestMove;
        }

        private int Minimax(
            GameBoard board,
            int depth,
            int alpha,
            int beta,
            bool isWhiteTurn)
        {
            List<AIMove> moves = GetAllMoves(board, isWhiteTurn);

            if (moves.Count == 0)
                return isWhiteTurn
                    ? -100000 - Math.Max(depth, 0)
                    : 100000 + Math.Max(depth, 0);

            if (depth <= 0)
                return EvaluateBoard(board);

            if (!isWhiteTurn)
            {
                int best = int.MinValue;

                foreach (AIMove move in moves)
                {
                    GameBoard temp = CloneBoard(board);
                    ExecuteMove(temp, move);

                    int score = Minimax(
                        temp, depth - 1,
                        alpha, beta, true);

                    best = Math.Max(best, score);
                    alpha = Math.Max(alpha, best);

                    if (alpha >= beta)
                        break;
                }

                return best;
            }
            else
            {
                int best = int.MaxValue;

                foreach (AIMove move in moves)
                {
                    GameBoard temp = CloneBoard(board);
                    ExecuteMove(temp, move);

                    int score = Minimax(
                        temp, depth - 1,
                        alpha, beta, false);

                    best = Math.Min(best, score);
                    beta = Math.Min(beta, best);

                    if (alpha >= beta)
                        break;
                }

                return best;
            }
        }

        private int EvaluateBoard(GameBoard board)
        {
            int score = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    switch (board.Cells[i, j])
                    {
                        case PieceType.WhiteMan:
                            score += 10;
                            break;

                        case PieceType.WhiteKing:
                            score += 30;
                            break;

                        case PieceType.BlackMan:
                            score -= 10;
                            break;

                        case PieceType.BlackKing:
                            score -= 30;
                            break;
                    }
                }
            }

            return score;
        }

        private bool IsOwnPiece(PieceType piece, bool isWhite)
        {
            if (isWhite)
                return piece == PieceType.WhiteMan ||
                       piece == PieceType.WhiteKing;

            return piece == PieceType.BlackMan ||
                   piece == PieceType.BlackKing;
        }

        private bool IsEnemy(PieceType piece, bool isWhite)
        {
            return piece != PieceType.Empty &&
                   !IsOwnPiece(piece, isWhite);
        }

        private bool Inside(int x, int y)
        {
            return x >= 0 && x < 8 &&
                   y >= 0 && y < 8;
        }

        private PieceType Promote(
            PieceType piece, int row)
        {
            if (piece == PieceType.WhiteMan && row == 0)
                return PieceType.WhiteKing;

            if (piece == PieceType.BlackMan && row == 7)
                return PieceType.BlackKing;

            return piece;
        }


        private List<AIMove> GetAllMoves(
            GameBoard board, bool isWhite)
        {
            List<AIMove> captures = new List<AIMove>();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (!IsOwnPiece(board.Cells[x, y], isWhite))
                        continue;

                    FindCaptures(
                        board,
                        x, y,
                        x, y,
                        isWhite,
                        new List<(int, int)> { (x, y) },
                        new List<(int, int)>(),
                        captures);
                }
            }

            if (captures.Count > 0)
                return captures;

            List<AIMove> moves = new List<AIMove>();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    PieceType piece = board.Cells[x, y];

                    if (!IsOwnPiece(piece, isWhite))
                        continue;

                    bool isKing =
                        piece == PieceType.WhiteKing ||
                        piece == PieceType.BlackKing;

                    for (int d = 0; d < 4; d++)
                    {
                        if (!isKing)
                        {
                            if (piece == PieceType.WhiteMan &&
                                dx[d] != -1)
                                continue;

                            if (piece == PieceType.BlackMan &&
                                dx[d] != 1)
                                continue;
                        }

                        int step = 1;

                        while (true)
                        {
                            int nx = x + dx[d] * step;
                            int ny = y + dy[d] * step;

                            if (!Inside(nx, ny))
                                break;

                            if (board.Cells[nx, ny] != PieceType.Empty)
                                break;

                            moves.Add(new AIMove(x, y, nx, ny));

                            if (!isKing)
                                break;

                            step++;
                        }
                    }
                }
            }

            return moves;
        }



        private void FindCaptures(
            GameBoard board,
            int startX,
            int startY,
            int x,
            int y,
            bool isWhite,
            List<(int X, int Y)> path,
            List<(int X, int Y)> captured,
            List<AIMove> result)
        {
            bool found = false;

            PieceType piece = board.Cells[x, y];

            bool isKing =
                piece == PieceType.WhiteKing ||
                piece == PieceType.BlackKing;

            for (int d = 0; d < 4; d++)
            {
                int step = 1;
                int enemyX = -1;
                int enemyY = -1;

                while (true)
                {
                    int nx = x + dx[d] * step;
                    int ny = y + dy[d] * step;

                    if (!Inside(nx, ny))
                        break;

                    PieceType current = board.Cells[nx, ny];

                    if (IsOwnPiece(current, isWhite))
                        break;

                    if (IsEnemy(current, isWhite))
                    {
                        if (enemyX != -1)
                            break;

                        enemyX = nx;
                        enemyY = ny;
                    }
                    else if (enemyX != -1)
                    {
                        found = true;

                        GameBoard temp = CloneBoard(board);

                        temp.Cells[x, y] = PieceType.Empty;
                        temp.Cells[enemyX, enemyY] = PieceType.Empty;
                        temp.Cells[nx, ny] = Promote(piece, nx);

                        List<(int X, int Y)> newPath =
                            new List<(int X, int Y)>(path);

                        newPath.Add((nx, ny));

                        List<(int X, int Y)> newCaptured =
                            new List<(int X, int Y)>(captured);

                        newCaptured.Add((enemyX, enemyY));

                        FindCaptures(
                            temp,
                            startX,
                            startY,
                            nx,
                            ny,
                            isWhite,
                            newPath,
                            newCaptured,
                            result);
                    }

                    if (!isKing)
                    {
                        if (step >= 2)
                            break;
                    }

                    step++;
                }
            }

            if (!found && captured.Count > 0)
            {
                AIMove move = new AIMove(
                    startX, startY, x, y);

                move.Path = new List<(int, int)>(path);
                move.Captured = new List<(int, int)>(captured);

                result.Add(move);
            }
        }


        private void ExecuteMove(
            GameBoard board, AIMove move)
        {
            PieceType piece =
                board.Cells[move.FromX, move.FromY];

            board.Cells[move.FromX, move.FromY] =
                PieceType.Empty;

            foreach (var captured in move.Captured)
            {
                board.Cells[captured.X, captured.Y] =
                    PieceType.Empty;
            }

            for (int i = 1; i < move.Path.Count; i++)
            {
                piece = Promote(piece, move.Path[i].X);
            }

            board.Cells[move.ToX, move.ToY] = piece;
        }

        private GameBoard CloneBoard(GameBoard original)
        {
            GameBoard clone = new GameBoard();

            Array.Copy(
                original.Cells,
                clone.Cells,
                original.Cells.Length);

            return clone;
        }
    }
}
