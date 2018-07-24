using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class OnTileClickLevel2 : MonoBehaviour {

    //public ITilemap world;
    //public Tilemap extWorld;
    public Tilemap map;
    public AudioSource snapSound1;
    public AudioSource snapSound2;
    public AudioSource successSound;
    public Text levelFinishedText1,
                levelFinishedText2,
                levelFinishedText3;
    public GameObject continueButton;

    public int startPipeX,
               startPipeY,
               endPipeX,
               endPipeY;
    public bool levelOver = false;
    public bool playedSuccess = false;
    public bool fadingText = false;
    const float period = 4.0f;

    private enum TileType
    {
        threeWay,
        fourWay,
        straight,
        bend,
        closedEnd,
        openEnd,
        underGround,
        dirt
    }

    // Use this for initialization
    void Start () {
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



        if (!levelOver && rotate)
        {
            Vector3 mouseVec3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(string.Format("Co-ords of mouse is [X: {0} Y: {1} Z:{2}]", mouseVec3.x, mouseVec3.y, mouseVec3.z));
            //mouseVec3.z = 0;




            /**
             * 
             *  BIG TODO:
             *  - Adjustment is OFF
             *  LEAVING OFF:
             *  - Can find actual tile with (floor + ceil) / 2 BUT!!:
             *  - VECTOR MUST BE IN INT! How to use that half point?
             * 
             * 
             ****/


            //// Adjust X and Y for scale of tiles (128 px = 1.28 scale)
            //int adjustedX = (int)(mouseVec3.x / 1.28) - 1;
            //int adjustedY = (int)(mouseVec3.y / 1.28);

            //int adjustedX = 0;
            //int adjustedY = 0;

            //if (mouseVec3.x < 0)
            //{
            //    adjustedX = Mathf.FloorToInt(mouseVec3.x);
            //}
            //else
            //{
            //    adjustedX = Mathf.CeilToInt(mouseVec3.x);
            //}

            int adjustedX = (int)mouseVec3.x;
            int adjustedY = (int)mouseVec3.y;
            int adjustedZ = (int)mouseVec3.z;

            adjustedX = Mathf.FloorToInt(mouseVec3.x);
            adjustedY = Mathf.FloorToInt(mouseVec3.y);

            // Skip border tiles
            if (adjustedX > -9 && adjustedX < 9 && adjustedY < 5 && adjustedY > -5)
            {
                Debug.Log(string.Format("Adjusted co-ords of mouse is [X: {0} Y: {1} Z: {2}]", adjustedX, adjustedY, adjustedZ));

                Vector3Int tileMousePos = new Vector3Int(adjustedX, adjustedY, 0);

                // Determine how the tile is already rotated.
                var transformMatrix = map.GetTransformMatrix(tileMousePos);
                Quaternion rotation = transformMatrix.rotation;

                // TODO: Remove useless angles?
                float ex = rotation.eulerAngles.x,
                      ey = rotation.eulerAngles.y,
                      ez = rotation.eulerAngles.z;
                int rotationAngle = (int)ez;


                // Adjust rotation angle based off of mouse-button.
                if (clockwise)
                {
                    rotationAngle -= 90;
                }
                else
                {
                    rotationAngle += 90;
                }

                Debug.Log(string.Format("Rotation Angle: {0}", rotationAngle));

                // Rotate the tile and refresh it.
                map.SetTransformMatrix(tileMousePos, Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotationAngle)));
                map.RefreshTile(tileMousePos);

                if (clockwise)
                {
                    snapSound1.Play();
                }
                else
                {
                    snapSound2.Play();
                }

                /**
                 * DEBUG::
                 * Testing getting tile types
                 * 
                 ****/
                var tileBase = map.GetTile(tileMousePos);

                Debug.Log(string.Format("Tile type: {0}", tileBase.name));

                switch(tileBase.name)
                {
                    case "BendPipe":
                        break;
                    case "CrossPipe":
                        break;
                    case "ClosedDeadEnd":
                        break;
                    case "Dirt":
                        break;
                    case "OpenDeadEnd":
                        break;
                    case "StraightPipe":
                        break;
                    case "T Pipe":
                        break;
                    case "UndergroundPipe":
                        break;
                    default:
                        break;
                }

                if (CheckForSolution())
                //if (!playedSuccess && CheckForSolution())
                {
                    playedSuccess = true;
                    successSound.Play();
                    fadingText = true;
                    levelOver = true;
                }
            }

            
        }
    }

    //private TileType GetTileType(Vector3Int tilePos)
    //{
    //    //if (map.GetTile(tilePos).GetTileData(to).GetComponent<WallTile>() != null;)
    //}

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

    private bool CheckForSolution()
    //private bool CheckForSolution(Side parentSide, int x, int y)
    {
        if (GetRotationAngle(-8, 2) != 180) return false;
        if (GetRotationAngle(-7, 2) != 90) return false;
        if (GetRotationAngle(-6, 2) != 90) return false;
        if (GetRotationAngle(4, 2) != 90) return false;
        if (GetRotationAngle(5, 2) != 180) return false;
        if (GetRotationAngle(6, 2) != 90) return false;

        //if (GetRotationAngle(-8, 1) != 180) return false;
        if (GetRotationAngle(-7, 1) != 0) return false;
        if (GetRotationAngle(-6, 1) != 90 && GetRotationAngle(-6, 1) != 270) return false;
        if (GetRotationAngle(4, 1) != 180) return false;
        if (GetRotationAngle(5, 1) != 0) return false;
        if (GetRotationAngle(6, 1) != 90 && GetRotationAngle(6, 1) != 270) return false;

        if (GetRotationAngle(-8, 0) != 270) return false;
        if (GetRotationAngle(-7, 0) != 90) return false;
        if (GetRotationAngle(-6, 0) != 270) return false;
        if (GetRotationAngle(-5, 0) != 90) return false;
        if (GetRotationAngle(-4, 0) != 180 && GetRotationAngle(-4, 0) != 0) return false;
        if (GetRotationAngle(-3, 0) != 180 && GetRotationAngle(-3, 0) != 0) return false;
        if (GetRotationAngle(-2, 0) != 90) return false;
        if (GetRotationAngle(4, 0) != 90 && GetRotationAngle(4, 0) != 270) return false;
        if (GetRotationAngle(6, 0) != 90 && GetRotationAngle(6, 0) != 270) return false;

        if (GetRotationAngle(-7, -1) != 270) return false;
        if (GetRotationAngle(-6, -1) != 90) return false;
        if (GetRotationAngle(-5, -1) != 270) return false;
        if (GetRotationAngle(-4, -1) != 90) return false;
        if (GetRotationAngle(-3, -1) != 90) return false;
        if (GetRotationAngle(-2, -1) != 270) return false;
        if (GetRotationAngle(-1, -1) != 180 && GetRotationAngle(-1, -1) != 0) return false;
        if (GetRotationAngle(0, -1) != 90) return false;
        if (GetRotationAngle(1, -1) != 180) return false;
        if (GetRotationAngle(2, -1) != 90) return false;
        if (GetRotationAngle(3, -1) != 180) return false;
        if (GetRotationAngle(4, -1) != 0) return false;
        if (GetRotationAngle(6, -1) != 270) return false;
        if (GetRotationAngle(7, -1) != 90) return false;


        if (GetRotationAngle(-6, -2) != 90 && GetRotationAngle(-6, -2) != 270) return false;
        if (GetRotationAngle(-5, -2) != 180) return false;
        if (GetRotationAngle(-4, -2) != 270) return false;
        if (GetRotationAngle(-3, -2) != 270) return false;
        if (GetRotationAngle(-2, -2) != 180 && GetRotationAngle(-2, -2) != 0) return false;
        if (GetRotationAngle(-1, -2) != 0) return false;
        if (GetRotationAngle(0, -2) != 270) return false;
        if (GetRotationAngle(1, -2) != 270) return false;
        if (GetRotationAngle(2, -2) != 270) return false;
        if (GetRotationAngle(3, -2) != 0) return false;
        if (GetRotationAngle(6, -2) != 180) return false;
        //if (GetRotationAngle(7, -2) != 90) return false;
        if (GetRotationAngle(8, -2) != 90) return false;

        if (GetRotationAngle(-6, -3) != 270) return false;
        if (GetRotationAngle(-5, -3) != 0) return false;
        if (GetRotationAngle(6, -3) != 270) return false;
        if (GetRotationAngle(7, -3) != 270) return false;
        if (GetRotationAngle(8, -3) != 0) return false;

        return true;
    }

    private int GetRotationAngle(int x, int y)
    {
        Vector3Int tilePos = new Vector3Int(x, y, 0);
        Quaternion rotation = map.GetTransformMatrix(tilePos).rotation;
        int rotationAngle = (int)rotation.eulerAngles.z;
        return rotationAngle;
    }

    private bool CheckAngleSolved(int x, int y, int angleSolution)
    {
        return GetRotationAngle(x, y) == angleSolution;
    }
}
