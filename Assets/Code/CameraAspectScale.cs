using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAspectScale : MonoBehaviour {

    // Use this for initialization
    void Start () {
        float totalCellsWidth = 19.0f;
        Camera.main.orthographicSize = (totalCellsWidth / Screen.width * Screen.height / 2.0f);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
