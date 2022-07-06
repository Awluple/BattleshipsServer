using System;
using System.Collections.Generic;
using BattleshipsShared.Communication;


namespace BattleshipsServer.Board
{
     public enum ShipOrientation {
        Horizontal,
        Vertical
    }

    public struct Coords
    {
        public Coords(int col, int row)
        {
            Column = col;
            Row = row;
        }
    
        public int Column { get; }
        public int Row { get; }
    }

    public class GameBoard
    {
        public int[,] board{get; set;}

        public GameBoard(int[,] board) {
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

        public ShotStatus CheckShot(int row, int column) {
            if(board[row, column] == 1) {
                board[row, column] = -1;
            } else {
                return ShotStatus.Miss;
            }
            bool alive = this.CheckShipHealth(row, column);
            ShowBoard();
            if(alive == true) {
                return ShotStatus.Hit;
            } else {
                return ShotStatus.Destroyed;
            }
        }

        private bool CheckShipHealth(int row, int column) {
            (bool tried, bool alive) firstSide = (false, true);
            (bool tried, bool alive) secondSide = (false, true);

            for (var rowIndex = row - 1; rowIndex <= row + 1; rowIndex++) {
                for (var columnIndex = column - 1; columnIndex <= column + 1; columnIndex++) {
                    if(columnIndex < 0 || columnIndex >= 11 || rowIndex < 0 || rowIndex >= 11) continue;
                    if(board[rowIndex, columnIndex] != 0){
                        ShipOrientation orientation = rowIndex - row != 0 ? ShipOrientation.Horizontal : ShipOrientation.Vertical;

                        if(rowIndex == row && columnIndex == column) {
                            continue;
                        }
                        
                        if(firstSide.tried == false) {
                            firstSide.tried = true;
                            firstSide.alive = CheckNextSegment(orientation, 1, column, row);
                        } else {
                            secondSide.tried = true;
                            secondSide.alive = CheckNextSegment(orientation, -1, column, row);
                        }
                    };
                }
            }

            if(firstSide.alive || secondSide.alive) {
                if(firstSide.tried || secondSide.tried){
                    return true;
                } else {
                    return false; // mark as destroyed for 1 segment ships
                }
            } else {
                return false;
            }

        }

        private bool CheckNextSegment(ShipOrientation orientation, int vector, int column, int row, int segment = 1, bool alive = true) {
            int newCol = orientation == ShipOrientation.Vertical ? column + segment * vector : column;
            int newRow = orientation == ShipOrientation.Horizontal ? row + segment * vector : row;

            if(newCol < 0 || newCol > 9 || newRow < 0 || newRow > 9) return alive;

            if(board[newRow, newCol] == 1){
                return true;
            } else if (board[newRow, newCol] == 0) {
                return alive;
            } else {
                alive = false;
                return CheckNextSegment(orientation, vector, column, row, segment + 1, alive);
            };
        }
    }
}