 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class OnTileClickMainMenu : MonoBehaviour {

    public Tilemap map;
    public AudioSource snapSound1,
                       snapSound2;
    public Button playButton;
    public Canvas quitCanvas,
                  creditsCanvas,
                  helpCanvas;

    private bool showingUI = false;

    // Use this for initialization
    void Start () {
        playButton.enabled = true;
        quitCanvas.enabled = false;
        creditsCanvas.enabled = false;
        helpCanvas.enabled = false;
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

        // If escape/back button is pressed, show the quit UI (if the level is not done).
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (showingUI)
            {
                showingUI = false;
                quitCanvas.enabled = false;
                helpCanvas.enabled = false;
                creditsCanvas.enabled = false;
            }
            else
            {
                showingUI = true;
                quitCanvas.enabled = true;
            }
        }


        if (rotate && !showingUI)
        {
            Vector3 mouseVec3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log(string.Format("Co-ords of mouse is [X: {0} Y: {1} Z:{2}]", mouseVec3.x, mouseVec3.y, mouseVec3.z));

            //int adjustedX = (int)mouseVec3.x;
            //int adjustedY = (int)mouseVec3.y;

            int adjustedX = Mathf.FloorToInt(mouseVec3.x);
            int adjustedY = Mathf.FloorToInt(mouseVec3.y);

            // Skip border tiles
            if (adjustedX > -9 && adjustedX < 9 && adjustedY < 5 && adjustedY > -5)
            {
                //Debug.Log(string.Format("Adjusted co-ords of mouse is [X: {0} Y: {1} Z: {2}]", adjustedX, adjustedY, adjustedZ));

                Vector3Int tileMousePos = new Vector3Int(adjustedX, adjustedY, 0);

                // Determine how the tile is already rotated.
                var transformMatrix = map.GetTransformMatrix(tileMousePos);
                Quaternion rotation = transformMatrix.rotation;

                int rotationAngle = (int)rotation.eulerAngles.z;


                // Switch rotation angle based off which button was pressed.
                if (clockwise) rotationAngle -= 90;
                else rotationAngle += 90;


                // Rotate the tile and refresh it.
                map.SetTransformMatrix(tileMousePos, Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotationAngle)));
                map.RefreshTile(tileMousePos);

                // Play the appropriate sound.
                if (clockwise) snapSound1.Play();
                else snapSound2.Play();
            }
        }
    }

    public void CloseQuitUI()
    {
        quitCanvas.enabled = false;
        showingUI = false;
        snapSound1.Play();
    }

    public void ShowCreditsUI(bool visible)
    {
        // Don't allow click when some UI already visible.
        if (visible && showingUI) return;

        showingUI = visible;
        creditsCanvas.enabled = visible;
        if (!visible) snapSound1.Play();
    }

    public void ShowHelpUI(bool visible)
    {
        // Don't allow click when some UI already visible.
        if (visible && showingUI) return;

        showingUI = visible;
        helpCanvas.enabled = visible;
        if (!visible) snapSound1.Play();
    }

    public void PlayButtonClicked()
    {
        if (showingUI) return;
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
