using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GvrCameraEdit : MonoBehaviour {

	public GameObject Cam1;
	public GameObject Cam2;
	public GameObject CamMain;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (Cam2 == null) 
		{
			Cam1 = GameObject.Find ("Main Camera Left");
			Cam2 = GameObject.Find ("Main Camera Right");
			CamMain = GameObject.Find ("Main Camera");
		}

		Debug.Log ("Left:"+Cam1.transform.position);
		Debug.Log ("Right:"+Cam2.transform.position);

		Cam1.transform.localPosition = new Vector3(-0.00018f,0.0f,0.0f);
		Cam2.transform.localPosition = new Vector3(-0.000003f,0.0f,0.0f);
	}
}
