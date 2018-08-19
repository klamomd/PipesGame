using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PipeTap.Enums;
using PipeTap.Utilities;

namespace PipeTap.Utilities
{
    public class LevelGenerator : MonoBehaviour
    {
        private System.Random r = new System.Random();
        private char[,] coloredTiles;
        private int startX, startY,
                    endX, endY,
                    xSize, ySize;


        // Returns a 2d array of Tuples containing a TileType and a rotation (0 <= rotation < 360).
        public Tuple<TileType, int>[,] GenerateNewLevel(int xSize, int ySize, int startX, int startY, int endX, int endY)
        //public Tuple<TileType, int>[,] GenerateNewLevel(int xSize, int ySize, Tuple<int, int> startCoords, Tuple<int, int> endCoords)
        {
            this.xSize = xSize;
            this.ySize = ySize;
            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;
            //startX = startCoords.Item1;
            //startY = startCoords.Item2;
            //endX = endCoords.Item1;
            //endY = endCoords.Item2;

            char[,] basicCharSet = GenerateCharSet();
            char[,] charSet = ConvertTileSet(basicCharSet);


            Tuple<TileType, int>[,] newLevelTiles = new Tuple<TileType, int>[charSet.GetLength(0), charSet.GetLength(1)];

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (IsBorderTile(x, y))
                    {
                        if (x == 0 && y == 0) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderBend, 180);
                        else if (x == 0 && y == ySize - 1) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderBend, 270);
                        else if (x == xSize - 1 && y == 0) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderBend, 90);
                        else if (x == xSize - 1 && y == ySize - 1) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderBend, 0);
                        else
                        {
                            if (x == 0 || x == xSize - 1) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderStraight, 90);
                            else if (y == 0 || y == ySize - 1) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderStraight, 0);
                        }

                        // Overriding border tiles that are directly above/below start/end pipes to be dead-end border tiles that are also rotated properly.
                        int rotation = 0;
                        if (x == startX && y == startY - 1)
                        {
                            // If this is a corner, needs to be rotated sideways.
                            if (x == 0 && y == 0) rotation = 180;
                            else rotation = 270;

                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderDeadEnd, rotation);
                        }
                        else if (x == startX && y == startY + 1)
                        {
                            // If this is a corner, needs to be rotated sideways.
                            if (x == 0 && y == ySize - 1) rotation = 180;
                            else rotation = 90;

                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderDeadEnd, rotation);
                        }
                        else if (x == endX && y == endY - 1)
                        {
                            // If this is a corner, needs to be rotated sideways.
                            if (x == xSize - 1 && y == 0) rotation = 0;
                            else rotation = 270;

                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderDeadEnd, rotation);
                        }
                        else if (x == endX && y == endY + 1)
                        {
                            // If this is a corner, needs to be rotated sideways.
                            if (x == xSize - 1 && y == 0) rotation = 0;
                            else rotation = 90;

                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderDeadEnd, rotation);
                        }

                        // Start/end tiles.
                        if (x == startX && y == startY)
                        {
                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.underGround, 180);
                        }
                        else if (x == endX && y == endY)
                        {
                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.underGround, 0);
                        }
                    }
                    else
                    {
                        switch (charSet[x, y])
                        {
                            case ' ':
                                newLevelTiles[x, y] = new Tuple<TileType, int>(GetRandomTileType(), GetRandomRotation());
                                break;
                            case 'b':
                                newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.bend, GetRandomRotation());
                                break;
                            case 's':
                                newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.straight, GetRandomRotation());
                                break;
                            case '3':
                                newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.threeWay, GetRandomRotation());
                                break;
                            case '4':
                                newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.fourWay, GetRandomRotation());
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                }
            }

            return newLevelTiles;



            //throw new NotImplementedException();
        }
        
        private char[,] GenerateCharSet()
        {
            //bool foundPath = false;
            while (true)
            //while (!foundPath)
            {
                coloredTiles = new char[xSize, ySize];

                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        if (x == startX && y == startY) coloredTiles[x, y] = 'S';
                        else if (x == endX && y == endY) coloredTiles[x, y] = 'E';
                        else coloredTiles[x, y] = IsBorderTile(x, y) ? '-' : ' ';
                    }
                }

                try
                {
                    ColorPath(startX, startY, Direction.Left);
                    //foundPath = true;
                    return coloredTiles;
                }
                catch { }
            }
        }

        private void ColorPath(int currentX, int currentY, Direction origin)
        {
            List<Tuple<Direction, Tuple<int, int>>> validTiles = new List<Tuple<Direction, Tuple<int, int>>>();

            List<Direction> sides = new List<Direction> { Direction.Bottom, Direction.Left, Direction.Right, Direction.Top };
            sides.Remove(origin);
            foreach (var s in sides)
            {
                Tuple<int, int> adjTile = GetAdjacentTileCoords(currentX, currentY, s);

                // Reached end tile, done recursing.
                if (adjTile.Item1 == endX && adjTile.Item2 == endY) return;

                // Foreach adjacent tile, if it is valid and empty, add it to our list of valid tiles (along with the direction it is in to make things easier later).
                if (IsTileValidAndEmpty(adjTile.Item1, adjTile.Item2))
                {
                    validTiles.Add(new Tuple<Direction, Tuple<int, int>>(s, adjTile));
                }
            }

            // If the algorithm has backed itself into a corner, throw an exception to fail out and try again.
            if (validTiles.Count == 0) throw new Exception();

            // Otherwise, choose a random tile to continue onto.
            int tileChoice = r.Next(validTiles.Count);
            Direction d = validTiles[tileChoice].Item1;
            Tuple<int, int> coords = validTiles[tileChoice].Item2;
            coloredTiles[coords.Item1, coords.Item2] = 'X';

            // Recurse.
            ColorPath(coords.Item1, coords.Item2, GetOppositeDirection(d));
        }

        private TileType GetRandomTileType()
        {
            int i = r.Next(7);
            switch (i)
            {
                case 0:
                    return TileType.dirt;
                case 1:
                    return TileType.closedEnd;
                case 2:
                    return TileType.bend;
                case 3:
                    return TileType.threeWay;
                case 4:
                    return TileType.fourWay;
                case 5:
                    return TileType.openEnd;
                case 6:
                    return TileType.straight;
                default: throw new Exception();
            }
        }

        // Returns true if the tile is a border tile.
        public bool IsBorderTile(int x, int y)
        {
            return x == 0 || x == xSize - 1 || y == 0 || y == ySize - 1;
        }

        // Returns true if the tile is within our borders (not including border tiles) and is empty.
        public bool IsTileValidAndEmpty(int x, int y)
        {
            if (x < 0 || x >= xSize || y < 0 || y >= ySize) return false;
            if (IsBorderTile(x, y)) return false;
            else return coloredTiles[x, y] == ' ';
        }

        // Returns true if the sides provided are opposites.
        private bool AreSidesOpposite(Direction side1, Direction side2)
        {
            switch (side1)
            {
                case Direction.Bottom:
                    return side2 == Direction.Top;
                case Direction.Left:
                    return side2 == Direction.Right;
                case Direction.Right:
                    return side2 == Direction.Left;
                case Direction.Top:
                    return side2 == Direction.Bottom;
                default:
                    throw new System.Exception();
            }
        }

        // Returns a random direction.
        public Direction GetRandomDirection()
        {
            int d = r.Next(4);
            switch (d)
            {
                case 0:
                    return Direction.Top;
                case 1:
                    return Direction.Bottom;
                case 2:
                    return Direction.Right;
                case 3:
                    return Direction.Left;
                default: throw new Exception();
            }
        }

        // Returns a random rotation.
        public int GetRandomRotation()
        {
            return r.Next(4) * 90;
        }

        // Returns the opposite direction to the one provided.
        // TODO: REDUNDANT - METHOD ALREADY EXISTS IN PipeFaceCalculator - REPLACE?
        public Direction GetOppositeDirection(Direction original)
        {
            switch (original)
            {
                case Direction.Bottom:
                    return Direction.Top;
                case Direction.Top:
                    return Direction.Bottom;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                default: throw new Exception();
            }
        }

        // Given the coordinates of a tile and the direction of an adjacent tile, returns the adjacent tile's coordinates in a tuple.
        public Tuple<int, int> GetAdjacentTileCoords(int x, int y, Direction d)
        {
            switch (d)
            {
                case Direction.Top:
                    return new Tuple<int, int>(x, y + 1);
                case Direction.Bottom:
                    return new Tuple<int, int>(x, y - 1);
                case Direction.Right:
                    return new Tuple<int, int>(x + 1, y);
                case Direction.Left:
                    return new Tuple<int, int>(x - 1, y);
                default: throw new Exception();
            }
        }

        private char[,] ConvertTileSet(char[,] tileSet)
        {
            char[,] convertedTileSet = new char[xSize, ySize];

            for (int x = 0; x < xSize; x++)
            {
                convertedTileSet[x, 0] = '-';
                convertedTileSet[x, ySize - 1] = '-';
            }

            for (int y = 0; y < ySize; y++)
            {
                convertedTileSet[0, y] = '-';
                convertedTileSet[xSize - 1, y] = '-';
            }

            for (int x = 1; x < xSize - 1; x++)
            {
                for (int y = 1; y < ySize - 1; y++)
                {
                    TileType type = GetTileType(x, y);
                    switch (type)
                    {
                        case TileType.bend:
                            convertedTileSet[x, y] = 'b';
                            break;
                        case TileType.straight:
                            convertedTileSet[x, y] = 's';
                            break;
                        case TileType.threeWay:
                            convertedTileSet[x, y] = '3';
                            break;
                        case TileType.fourWay:
                            convertedTileSet[x, y] = '4';
                            break;
                        case TileType.dirt:
                            convertedTileSet[x, y] = ' ';
                            break;
                        default:
                            throw new Exception();
                    }
                }
            }

            return convertedTileSet;
        }

        private TileType GetTileType(int x, int y)
        {
            if (coloredTiles[x, y] == ' ') return TileType.dirt;

            List<Direction> adjacentTiles = new List<Direction>();

            List<char> validAdjacentTiles = new List<char> { 'X', 'S', 'E' };
            if (validAdjacentTiles.Contains(coloredTiles[x - 1, y])) adjacentTiles.Add(Direction.Left);
            if (validAdjacentTiles.Contains(coloredTiles[x + 1, y])) adjacentTiles.Add(Direction.Right);
            if (validAdjacentTiles.Contains(coloredTiles[x, y + 1])) adjacentTiles.Add(Direction.Bottom);
            if (validAdjacentTiles.Contains(coloredTiles[x, y - 1])) adjacentTiles.Add(Direction.Top);

            switch (adjacentTiles.Count)
            {
                case 4:
                    return TileType.fourWay;
                case 3:
                    return TileType.threeWay;
                case 2:
                    return AreSidesOpposite(adjacentTiles[0], adjacentTiles[1]) ? TileType.straight : TileType.bend;
                default:
                    throw new Exception();
            }
        }
    }

    //// Unity does not have Tuples, so here's an implementation for me to use.
    //public class Tuple<T1, T2>
    //{
    //    public T1 Item1 { get; private set; }
    //    public T2 Item2 { get; private set; }
    //    internal Tuple(T1 item1, T2 item2)
    //    {
    //        Item1 = item1;
    //        Item2 = item2;
    //    }
    //}
}