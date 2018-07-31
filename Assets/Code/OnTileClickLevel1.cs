using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using PipeTap.Enums;
using PipeTap.Utilities;

public class OnTileClickLevel1 : MonoBehaviour {

    public Tilemap map;
    public AudioSource snapSound1,
                       snapSound2,
                       successSound;
    public Text levelFinishedText1,
                levelFinishedText2,
                levelFinishedText3;
    public GameObject continueButton;

    private PipeFaceCalculator calculator = new PipeFaceCalculator();

    public int startPipeX,
               startPipeY,
               endPipeX,
               endPipeY;
    public bool levelOver = false,
                fadingText = false;
    const float period = 4.0f;




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
                if (clockwise) snapSound1.Play();
                else snapSound2.Play();


                /**
                 * DEBUG::
                 * Testing getting tile types
                 * 
                 ****/
                TileType tileType;
                var tileBase = map.GetTile(tileMousePos);
                switch (tileBase.name)
                {
                    case "BendPipe":
                        tileType = TileType.bend;
                        break;
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
                        throw new System.Exception();
                }

                ////TODO: ADD A "COLOR TILE" FUNCTION TO BE CALLED BY CALCULATOR
                ////TODO: ADD AN "UNCOLOR TILE" FUNCTION TO BE CALLED BY CALCULATOR

                //// ATTEMPTING TO CHANGE ICON - WORKS
                //map.SetTile(tileMousePos, GetRotatedTileBase(tileType, GetRotationAngle(adjustedX, adjustedY)));
                //map.RefreshTile(tileMousePos);
                //Debug.Log(string.Format("Tile type: {0}", tileBase.name));

                

                // If the pipes are properly connected, play the success sound and set booleans to indicate that the level is over and that the "Level Finished" text must be faded in.
                if (calculator.CheckIfSolutionFound(map, -9, 1, 9, -2))
                {
                    successSound.Play();
                    fadingText = true;
                    levelOver = true;
                }

                //if (CheckForSolution())
                //{
                //    successSound.Play();
                //    fadingText = true;
                //    levelOver = true;
                //}
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


    //TODO: REMOVE HARD-CODED SOLUTIONS, IMPLEMENT DYNAMIC SOLUTION CHECKING
    private bool CheckForSolution()
    {
        if (GetRotationAngle(-5, 2) != 180) return false;
        if (GetRotationAngle(-4, 2) != 180 && GetRotationAngle(-4, 2) != 0) return false;
        if (GetRotationAngle(-3, 2) != 90) return false;
        if (GetRotationAngle(-2, 2) != 180 && GetRotationAngle(-2, 2) != 0) return false;
        if (GetRotationAngle(-1, 2) != 90) return false;

        if (GetRotationAngle(-8, 1) != 180 && GetRotationAngle(-8, 1) != 0) return false;
        if (GetRotationAngle(-7, 1) != 180 && GetRotationAngle(-7, 1) != 0) return false;
        if (GetRotationAngle(-6, 1) != 180 && GetRotationAngle(-6, 1) != 0) return false;
        if (GetRotationAngle(-5, 1) != 270) return false;
        if (GetRotationAngle(-4, 1) != 180 && GetRotationAngle(-4, 1) != 0) return false;
        //if (GetRotationAngle(-3, 1) != 180) return false;
        if (GetRotationAngle(-2, 1) != 180 && GetRotationAngle(-2, 1) != 0) return false;
        //if (GetRotationAngle(-1, 1) != 180) return false;
        if (GetRotationAngle(0, 1) != 90) return false;
        if (GetRotationAngle(1, 1) != 180 && GetRotationAngle(1, 1) != 0) return false;
        if (GetRotationAngle(2, 1) != 90) return false;

        if (GetRotationAngle(-3, 0) != 270) return false;
        if (GetRotationAngle(-2, 0) != 180 && GetRotationAngle(-2, 0) != 0) return false;
        if (GetRotationAngle(-1, 0) != 270) return false;
        if (GetRotationAngle(0, 0) != 0) return false;
        if (GetRotationAngle(2, 0) != 270) return false;
        if (GetRotationAngle(3, 0) != 90) return false;

        if (GetRotationAngle(3, -1) != 270) return false;
        if (GetRotationAngle(4, -1) != 90) return false;

        if (GetRotationAngle(4, -2) != 90 && GetRotationAngle(4, -2) != 270) return false;
        if (GetRotationAngle(5, -2) != 180) return false;
        if (GetRotationAngle(6, -2) != 180 && GetRotationAngle(6, -2) != 0) return false;
        if (GetRotationAngle(7, -2) != 180 && GetRotationAngle(7, -2) != 0) return false;
        if (GetRotationAngle(8, -2) != 180 && GetRotationAngle(8, -2) != 0) return false;

        if (GetRotationAngle(4, -3) != 270) return false;
        if (GetRotationAngle(5, -3) != 0) return false;

        return true;
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

    private TileBase GetRotatedTileBase(TileType tileType, int rotation)
    {
        int x, y;
        switch (tileType)
        {
            case TileType.bend:
                x = -6;
                y = -8;
                break;
            case TileType.closedEnd:
                x = -9;
                y = -9;
                break;
            case TileType.dirt:
                x = -6;
                y = -9;
                break;
            case TileType.fourWay:
                x = -8;
                y = -8;
                break;
            case TileType.openEnd:
                x = -9;
                y = -8;
                break;
            case TileType.straight:
                x = -7;
                y = -8;
                break;
            case TileType.threeWay:
                x = -7;
                y = -9;
                break;
            case TileType.underGround:
                x = -6;
                y = -9;
                break;
            default: throw new System.Exception();
        }

        // Rotate the tile and refresh it.
        Vector3Int tilePos = new Vector3Int(x, y, 0);
        map.SetTransformMatrix(tilePos, Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotation)));
        map.RefreshTile(tilePos);
        

        TileBase rotatedTileBase = map.GetTile(tilePos);

        // Rotate the tile back and refresh it.
        map.SetTransformMatrix(tilePos, Matrix4x4.Rotate(Quaternion.Euler(0, 0, -rotation)));
        map.RefreshTile(tilePos);

        return rotatedTileBase;
    }
}
