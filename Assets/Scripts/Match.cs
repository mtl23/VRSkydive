using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match : MonoBehaviour {


   public int score = 0;
   public int round = 1;

   public bool target = false;
   public bool target2 = false;
   public bool target3 = false;
   public bool target4 = false;
   public bool target5 = false;

   public GameObject t;
   public GameObject t2;
   public GameObject t3;
   public GameObject t4;
   public GameObject t5;


 

	// Use this for initialization
	void Start () {


	}
	
	// Update is called once per frame
	void Update () {

        if (target == true)
        {
            t.SetActive(false);

        }

        if (target2 == true)
        {
            t2.SetActive(false);

        }

        if (target3 == true)
        {
            t3.SetActive(false);

        }

        if (target4 == true)
        {
            t4.SetActive(false);

        }

        if (target5 == true)
        {
            t5.SetActive(false);

        }

	}
}
