﻿using PipeTap.Utilities;
using PipeTap.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {

    public Tilemap map;
    public AudioSource snapSound1,
                       snapSound2,
                       successSound;
    public Text levelFinishedText1,
                levelFinishedText2,
                levelFinishedText3;
    public GameObject continueButton;

    private PipeFaceCalculator calculator;

    public int startPipeX,
               startPipeY,
               endPipeX,
               endPipeY;
    public bool levelOver = false,
                fadingText = false,
                runOnceOnStart = true;
    const float period = 4.0f;
    private const bool __IS_DEBUG = true;

    // Use this for initialization
    void Start () {
        // Make the "Level Finished" text and Continue button invisible.
        Color colorOfObject1 = levelFinishedText1.color;
        colorOfObject1.a = 0;
        levelFinishedText1.color = colorOfObject1;

        Color colorOfObject2 = levelFinishedText2.color;
        colorOfObject2.a = 0;
        levelFinishedText2.color = colorOfObject2;

        Color colorOfObject3 = levelFinishedText3.color;
        colorOfObject3.a = 0;
        levelFinishedText3.color = colorOfObject3;

        continueButton.SetActive(false);


        calculator = new PipeFaceCalculator(map);

        //LevelGenerator levelGenerator = new LevelGenerator();

        //Tuple<int, int> startCoords = ConvertCoordinates(startPipeX, startPipeY, -9, -5, false);
        //Tuple<int, int> endCoords = ConvertCoordinates(endPipeX, endPipeY, -9, -5, false);

        //Tuple<TileType, int>[,] newLevel = levelGenerator.GenerateNewLevel(19, 11, startCoords.Item1, startCoords.Item2, endCoords.Item1, endCoords.Item2);
        //Tuple<TileType, int>[,] newLevel = levelGenerator.GenerateNewLevel(19, 11, ConvertCoordinates(startPipeX, startPipeY, -9, -5, false), ConvertCoordinates(endPipeX, endPipeY, -9, -5, false));

        //TODO: LEAVING OFF: NEED TO TEST AND CORRECT PAINTING!

        RepopulateTileMap();
        map.RefreshAllTiles();

        //for (int x = 0; x < newLevel.GetLength(0); x++)
        //{
        //    for (int y = 0; y < newLevel.GetLength(1); y++)
        //    {
        //        Tuple<int, int> convertedCoords = ConvertCoordinates(x, y, -9, -5, true);
        //        bool highlight = levelGenerator.IsBorderTile(x, y);
        //        calculator.PaintTile(convertedCoords.Item1, convertedCoords.Item2, newLevel[x, y].Item1, newLevel[x, y].Item2, highlight);
        //    }
        //}
    }
	
	// Update is called once per frame
	void Update () {

        // If fading text in, increase the opacity of the "Level Finished" text and make the Continue button visible.
        if (fadingText)
        {
            float prop = (Time.time / period);

            Color colorOfObject1 = levelFinishedText1.color;
            colorOfObject1.a = Mathf.Lerp(0, 1, prop);
            levelFinishedText1.color = colorOfObject1;

            Color colorOfObject2 = levelFinishedText2.color;
            colorOfObject2.a = Mathf.Lerp(0, 1, prop);
            levelFinishedText2.color = colorOfObject2;

            Color colorOfObject3 = levelFinishedText3.color;
            colorOfObject3.a = Mathf.Lerp(0, 1, prop);
            levelFinishedText3.color = colorOfObject3;

            continueButton.SetActive(true);

        }

        bool rotate = false;
        bool clockwise = false;

        // Change rotation direction based off of which button is clicked.
        if (Input.GetMouseButtonDown(0))
        {
            rotate = true;
            clockwise = true;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            rotate = true;
        }

        if (__IS_DEBUG && Input.GetKeyDown(KeyCode.A))
        {
            RepopulateTileMap();
            snapSound2.Play();
        }

        // Only rotate if the level is not over and the user has left- or right-clicked.
        if (!levelOver && rotate)
        {
            Vector3 mouseVec3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(string.Format("Co-ords of mouse is [X: {0} Y: {1} Z:{2}]", mouseVec3.x, mouseVec3.y, mouseVec3.z));

            // Use mouse coordinates to determine which tile the user clicked on.
            int adjustedX = (int)mouseVec3.x;
            int adjustedY = (int)mouseVec3.y;
            int adjustedZ = (int)mouseVec3.z;

            adjustedX = Mathf.FloorToInt(mouseVec3.x);
            adjustedY = Mathf.FloorToInt(mouseVec3.y);

            // Only continue with rotating the tile if it is not a border tile (or beyond). A tile is on the border when |tile.x| == 9 and/or |tile.y| == 5.
            if (adjustedX > -9 && adjustedX < 9 && adjustedY < 5 && adjustedY > -5)
            {
                Debug.Log(string.Format("Adjusted co-ords of mouse is [X: {0} Y: {1} Z: {2}]", adjustedX, adjustedY, adjustedZ));

                Vector3Int tileMousePos = new Vector3Int(adjustedX, adjustedY, 0);

                // Determine how the tile is already rotated.
                var transformMatrix = map.GetTransformMatrix(tileMousePos);
                Quaternion rotation = transformMatrix.rotation;

                int rotationAngle = (int)rotation.eulerAngles.z;


                // Switch rotation angle based off which button was pressed.
                if (clockwise) rotationAngle -= 90;
                else rotationAngle += 90;

                Debug.Log(string.Format("Rotation Angle: {0}", rotationAngle));

                // Rotate the tile and refresh it.
                map.SetTransformMatrix(tileMousePos, Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotationAngle)));
                map.RefreshTile(tileMousePos);

                // Play the appropriate sound.
                //if (clockwise) snapSound1.Play();
                //else snapSound2.Play();
                if (Mathf.Abs(adjustedX) % 2 == 1 ^ Mathf.Abs(adjustedY) % 2 == 1) snapSound1.Play();
                else snapSound2.Play();


                // If the pipes are properly connected, play the success sound and set booleans to indicate that the level is over and that the "Level Finished" text must be faded in.
                if (calculator.CheckIfSolutionFound(map, startPipeX, startPipeY, endPipeX, endPipeY))
                {
                    successSound.Play();
                    fadingText = true;
                    levelOver = true;
                }

                //// If the pipes are properly connected, play the success sound and set booleans to indicate that the level is over and that the "Level Finished" text must be faded in.
                //if (CheckForSolution())
                //{
                //    successSound.Play();
                //    fadingText = true;
                //    levelOver = true;
                //}
            }

            
        }
    }

    private void RepopulateTileMap()
    {
        Tuple<int, int> startCoords = ConvertCoordinates(startPipeX, startPipeY, -9, -5, false);
        Tuple<int, int> endCoords = ConvertCoordinates(endPipeX, endPipeY, -9, -5, false);

        LevelGenerator levelGenerator = new LevelGenerator();

        Tuple<TileType, int>[,] newLevel = levelGenerator.GenerateNewLevel(19, 11, startCoords.Item1, startCoords.Item2, endCoords.Item1, endCoords.Item2);

        for (int x = 0; x < newLevel.GetLength(0); x++)
        {
            for (int y = 0; y < newLevel.GetLength(1); y++)
            {
                Tuple<int, int> convertedCoords = ConvertCoordinates(x, y, -9, -5, true);
                bool highlight = levelGenerator.IsBorderTile(x, y);
                calculator.PaintTile(convertedCoords.Item1, convertedCoords.Item2, newLevel[x, y].Item1, newLevel[x, y].Item2, highlight);
            }
        }
    }

    // TODO: Move Side enum and GetOppositeSide elsewhere!
    private enum Side
    {
        Top,
        Left,
        Bottom,
        Right

    }

    private Side GetOppositeSide(Side s)
    {
        switch(s)
        {
            case Side.Top:
                return Side.Bottom;
            case Side.Left:
                return Side.Right;
            case Side.Bottom:
                return Side.Top;
            case Side.Right:
                return Side.Left;
            default:
                throw new System.Exception();
        }
    }

    private Tuple<int, int> ConvertCoordinates(Tuple<int, int> coords, int minX, int minY, bool toUnity)
    {
        return ConvertCoordinates(coords.Item1, coords.Item2, minX, minY, toUnity);
    }

    private Tuple<int, int> ConvertCoordinates(int x, int y, int minX, int minY, bool toUnity)
    {
        if (toUnity)
        {
            int newX = x - Mathf.Abs(minX);
            int newY = -(y - Mathf.Abs(minY));
            return new Tuple<int, int>(newX, newY);
            // 0 -> 5
            // 1 -> 4
            // 2 -> 3
            // 3 -> 2
            // 4 -> 1
            // 5 -> 0
            // 6 -> -1
            // 7 -> -2
            // 8 -> -3
            // 9 -> -4
            // 10 -> -5
        }
        else
        {
            int newX = x + Mathf.Abs(minX);
            int newY = -(y - Mathf.Abs(minY));
            return new Tuple<int, int>(newX, newY);
            //  5 -> 0 
            //  4 -> 1
            //  3 -> 2
            //  2 -> 3
            //  1 -> 4
            //  0 -> 5
            // -1 -> 6
            // -2 -> 7
            // -3 -> 8
            // -4 -> 9
            // -5 -> 10
        }
    }

    private int GetRotationAngle(int x, int y)
    {
        Vector3Int tilePos = new Vector3Int(x, y, 0);
        Quaternion rotation = map.GetTransformMatrix(tilePos).rotation;
        int rotationAngle = (int)rotation.eulerAngles.z;
        return rotationAngle;
    }

    // TODO: Either remove this method or use it in place of the (slightly) clunkier GetRotationAngle if statements.
    private bool CheckAngleSolved(int x, int y, int angleSolution)
    {
        return GetRotationAngle(x, y) == angleSolution;
    }

    //// Unity does not have Tuples, so here's an implementation for me to use.
    //private class Tuple<T1, T2>
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