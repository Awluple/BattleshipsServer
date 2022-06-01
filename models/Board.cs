using System;

namespace BattleshipsServer.Board
{
    public class Board
    {
        public int[,] board{get; set;}

        public Board(int[,] board) {
            this.board = board;
        }

        public void ShowBoard() {
            int rowLength = board.GetLength(0);
            int colLength = board.GetLength(1);

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    Console.Write(string.Format("|{0}| ", board[i, j]));
                }
                Console.WriteLine("\n-------------------------------");
            }
        }
    }
}