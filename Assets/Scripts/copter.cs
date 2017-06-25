using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class copter : MonoBehaviour {

    public Vector3 rotate;
	// Use this for initialization
	void Start () {
        rotate = new Vector3(0, 0, 20);
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(rotate);
	}
}
