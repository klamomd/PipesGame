using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PipeTap.Enums;
using PipeTap.Utilities;

namespace PipeTap.Utilities
{
    public class LevelGenerator
    {
        private System.Random rand = new System.Random();

        private char[,] tileCharacters;       // 2d array to keep track of every tile's type, represented as a character.
                                              // Characters in this container (used only when generating a basic character set) can be 'S' - Start, 'E' - End, '-' - Border, ' ' - Empty, 'X' - Path

        private int startX, startY,           // Coordinates of Start and End tiles, and grid dimensions.
                    endX, endY,
                    xSize, ySize;


        // Returns a 2d array of Tuples containing a TileType (Tuple.Item1) and a rotation (0 <= rotation < 360) (Tuple.Item2). This array can then be used to populate an empty TileMap.
        public Tuple<TileType, int>[,] GenerateNewLevel(int _xSize, int _ySize, int _startX, int _startY, int _endX, int _endY)
        {
            // Store all values locally.
            xSize = _xSize;
            ySize = _ySize;
            startX = _startX;
            startY = _startY;
            endX = _endX;
            endY = _endY;

            // Generate the basic character set, which contains all tiles and a path from Start to End.
            char[,] basicCharSet = GenerateCharSet();
            // Convert the basic character set into a refined character set.
            char[,] charSet = ConvertTileSet(basicCharSet);


            // Create a container to store our individual tiles and their rotations.
            Tuple<TileType, int>[,] newLevelTiles = new Tuple<TileType, int>[charSet.GetLength(0), charSet.GetLength(1)];

            // Loop through every tile on the generated map and create a corresponding Tuple<TileType, int> to store in the above container.
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    // Border tiles have to be handled separately, as they have special cases.
                    if (IsBorderTile(x, y))
                    {
                        // First we broadly assign all border tiles the standard bend or straight tile and the appropriate rotation, based on where they are located.
                        if (x == 0 && y == 0) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderBend, 180);
                        else if (x == 0 && y == ySize - 1) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderBend, 270);
                        else if (x == xSize - 1 && y == 0) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderBend, 90);
                        else if (x == xSize - 1 && y == ySize - 1) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderBend, 0);
                        else
                        {
                            if (x == 0 || x == xSize - 1) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderStraight, 90);
                            else if (y == 0 || y == ySize - 1) newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderStraight, 0);
                        }


                        // Next we reassign border tiles that are directly above/below Start/End pipes to be dead-end border tiles.

                        int rotation = 0;
                        if (x == startX && y == startY - 1)             // Check above the Start pipe.
                        {
                            // If this is a corner, needs to be rotated sideways.
                            if (x == 0 && y == 0) rotation = 180;
                            else rotation = 270;

                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderDeadEnd, rotation);
                        }
                        else if (x == startX && y == startY + 1)         // Check below the Start pipe.
                        {
                            // If this is a corner, needs to be rotated sideways.
                            if (x == 0 && y == ySize - 1) rotation = 180;
                            else rotation = 90;

                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderDeadEnd, rotation);
                        }
                        else if (x == endX && y == endY - 1)            // Check above the End pipe.
                        {
                            // If this is a corner, needs to be rotated sideways.
                            if (x == xSize - 1 && y == 0) rotation = 0;
                            else rotation = 270;

                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderDeadEnd, rotation);
                        }
                        else if (x == endX && y == endY + 1)            // Check below the End pipe.
                        {
                            // If this is a corner, needs to be rotated sideways.
                            if (x == xSize - 1 && y == ySize - 1) rotation = 0;
                            else rotation = 90;

                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.borderDeadEnd, rotation);
                        }


                        // Finally, tiles at the Start and End coordinates are reassigned to have a unique TileType.
                        if (x == startX && y == startY)
                        {
                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.underGround, 180);
                        }
                        else if (x == endX && y == endY)
                        {
                            newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.underGround, 0);
                        }
                    }

                    // Non-border tiles are easily converted into their corresponding TileTypes with a simple switch-case.
                    else
                    {
                        switch (charSet[x, y])
                        {
                            // Empty tiles are filled with a randomly generated TileType.
                            case ' ':
                                newLevelTiles[x, y] = new Tuple<TileType, int>(GetRandomTileType(), GetRandomRotation());
                                break;
                            // 'b' represents bend tiles.
                            case 'b':
                                newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.bend, GetRandomRotation());
                                break;
                            // 's' represents straight tiles.
                            case 's':
                                newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.straight, GetRandomRotation());
                                break;
                            // '3' represents three-way intersections, or T-tiles.
                            case '3':
                                newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.threeWay, GetRandomRotation());
                                break;
                            case '4':
                            // '4' represents four-way intersections, or cross tiles.
                                newLevelTiles[x, y] = new Tuple<TileType, int>(TileType.fourWay, GetRandomRotation());
                                break;
                            // No other tile types are expected with the current level generator.
                            default:
                                throw new Exception(string.Format("Unexpected character for tile at ({0}, {1}): {2}", x, y, charSet[x,y]));
                        }
                    }
                }
            }

            return newLevelTiles;
        }
        

        // GenerateCharSet is a crude function that brute-forces generating a level. It relies on the recursive method ColorPath, and simply reruns it every time it throws an exception, then returns the result when it finally succeeds.
        private char[,] GenerateCharSet()
        {
            while (true)
            {
                // Reset the list of tile characters.
                tileCharacters = new char[xSize, ySize];


                // TODO: Move this outside of while loop, store as blank charset, use that to reset tileCharacters each time? Concern: Would a clean copy be made of the blank charset each time, or is there a chance it gets changed as tileCharacters does?

                // Assign each tile a character based on its location.
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        if (x == startX && y == startY) tileCharacters[x, y] = 'S';     // Start tile.
                        else if (x == endX && y == endY) tileCharacters[x, y] = 'E';    // End tile.
                        else tileCharacters[x, y] = IsBorderTile(x, y) ? '-' : ' ';     // Border or empty tile.
                    }
                }

                // Pass the blank character grid to ColorPath. If it finishes without hitting an exception, return tileCharacters (which will now have a path from Start to End).
                try
                {
                    ColorPath(startX, startY, Direction.Left);
                    return tileCharacters;
                }
                catch { }
            }
        }

        // A recursive method which randomly chooses a path through the tiles and, hopefully, passes next to the End tile (at which point a valid path is found and recursion stops).
        // If it gets stuck, meaning if it has no more branches to explore that are uncolored, it throws an exception to indicate so.
        // currentX and currentY represent the coordinates of the current tile. origin indicates the direction of the parent tile relative to the current tile.
        // When ColorPath is called initially, currentX and currentY should be the coordinates of the Start tile and origin should be Direction.Left (as right now, Start tiles always have their opening facing to the right).
        private void ColorPath(int currentX, int currentY, Direction origin)
        {
            // validTiles will contain all tiles that are adjacent to the current tile and that have not been explored yet. 
            List<Tuple<Direction, Tuple<int, int>>> validTiles = new List<Tuple<Direction, Tuple<int, int>>>();

            // Create a list of all adjacent sides, but ignore the tile we just came from.
            List<Direction> sides = new List<Direction> { Direction.Bottom, Direction.Left, Direction.Right, Direction.Top };
            sides.Remove(origin);

            // Check each adjacent tile.
            foreach (var s in sides)
            {
                Tuple<int, int> adjTile = GetAdjacentTileCoords(currentX, currentY, s);

                // If we're adjacent to the End tile, we've found a valid path and we're done recursing.
                if (adjTile.Item1 == endX && adjTile.Item2 == endY) return;

                // For each adjacent tile, if it is valid (within the borders) and hasn't been added to the path yet, add it to our list of valid tiles (along with the direction it is in to make things easier later).
                if (IsTileValidAndEmpty(adjTile.Item1, adjTile.Item2))
                {
                    validTiles.Add(new Tuple<Direction, Tuple<int, int>>(s, adjTile));
                }
            }


            // If the algorithm has backed itself into a corner and there are no remaining valid tiles, throw an exception to fail out and try again.
            if (validTiles.Count == 0) throw new Exception();


            // Otherwise, choose a random tile to continue onto.
            int tileChoice = rand.Next(validTiles.Count);

            // Pull the stored direction and coordinates from the Tuple.
            Direction nextTileDirection = validTiles[tileChoice].Item1;
            Tuple<int, int> adjCoords = validTiles[tileChoice].Item2;

            // Reassign the character for the given tile to an X, indicating it's part of the path.
            tileCharacters[adjCoords.Item1, adjCoords.Item2] = 'X';

            // Recurse by passing the adjacent tile coordinates and the direction opposite to how we're moving.
            ColorPath(adjCoords.Item1, adjCoords.Item2, GetOppositeDirection(nextTileDirection));
        }

        // Returns a random tile type (excluding borders and other special types).
        private TileType GetRandomTileType()
        {
            int i = rand.Next(7);
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

        // Returns true if the tile is within our borders (excluding border tiles) and is empty.
        public bool IsTileValidAndEmpty(int x, int y)
        {
            if (x < 0 || x >= xSize || y < 0 || y >= ySize) return false;
            if (IsBorderTile(x, y)) return false;
            else return tileCharacters[x, y] == ' ';
        }

        // Returns true if the directions provided are opposites.
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
            int d = rand.Next(4);
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

        // Returns a random rotation between [0, 360).
        public int GetRandomRotation()
        {
            return rand.Next(4) * 90;
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

        // Given a basic charset generated by GenerateCharSet(), convert every path tile (represented as 'X') to a more specific character ('b' - bend, 's' - straight, '3' - three way intersection, '4' - four way intersection).
        private char[,] ConvertTileSet(char[,] tileSet)
        {
            // Create a new 2d char array to store everything in.
            char[,] convertedTileSet = new char[xSize, ySize];

            // Auto-fill the top and bottom borders.
            for (int x = 0; x < xSize; x++)
            {
                convertedTileSet[x, 0] = '-';
                convertedTileSet[x, ySize - 1] = '-';
            }

            // Auto-fill the left and right borders.
            for (int y = 0; y < ySize; y++)
            {
                convertedTileSet[0, y] = '-';
                convertedTileSet[xSize - 1, y] = '-';
            }

            // Loop through every tile (excluding borders) and call GetTileType on it to determine what it should be. Then store the corresponding character in our convertedTileSet.
            for (int x = 1; x < xSize - 1; x++)
            {
                for (int y = 1; y < ySize - 1; y++)
                {
                    TileType type = GetTileType(tileSet, x, y);
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

        // Given a tile charset and coordinates, returns the TileType represented by the character at those coordinates.
        private TileType GetTileType(char[,] charSet, int x, int y)
        {
            // If the character is a space, it's a dirt tile.
            if (charSet[x, y] == ' ') return TileType.dirt;

            // Create an empty list to store the directions of adjacent tiles; the total count will be used later to determine what type of tile this is.
            List<Direction> adjacentConnectedTiles = new List<Direction>();

            // Connectable tiles can be 'S' - Start, 'E' - End, or 'X' - Path. Border and dirt tiles are not connectable.
            List<char> connectableTiles = new List<char> { 'X', 'S', 'E' };

            // Check each adjacent tile to see if it is a valid connection. If so, store the corresponding direction.
            if (connectableTiles.Contains(charSet[x - 1, y])) adjacentConnectedTiles.Add(Direction.Left);
            if (connectableTiles.Contains(charSet[x + 1, y])) adjacentConnectedTiles.Add(Direction.Right);
            if (connectableTiles.Contains(charSet[x, y + 1])) adjacentConnectedTiles.Add(Direction.Bottom);
            if (connectableTiles.Contains(charSet[x, y - 1])) adjacentConnectedTiles.Add(Direction.Top);

            // Based on the number of adjacent connected tiles, return the appropriate tile type.
            switch (adjacentConnectedTiles.Count)
            {
                case 4:
                    return TileType.fourWay;
                case 3:
                    return TileType.threeWay;
                case 2:
                    // If there are 2 adjacent connected tiles, we check if they are on opposite sides. If so, the tile is straight, otherwise bent.
                    return AreSidesOpposite(adjacentConnectedTiles[0], adjacentConnectedTiles[1]) ? TileType.straight : TileType.bend;
                default:
                    throw new Exception();
            }
        }
    }
}