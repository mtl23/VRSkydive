using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class helicopter : MonoBehaviour {
    public player player;
    public AudioClip chopper;
    public AudioSource SFX;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (player.state == 0 || player.state == 3)
        {
            SFX.volume = .75f;
            SFX.clip = chopper;
        }

        if (player.state == 1 || player.state == 2)
        {
            SFX.volume = 0;
            
        }
	}
}
