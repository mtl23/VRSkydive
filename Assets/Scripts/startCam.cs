using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class startCam : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.eulerAngles= new Vector3 (90, 0, 0);
	}
}
