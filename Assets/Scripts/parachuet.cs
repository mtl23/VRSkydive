using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parachuet : MonoBehaviour {
	public GameObject chute;
	public float timer;
	public Transform guy;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {


		if (timer > 0) {
		
			chute.SetActive(false);
			timer -= 1 * Time.deltaTime;
		}
		else if(timer<=0)
		{
			chute.SetActive(true);

		}

		if(guy.position.y <500)
		{
			guy.position = new Vector3(157,3200,2325);
			timer = 6;
		}
	
	}


}
