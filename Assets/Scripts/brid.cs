using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class brid : MonoBehaviour {

    public Vector3 fly;
	// Use this for initialization
	void Start () {
        fly = new Vector3(1, 0, 1);
	}
	
	// Update is called once per frame
	void Update () {

        gameObject.transform.Translate(fly);
    }
}
