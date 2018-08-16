using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeTapLevelGenerator
{
    class Program
    {
        static Random r = new Random();

        static int xMin = 0,
                   xMax = 18,
                   yMin = 0,
                   yMax = 10,
                   startX = 0,
                   startY = 6,
                   endX = 18,
                   endY = 3,
                   xSize = Math.Abs(xMin) + Math.Abs(xMax) + 1,
                   ySize = Math.Abs(yMin) + Math.Abs(yMax) + 1;

        static char[,] coloredTiles;



        static void Main(string[] args)
        {
            

            bool foundPath = false;
            while (!foundPath)
            {
                coloredTiles = new char[xSize, ySize];

                for (int x = xMin; x <= xMax; x++)
                {
                    for (int y = yMin; y <= yMax; y++)
                    {
                        if (x == startX && y == startY) coloredTiles[x, y] = 'S';
                        else if (x == endX && y == endY) coloredTiles[x, y] = 'E';
                        else coloredTiles[x, y] = IsBorderTile(x, y) ? '-' : ' ';
                    }
                }

                try
                {
                    ColorPath(startX, startY, Direction.Left);
                    foundPath = true;
                }
                catch
                {

                }
            }


            //LEFTOFF: WORKS! Now just need to write a conversion method to replace each X with its appropriate pipe.
            // Spaces with 2 adjacent tiles (INCLUDING START AND END TILES) can be:
            // = Straight pipe (2 adjacent on opposite sides)
            // = Bend pipe (2 adjacent on touching sides)
            // Spaces with 3 adjacent tiles are T-pipes.
            // Spaces with 4 adjacent tiles are cross-pipes.
            // NOTE: No need to rotate them to their correct positions, since they'll just be shuffled anyways.
            


            PrintToConsole(coloredTiles);
            Console.WriteLine();
            PrintToConsole(ConvertTileSet(coloredTiles));
            Console.Read();
        }

        public static void ColorPath(int currentX, int currentY, Direction origin)
        {
            List <Tuple<Direction,Tuple<int, int>>> validTiles = new List<Tuple<Direction, Tuple<int, int>>>();

            List<Direction> sides = new List<Direction> { Direction.Down, Direction.Left, Direction.Right, Direction.Up };
            sides.Remove(origin);
            foreach (var s in sides)
            {
                Tuple<int, int> adjTile = GetAdjacentTileCoords(currentX, currentY, s);
                if (adjTile.Item1 == endX && adjTile.Item2 == endY) return; // Reached end tile.
                if (IsTileValid(adjTile.Item1, adjTile.Item2))
                {
                    validTiles.Add(new Tuple<Direction, Tuple<int, int>>(s, adjTile));
                }
            }

            if (validTiles.Count == 0)
            {
                //Console.WriteLine("Backed myself into a corner :<");
                throw new Exception();
            }

            int tileChoice = r.Next(validTiles.Count);
            Direction d = validTiles[tileChoice].Item1;
            Tuple<int, int> coords = validTiles[tileChoice].Item2;
            coloredTiles[coords.Item1, coords.Item2] = 'X';
            ColorPath(coords.Item1, coords.Item2, GetOppositeDirection(d));
        }

        public static bool IsTileValid(int x, int y)
        {
            if (x < xMin || x > xMax || y < yMin || y > yMax) return false;
            if (IsBorderTile(x, y)) return false;
            else return coloredTiles[x, y] == ' ';
        }

        public static bool IsBorderTile(int x, int y)
        {
            return x == xMin || x == xMax || y == yMin || y == yMax;
        }

        public static Direction GetRandomDirection()
        {
            int d = r.Next(4);
            switch (d)
            {
                case 0:
                    return Direction.Up;
                case 1:
                    return Direction.Down;
                case 2:
                    return Direction.Right;
                case 3:
                    return Direction.Left;
                default: throw new Exception();
            }
        }

        public static Direction GetOppositeDirection(Direction original)
        {
            switch (original)
            {
                case Direction.Down:
                    return Direction.Up;
                case Direction.Up:
                    return Direction.Down;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                default: throw new Exception();
            }
        }

        public static TileType GetTileType(int x, int y)
        {
            if (coloredTiles[x, y] == ' ') return TileType.dirt;

            List<Direction> adjacentTiles = new List<Direction>();

            List<char> validAdjacentTiles = new List<char> { 'X', 'S', 'E' };
            if (validAdjacentTiles.Contains(coloredTiles[x - 1, y])) adjacentTiles.Add(Direction.Left);
            if (validAdjacentTiles.Contains(coloredTiles[x + 1, y])) adjacentTiles.Add(Direction.Right);
            if (validAdjacentTiles.Contains(coloredTiles[x, y + 1])) adjacentTiles.Add(Direction.Down);
            if (validAdjacentTiles.Contains(coloredTiles[x, y - 1])) adjacentTiles.Add(Direction.Up);

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

        public static Tuple<int, int> GetAdjacentTileCoords(int x, int y, Direction d)
        {
            switch (d)
            {
                case Direction.Up:
                    return new Tuple<int, int>(x, y + 1);
                case Direction.Down:
                    return new Tuple<int, int>(x, y - 1);
                case Direction.Right:
                    return new Tuple<int, int>(x + 1, y);
                case Direction.Left:
                    return new Tuple<int, int>(x - 1, y);
                default: throw new Exception();
            }
        }

        public static void PrintToConsole(char[,] tileSet)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    Console.Write(tileSet[x, y]);
                }
                Console.WriteLine();
            }
        }

        public static char[,] ConvertTileSet(char[,] tileSet)
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


        //public static char[,] FillEmptySlots(char[,] tileSet)
        //{
        //    //TODO: GENERATE RANDOM PIECES TO FILL BOARD
        //    //TODO: ADJUST RATES AT WHICH PIECES ARE CHOSEN
        //}

        //TODO: ROTATE ALL TILES RANDOMLY


        public static bool AreSidesOpposite(Direction side1, Direction side2)
        {
            switch (side1)
            {
                case Direction.Down:
                    return side2 == Direction.Up;
                case Direction.Left:
                    return side2 == Direction.Right;
                case Direction.Right:
                    return side2 == Direction.Left;
                case Direction.Up:
                    return side2 == Direction.Down;
                default:
                    throw new Exception();
            }
        }

        public enum Direction
        {
            Up,
            Down,
            Right,
            Left
        }

        public enum TileType
        {
            straight,
            bend,
            threeWay,
            fourWay,
            dirt
        }

        public class Cell
        {
            public Cell(int x, int y, bool isBorder)
            {
                X = x;
                Y = y;
                IsFilled = false;
                IsBorder = isBorder;
            }

            public int X { get; }
            public int Y { get; }
            public bool IsFilled { get; private set; }
            public bool IsBorder { get; }

            public bool SetFilled (bool filled)
            {
                if (IsBorder) return false;
                IsFilled = filled;
                return true;
            }
        }
    }
}
