using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class OnTileClickMainMenu : MonoBehaviour {

    public Tilemap map;
    public AudioSource snapSound1;
    public AudioSource snapSound2;
    public Button playButton;

    //private enum TileType
    //{
    //    threeWay,
    //    fourWay,
    //    straight,
    //    bend,
    //    closedEnd,
    //    openEnd,
    //    underGround,
    //    dirt
    //}

    // Use this for initialization
    void Start () {
        playButton.enabled = true;
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
                //float ex = rotation.eulerAngles.x,
                //      ey = rotation.eulerAngles.y,
                //      ez = rotation.eulerAngles.z;
                int rotationAngle = (int)rotation.eulerAngles.z;


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
}
