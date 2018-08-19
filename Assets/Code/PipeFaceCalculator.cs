using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PipeTap.Enums;
using UnityEngine.Tilemaps;

namespace PipeTap.Utilities
{
    public class PipeFaceCalculator : MonoBehaviour
    {
        private const bool __IS_DEBUG = true;

        private Tilemap map;
        public int borderXAbs = 9,
                   borderYAbs = 5;
        private int startPipeX,
                    startPipeY,
                    endPipeX,
                    endPipeY;
        private List<Tile> coloredTiles = new List<Tile>();
        private bool foundLeaks = false;

        public PipeFaceCalculator()
        {

        }
        public PipeFaceCalculator(Tilemap map)
        {
            this.map = map;
        }

        public bool CheckIfSolutionFound(Tilemap mapToCheck, int startX, int startY, int endX, int endY)
        {
            if (mapToCheck == null) throw new System.Exception("Null Tilemap");
            map = mapToCheck;
            foundLeaks = false;
            startPipeX = startX;
            startPipeY = startY;
            endPipeX = endX;
            endPipeY = endY;


            // Step 1: 
            // Reset the list of coloredTiles.
            coloredTiles.Clear();
            for (int x = -8; x < 9; x++)
            {
                for (int y = -4; y < 5; y++)
                {
                    ColorTile(x, y, false);
                }
            }


            // Step 2:
            // Start with the start tile. Return true if FindSolution returns true, and if the End Pipe exists in the coloredTiles list.
            return FindSolution(new Tile(startX, startY), Direction.Left) && coloredTiles.Any<Tile>(t => t.EqualsTile(new Tile(endPipeX, endPipeY)));
        }

        // FindSolution is given the currentTile to branch through, and a Direction completedDirection to NOT branch through.
        private bool FindSolution(Tile currentTile, Direction completedDirection)
        {
            //Debug.Log(string.Format("||ENTERING [X: {0} Y: {1} completedDirection: {2}]", currentTile.X, currentTile.Y, completedDirection));


            coloredTiles.Add(currentTile);
            if (__IS_DEBUG)
            {
                ColorTile(currentTile, true);
            }

            // Get all directions, but remove the one we've completed.
            List<Direction> allSides = GetOpenTileFaces(currentTile);
            allSides.Remove(completedDirection);

            // For each one, check the adjacent side. If it is valid, recurse. Go through all of them.
            foreach (Direction d in allSides)
            {
                // If this check fails, then the side is a leak and we don't branch down it any further.
                if (!CheckAdjacentFaceValid(currentTile.X, currentTile.Y, d))
                {
                    //Debug.Log(string.Format("~~RETURNING FALSE [X: {0} Y: {1} d: {2}]", currentTile.X, currentTile.Y, d));
                    //return false;
                    foundLeaks = true;
                    continue;
                }

                // Adjacent tile.
                Tile adjacentTile = GetAdjacentTile(currentTile, d);
                // If we pass the leak test, and if this branch is open (and uncolored), continue through it.

                bool isTileColored = coloredTiles.Any(t => t.EqualsTile(adjacentTile));
                if (isTileColored) continue;

                if (IsFaceOpen(currentTile.X, currentTile.Y, d))
                {
                    // Return false if we fail to find a solution.
                    if (!FindSolution(adjacentTile, GetOppositeDirection(d)))
                    {
                        //Debug.Log(string.Format("~~RETURNING FALSE [X: {0} Y: {1} d: {2}]", adjacentTile.X, adjacentTile.Y, GetOppositeDirection(d)));
                        //return false;
                        foundLeaks = true;
                        continue;
                    }
                }
            }

            //// Return true if we have the end tile in our coloredTiles list AND if no leaks have been found.
            //return coloredTiles.Any<Tile>(t => t.EqualsTile(new Tile(endPipeX, endPipeY))) && !foundLeaks;

            // Return true if no leaks have been found. If all branches return true, then FindSolution returns true.
            return !foundLeaks;
        }


        public Tile GetAdjacentTile(Tile currentTile, Direction direction)
        {
            int adjX = currentTile.X, adjY = currentTile.Y;
            if (direction == Direction.Left)
            {
                adjX--;
            }
            else if (direction == Direction.Top)
            {
                adjY++;
            }
            else if (direction == Direction.Right)
            {
                adjX++;
            }
            else if (direction == Direction.Bottom)
            {
                adjY--;
            }
            else throw new System.Exception("Invalid direction!");

            return new Tile(adjX, adjY);
        }

