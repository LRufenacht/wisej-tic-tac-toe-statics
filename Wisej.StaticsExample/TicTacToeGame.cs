using System;

namespace Wisej.StaticsExample
{
    public static class TicTacToeGame
    {
        public static char[,] Board { get; private set; } = new char[3, 3];
        public static char CurrentPlayer { get; private set; } = 'X';
        public static bool GameOver { get; private set; }
        public static string Message { get; private set; } = "Player X's turn";

        public static event EventHandler<MoveEventArgs> MoveMade;
        public static event EventHandler GameReset;
        public static event EventHandler<string> GameEnded;

        public static void MakeMove(int row, int col)
        {
            if (GameOver || Board[row, col] != '\0')
                return;

            Board[row, col] = CurrentPlayer;

            if (CheckWin(CurrentPlayer))
            {
                GameOver = true;
                Message = $"Player {CurrentPlayer} wins!";
                MoveMade?.Invoke(null, new MoveEventArgs(row, col, CurrentPlayer));
                GameEnded?.Invoke(null, Message);
            }
            else if (CheckDraw())
            {
                GameOver = true;
                Message = "Draw!";
                MoveMade?.Invoke(null, new MoveEventArgs(row, col, CurrentPlayer));
                GameEnded?.Invoke(null, Message);
            }
            else
            {
                MoveMade?.Invoke(null, new MoveEventArgs(row, col, CurrentPlayer));
                CurrentPlayer = CurrentPlayer == 'X' ? 'O' : 'X';
                Message = $"Player {CurrentPlayer}'s turn";
            }
        }

        public static void Reset()
        {
            Board = new char[3, 3];
            CurrentPlayer = 'X';
            GameOver = false;
            Message = "Player X's turn";
            GameReset?.Invoke(null, EventArgs.Empty);
        }

        private static bool CheckWin(char player)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Board[i, 0] == player && Board[i, 1] == player && Board[i, 2] == player)
                    return true;
                if (Board[0, i] == player && Board[1, i] == player && Board[2, i] == player)
                    return true;
            }
            if (Board[0, 0] == player && Board[1, 1] == player && Board[2, 2] == player)
                return true;
            if (Board[0, 2] == player && Board[1, 1] == player && Board[2, 0] == player)
                return true;
            return false;
        }

        private static bool CheckDraw()
        {
            foreach (var cell in Board)
            {
                if (cell == '\0')
                    return false;
            }
            return true;
        }
    }

    public class MoveEventArgs : EventArgs
    {
        public int Row { get; }
        public int Col { get; }
        public char Player { get; }

        public MoveEventArgs(int row, int col, char player)
        {
            Row = row;
            Col = col;
            Player = player;
        }
    }
}
