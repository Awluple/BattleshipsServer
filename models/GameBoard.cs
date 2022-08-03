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

        private int aliveShipsSegments = 18;

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
                aliveShipsSegments--;

                if(aliveShipsSegments == 0) {
                    return ShotStatus.Finished;
                }
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
            (bool tried, bool alive) firstSide = (false, false);
            (bool tried, bool alive) secondSide = (false, false);

            for (var rowIndex = row - 1; rowIndex <= row + 1; rowIndex++) {
                for (var columnIndex = column - 1; columnIndex <= column + 1; columnIndex++) {
                    if(columnIndex < 0 || columnIndex > 9 || rowIndex < 0 || rowIndex > 9) continue;
                    if(board[rowIndex, columnIndex] != 0){
                        ShipOrientation orientation = rowIndex - row != 0 ? ShipOrientation.Horizontal : ShipOrientation.Vertical;

                        if(rowIndex == row && columnIndex == column) {
                            continue;
                        }

                        int vector;

                        if(orientation == ShipOrientation.Horizontal) {
                            vector = rowIndex - row;
                        } else {
                            vector = columnIndex - column;
                        }
                        
                        if(firstSide.tried == false) {
                            firstSide.tried = true;
                            firstSide.alive = CheckNextSegment(orientation, vector, column, row);
                        } else {
                            secondSide.tried = true;
                            secondSide.alive = CheckNextSegment(orientation, vector, column, row);
                        }
                    };
                }
            }

            if(firstSide.tried || secondSide.tried) {
                return firstSide.alive || secondSide.alive;
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