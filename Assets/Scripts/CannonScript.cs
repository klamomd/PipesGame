using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CannonScript : MonoBehaviour {

    //public ITilemap world;
    //public Tilemap extWorld;
    public Tilemap map;
    public AudioSource snapSound1;
    public AudioSource snapSound2;

    // Use this for initialization
    void Start () {
        //world = extWorld as ITilemap;
        //Grid grid = new Grid();
        //grid.
	}
	
	// Update is called once per frame
	void Update () {

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



        if (rotate)
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

            //if (mouseVec3.y < 0)
            //{
            //    adjustedY = Mathf.FloorToInt(mouseVec3.y);
            //}
            //else
            //{
            //    adjustedY = Mathf.CeilToInt(mouseVec3.y);
            //}

            int adjustedX = (int)mouseVec3.x;
            int adjustedY = (int)mouseVec3.y;
            int adjustedZ = (int)mouseVec3.z;

            adjustedX = Mathf.FloorToInt(mouseVec3.x);
            adjustedY = Mathf.FloorToInt(mouseVec3.y);


            //if (adjustedX < 1) adjustedX--;
            //else adjustedX++;
            //adjustedX--;

            //if (adjustedY < 1) adjustedY--;
            //else adjustedY++;

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
        }
    }
}