        // Check whether our pipe is open in the given direction. If not, return true (doesn't matter what the adjacent tile is, we're not going there).
        // If so, check whether the adjacent tile is also open, and if so we have a connection and return true. If not, we have a leak and return false.
        public bool CheckAdjacentFaceValid(int pipeX, int pipeY, Direction face)
        {
            bool thisFaceIsOpen = IsFaceOpen(pipeX, pipeY, face);

            // If we don't have an opening, then the adjacent face is valid regardless.
            if (!thisFaceIsOpen)
            {
                return true;
            }

            Direction oppositeFace = GetOppositeDirection(face);

            // Determine adjacent tile's x and y:
            int adjX = pipeX, adjY = pipeY;
            if (face == Direction.Left)
            {
                adjX--;
            }
            else if (face == Direction.Top)
            {
                adjY++;
            }
            else if (face == Direction.Right)
            {
                adjX++;
            }
            else if (face == Direction.Bottom)
            {
                adjY--;
            }
            else throw new System.Exception("Invalid direction!");

            // If we have found the end pipe, do not treat it as a border tile. We have to check it.
            bool isEndPipe = (adjX == endPipeX && adjY == endPipeY);
            if (!isEndPipe)
            {
                // If we've already determined we have an open face (wouldn't reach here if we didn't) and the adjacent tile is the border, then it is not valid as we have a leak.
                if (Mathf.Abs(adjX) >= borderXAbs)
                {
                    return false;
                }
                if (Mathf.Abs(adjY) >= borderYAbs)
                {
                    return false;
                }
            }

            // Return true if the adjacent pipe is open toward this pipe, signifying a connection.
            return IsFaceOpen(adjX, adjY, oppositeFace);
        }

        // Determine whether the provided pipe is open on a certain side by checking its rotation.
        public bool IsFaceOpen(int pipeX, int pipeY, Direction face)
        {
            TileType tileType = GetTileType(pipeX, pipeY);
            int rotation = GetRotationAngle(pipeX, pipeY);
            switch (tileType)
            {
                case TileType.bend:
                    if (face == Direction.Left) { return rotation == 0 || rotation == 90; }
                    else if (face == Direction.Top) { return rotation == 0 || rotation == 270; }
                    else if (face == Direction.Right) { return rotation == 180 || rotation == 270; }
                    else { return rotation == 180 || rotation == 90; }

                case TileType.closedEnd:
                case TileType.openEnd:
                case TileType.underGround:
                    if (face == Direction.Left) { return rotation == 0; }
                    else if (face == Direction.Top) { return rotation == 270; }
                    else if (face == Direction.Right) { return rotation == 180; }
                    else { return rotation == 90; }

                case TileType.dirt:
                    return false;
                case TileType.fourWay:
                    return true;

                case TileType.straight:
                    if (face == Direction.Left || face == Direction.Right) { return rotation == 0 || rotation == 180; }
                    else { return rotation == 90 || rotation == 270; }

                case TileType.threeWay:
                    if (face == Direction.Left) { return rotation != 180; }
                    else if (face == Direction.Top) { return rotation != 90; }
                    else if (face == Direction.Right) { return rotation != 0; }
                    else { return rotation != 270; }

                default:
                    throw new System.Exception("Invalid TileType.");
            }
        }

        // Returns a TileType enum representing the provided tile in that X,Y position.
        public TileType GetTileType(Tile tile)
        {
            return GetTileType(tile.X, tile.Y);
        }

        // Returns a TileType enum representing the tile located at the provided coordinates.
        public TileType GetTileType(int tileX, int tileY)
        {
            Vector3Int tilePos = new Vector3Int(tileX, tileY, 0);
            var tileBase = map.GetTile(tilePos);

            if (tileBase == null) return TileType.dirt;
            switch (tileBase.name)
            {
                case "BendPipe":
                    return TileType.bend;
                case "CrossPipe":
                case "Cross Pipe":
                    return TileType.fourWay;
                case "ClosedDeadEnd":
                    return TileType.closedEnd;
                case "Dirt":
                    return TileType.dirt;
                case "OpenDeadEnd":
                    return TileType.openEnd;
                case "StraightPipe":
                    return TileType.straight;
                case "T Pipe":
                    return TileType.threeWay;
                case "UndergroundPipe":
                    return TileType.underGround;
                default:
                    throw new System.Exception(string.Format("Invalid tile name: {0}!", tileBase.name));
            }
        }

        // Gets the z rotation angle of a given Tile.
        private int GetRotationAngle(Tile tile)
        {
            return GetRotationAngle(tile.X, tile.Y);
        }

        // Gets the z rotation angle of the tile at X,Y.
        private int GetRotationAngle(int x, int y)
        {
            Vector3Int tilePos = new Vector3Int(x, y, 0);
            Quaternion rotation = map.GetTransformMatrix(tilePos).rotation;
            int rawRotationAngle = (int)rotation.eulerAngles.z;
            int correctedRotationAngle = CorrectRotationAngle(rawRotationAngle);

            return correctedRotationAngle;
        }

        // Limits angles to between [0, 360).
        private int CorrectRotationAngle(int rotationAngle)
        {
            return Mathf.Abs(((rotationAngle / 90) % 4) * 90);
        }

        // Gets the opposite direction from the one provided.
        private Direction GetOppositeDirection(Direction d)
        {
            switch (d)
            {
                case Direction.Top:
                    return Direction.Bottom;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Bottom:
                    return Direction.Top;
                case Direction.Right:
                    return Direction.Left;
                default:
                    Debug.LogError("ERROR: Invalid direction: " + d);
                    throw new System.Exception("Invalid direction: " + d);
            }
        }

