using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour {

    public GameObject controller;
	// Use this for initialization
	void Start () {
      //  DontDestroyOnLoad(controller);
     //  Application.LoadLevel(1);
	}
	
	// Update is called once per frame
	void Update () {
	
		if ((Input.GetKeyUp (KeyCode.Space) || Input.GetMouseButtonDown (0))) {

			startgame ();
		}
	}

	public void startgame(){
	
		DontDestroyOnLoad(controller);
		Application.LoadLevel(1);
	}
}
