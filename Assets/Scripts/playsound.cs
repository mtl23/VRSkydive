using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playsound : MonoBehaviour {
    public AudioClip sfx;
    public AudioSource sfxPlayer;
	// Use this for initialization
	void Start () {
        sfxPlayer.clip = sfx;
	}

    void OnCollisionEnter(Collision col)
    {
        sfxPlayer.PlayOneShot(sfx);
    
    }

	// Update is called once per frame
	void Update () {
		
	}
}