        // Colors a tile. Fill indicates whether the background should be filled or transparent.
        private void ColorTile(Tile tile, bool fill)
        {
            ColorTile(tile.X, tile.Y, fill);
        }

        // Colors the tile at X, Y. Fill indicates whether the background should be filled or transparent.
        private void ColorTile(int x, int y, bool fill)
        {
            Vector3Int tileMousePos = new Vector3Int(x, y, 0);

            // Determine how the tile is already rotated.
            var transformMatrix = map.GetTransformMatrix(tileMousePos);
            Quaternion rotation = transformMatrix.rotation;

            int rotationAngle = GetRotationAngle(new Tile(x, y));
            //int rotationAngle = (int)rotation.eulerAngles.z;

            TileType tileType;
            var tileBase = map.GetTile(tileMousePos);
            if (tileBase == null)
            {
                tileType = TileType.dirt;
            }
            else
            {
                switch (tileBase.name)
                {
                    case "BendPipe":
                        tileType = TileType.bend;
                        break;
                    case "CrossPipe":
                    case "Cross Pipe":
                        tileType = TileType.fourWay;
                        break;
                    case "ClosedDeadEnd":
                        tileType = TileType.closedEnd;
                        break;
                    case "Dirt":
                        tileType = TileType.dirt;
                        break;
                    case "OpenDeadEnd":
                        tileType = TileType.openEnd;
                        break;
                    case "StraightPipe":
                        tileType = TileType.straight;
                        break;
                    case "T Pipe":
                        tileType = TileType.threeWay;
                        break;
                    case "UndergroundPipe":
                        tileType = TileType.underGround;
                        break;
                    default:
                        Debug.LogError("ERROR: Invalid tile name, could not get tile type: " + tileBase.name);
                        throw new System.Exception("Invalid tile name, could not get tile type: " + tileBase.name);
                }
            }

            map.SetTile(tileMousePos, GetRotatedTileBase(tileType, GetRotationAngle(x, y), fill));
            map.RefreshTile(tileMousePos);
        }

        // Fetch a TileBase from an off-screen portion of the grid to use in coloring other tiles.
        private TileBase GetRotatedTileBase(TileType tileType, int rotation, bool fill)
        {
            int x, y;
            switch (tileType)
            {
                case TileType.bend:
                case TileType.borderBend:
                    x = -7;
                    y = -11;
                    break;
                case TileType.closedEnd:
                case TileType.borderDeadEnd:
                    x = -13;
                    y = -13;
                    break;
                case TileType.dirt:
                    x = -7;
                    y = -13;
                    break;
                case TileType.fourWay:
                    x = -11;
                    y = -11;
                    break;
                case TileType.openEnd:
                    x = -13;
                    y = -11;
                    break;
                case TileType.straight:
                case TileType.borderStraight:
                    x = -9;
                    y = -11;
                    break;
                case TileType.threeWay:
                    x = -9;
                    y = -13;
                    break;
                case TileType.underGround:
                case TileType.start:
                case TileType.end:
                    x = -11;
                    y = -13;
                    break;
                default:
                    Debug.LogError("ERROR: Invalid tile type: " + tileType);
                    throw new System.Exception("Invalid tile type: " + tileType);
            }

            // Transparent tileset is 8 tiles to the right.
            if (!fill) x += 8;

            // 
            switch (rotation)
            {
                case 90:
                    y--;
                    break;
                case 180:
                    y--;
                    x++;
                    break;
                case 270:
                    x++;
                    break;
                default:
                    break;

            }

            Vector3Int tilePos = new Vector3Int(x, y, 0);
            return map.GetTile(tilePos);
        }

        public void PaintTile(int x, int y, TileType tileType, int rotation, bool highlight)
        {
            Vector3Int pos = new Vector3Int(x, y, 0);
            TileBase baseToPaint = GetRotatedTileBase(tileType, rotation, highlight);

            map.SetTile(pos, baseToPaint);
            map.SetTransformMatrix(pos, Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotation)));
            map.RefreshTile(pos);
        }

        // Function to get list of open faces for a given tile.
        private List<Direction> GetOpenTileFaces(Tile tile)
        {
            List<Direction> allSides = new List<Direction> { Direction.Left, Direction.Top, Direction.Right, Direction.Bottom };
            List<Direction> openSides = new List<Direction>();

            foreach (Direction d in allSides)
            {
                if (IsFaceOpen(tile.X, tile.Y, d))
                    openSides.Add(d);
            }

            return openSides;
        }


        // Basic Tile class for easy comparison and other utilities.
        public class Tile
        {
            public Tile(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; private set; }
            public int Y { get; private set; }

            public bool EqualsTile(Tile other)
            {
                if (other == null) return false;
                return (other.X == X && other.Y == Y);
            }

            public Tile GetAdjacentTileTop()
            {
                return new Tile(X, Y + 1);
            }

            public Tile GetAdjacentTileBottom()
            {
                return new Tile(X, Y - 1);
            }

            public Tile GetAdjacentTileLeft()
            {
                return new Tile(X - 1, Y);
            }

            public Tile GetAdjacentTileRight()
            {
                return new Tile(X + 1, Y);
            }
        }
    }
}