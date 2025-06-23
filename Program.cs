using System;
using System.Collections.Generic;

namespace ConnectFour
{
    class Program
    {
        static void Main(string[] args)
        {
            GameController game = new GameController();
            game.StartGame();
        }
    }

    public class GameController
    {
        private Board board;
        private Player player1;
        private Player player2;

        public void StartGame()
        {
            board = new Board();

            Console.WriteLine("Choose game mode:");
            Console.WriteLine("1. One-player (You vs Computer)");
            Console.WriteLine("2. Two-player (Player X vs Player O)");
            string mode = Console.ReadLine();

            if (mode == "1")
            {
                Random rand = new Random();
                bool userGoesFirst = rand.Next(2) == 0;
                if (userGoesFirst)
                {
                    player1 = new HumanPlayer('X');
                    player2 = new AIPlayer('O');
                }
                else
                {
                    player1 = new AIPlayer('X');
                    player2 = new HumanPlayer('O');
                }
            }
            else
            {
                player1 = new HumanPlayer('X');
                player2 = new HumanPlayer('O');
            }

            Player currentPlayer = player1;

            while (true)
            {
                board.Display();
                Console.WriteLine($"Player {currentPlayer.Symbol}'s turn.");
                int column = currentPlayer.ChooseColumn(board);

                if (!board.DropDisc(column, currentPlayer.Symbol))
                {
                    Console.WriteLine("Column is full or invalid. Try again.");
                    continue;
                }

                if (board.CheckWin(currentPlayer.Symbol))
                {
                    board.Display();
                    Console.WriteLine($"Player {currentPlayer.Symbol} wins!");
                    break;
                }

                if (board.IsFull())
                {
                    board.Display();
                    Console.WriteLine("The game is a tie!");
                    break;
                }

                currentPlayer = currentPlayer == player1 ? player2 : player1;
            }

            Console.WriteLine("Play again? (y/n): ");
            if (Console.ReadLine().ToLower() == "y")
                StartGame();
        }
    }

    public class Board
    {
        private char[,] grid;
        private const int Rows = 6;
        private const int Columns = 7;

        public Board()
        {
            grid = new char[Rows, Columns];
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Columns; c++)
                    grid[r, c] = '.';
        }

        public void Display()
        {
            Console.Clear();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                    Console.Write(grid[r, c] + " ");
                Console.WriteLine();
            }
            Console.WriteLine("1 2 3 4 5 6 7");
        }

        public bool DropDisc(int column, char symbol)
        {
            column -= 1;
            if (column < 0 || column >= Columns) return false;

            for (int r = Rows - 1; r >= 0; r--)
            {
                if (grid[r, column] == '.')
                {
                    grid[r, column] = symbol;
                    return true;
                }
            }
            return false;
        }

        public bool IsFull()
        {
            for (int c = 0; c < Columns; c++)
                if (grid[0, c] == '.')
                    return false;
            return true;
        }

        public bool CheckWin(char symbol)
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Columns - 3; c++)
                    if (Match(r, c, 0, 1, symbol)) return true;

            for (int r = 0; r < Rows - 3; r++)
                for (int c = 0; c < Columns; c++)
                    if (Match(r, c, 1, 0, symbol)) return true;

            for (int r = 3; r < Rows; r++)
                for (int c = 0; c < Columns - 3; c++)
                    if (Match(r, c, -1, 1, symbol)) return true;

            for (int r = 0; r < Rows - 3; r++)
                for (int c = 0; c < Columns - 3; c++)
                    if (Match(r, c, 1, 1, symbol)) return true;

            return false;
        }

        private bool Match(int row, int col, int dRow, int dCol, char symbol)
        {
            for (int i = 0; i < 4; i++)
            {
                if (grid[row + i * dRow, col + i * dCol] != symbol)
                    return false;
            }
            return true;
        }

        public bool IsValidMove(int column)
        {
            column -= 1;
            return column >= 0 && column < Columns && grid[0, column] == '.';
        }

        public char[,] GetCopyOfGrid()
        {
            return (char[,])grid.Clone();
        }
    }

    public abstract class Player
    {
        public char Symbol { get; private set; }
        public Player(char symbol)
        {
            Symbol = symbol;
        }
        public abstract int ChooseColumn(Board board);
    }

    public class HumanPlayer : Player
    {
        public HumanPlayer(char symbol) : base(symbol) { }

        public override int ChooseColumn(Board board)
        {
            int column;
            Console.Write("Enter column (1-7): ");
            while (!int.TryParse(Console.ReadLine(), out column) || !board.IsValidMove(column))
            {
                Console.Write("Invalid input. Enter column (1-7): ");
            }
            return column;
        }
    }

    public class AIPlayer : Player
    {
        private Random rand = new Random();
        public AIPlayer(char symbol) : base(symbol) { }

        public override int ChooseColumn(Board board)
        {
            Console.WriteLine("AI is choosing a move...");
            System.Threading.Thread.Sleep(1000);

            List<int> validColumns = new List<int>();
            for (int col = 1; col <= 7; col++)
            {
                if (board.IsValidMove(col))
                    validColumns.Add(col);
            }

            // Try to win
            foreach (int col in validColumns)
            {
                var tempBoard = new BoardSimulation(board);
                tempBoard.DropDisc(col, Symbol);
                if (tempBoard.CheckWin(Symbol))
                    return col;
            }

            // Try to block
            char opponent = Symbol == 'X' ? 'O' : 'X';
            foreach (int col in validColumns)
            {
                var tempBoard = new BoardSimulation(board);
                tempBoard.DropDisc(col, opponent);
                if (tempBoard.CheckWin(opponent))
                    return col;
            }

            // Else random
            return validColumns[rand.Next(validColumns.Count)];
        }

        private class BoardSimulation
        {
            private char[,] grid = new char[6, 7];

            public BoardSimulation(Board original)
            {
                Array.Copy(original.GetCopyOfGrid(), grid, grid.Length);
            }

            public void DropDisc(int column, char symbol)
            {
                column -= 1;
                for (int r = 5; r >= 0; r--)
                {
                    if (grid[r, column] == '.')
                    {
                        grid[r, column] = symbol;
                        break;
                    }
                }
            }

            public bool CheckWin(char symbol)
            {
                for (int r = 0; r < 6; r++)
                    for (int c = 0; c < 4; c++)
                        if (Match(r, c, 0, 1, symbol)) return true;

                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 7; c++)
                        if (Match(r, c, 1, 0, symbol)) return true;

                for (int r = 3; r < 6; r++)
                    for (int c = 0; c < 4; c++)
                        if (Match(r, c, -1, 1, symbol)) return true;

                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 4; c++)
                        if (Match(r, c, 1, 1, symbol)) return true;

                return false;
            }

            private bool Match(int r, int c, int dr, int dc, char s)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (grid[r + i * dr, c + i * dc] != s)
                        return false;
                }
                return true;
            }
        }
    }
}
