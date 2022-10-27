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
    /// <summary>Holds information about a game board of a player</summary>
    /// <param name="board">Array with placed ships</param>
    public class GameBoard
    {
        public int[,] board{get; set;}

        private int aliveShipsSegments = 18;

        public GameBoard(int[,] board) {
            this.board = board;
        }
        /// <summary>Prints the board in the terminal</summary>
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
        /// <summary>Checks a result of a shot</summary>
        /// <param name="row">Row cell number</param>
        /// <param name="column">Column cell number</param>
        /// <returns>Shot result</returns>
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
            if(alive == true) {
                return ShotStatus.Hit;
            } else {
                return ShotStatus.Destroyed;
            }
        }
        /// <summary>Checks if a ship has not damaged segments</summary>
        /// <param name="row">Row cell number</param>
        /// <param name="column">Column cell number</param>
        /// <returns>True if ship is still alive</returns>
        private bool CheckShipHealth(int row, int column) {
            (bool tried, bool alive) firstSide = (false, false);
            (bool tried, bool alive) secondSide = (false, false);

            for (var rowIndex = row - 1; rowIndex <= row + 1; rowIndex++) {
                for (var columnIndex = column - 1; columnIndex <= column + 1; columnIndex++) {
                    if(columnIndex < 0 || columnIndex > 9 || rowIndex < 0 || rowIndex > 9) continue;
                    
                    if(board[rowIndex, columnIndex] != 0) {
                        ShipOrientation orientation = rowIndex - row != 0 ? ShipOrientation.Horizontal : ShipOrientation.Vertical;

                        if(rowIndex == row && columnIndex == column) {
                            continue;
                        }

                        int direction; // determine the direction to check next
                        
                        if(orientation == ShipOrientation.Horizontal) {
                            direction = rowIndex - row;
                        } else {
                            direction = columnIndex - column;
                        }
                        
                        if(firstSide.tried == false) {
                            firstSide.tried = true;
                            firstSide.alive = CheckNextSegment(orientation, direction, column, row);
                        } else {
                            secondSide.tried = true;
                            secondSide.alive = CheckNextSegment(orientation, direction, column, row);
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
        /// <summary>Searches and cheacks every ship segments to see if it is alive</summary>
        /// <param name="orientation">Ship orientation on the board</param>
        /// <param name="direction">In which direction the checks should be made</param>
        /// <param name="column">Column cell number</param>
        /// <param name="row">Row cell number</param>
        /// <param name="segment">Number of currenty checked segment</param>
        /// <param name="alive">True if there has been a not demaged segment in any of the previous method calls</param>
        /// <returns>True if there are not damaged segments left</returns>

        private bool CheckNextSegment(ShipOrientation orientation, int direction, int column, int row, int segment = 1, bool alive = true) {
            int newCol = orientation == ShipOrientation.Vertical ? column + segment * direction : column;
            int newRow = orientation == ShipOrientation.Horizontal ? row + segment * direction : row;

            if(newCol < 0 || newCol > 9 || newRow < 0 || newRow > 9) return alive;

            if(board[newRow, newCol] == 1){
                return true;
            } else if (board[newRow, newCol] == 0) {
                return alive;
            } else {
                alive = false;
                return CheckNextSegment(orientation, direction, column, row, segment + 1, alive);
            };
        }
    }
}