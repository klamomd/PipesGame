using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayButtonAdjust : MonoBehaviour {

    public GameObject playButton;

	// Use this for initialization
	void Start () {
        // set the desired aspect ratio (the values in this example are
        // hard-coded for 16:9, but you could make them into public
        // variables instead so you can set them at design time)
        float targetaspect = 16.0f / 9.0f;

        // determine the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;

        // current viewport height should be scaled by this amount
        float scaleheight = windowaspect / targetaspect;

        var newTransform = playButton.transform.position;
        newTransform.y = newTransform.y * scaleheight;
        playButton.transform.position = newTransform;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
