using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public GameObject PanelQ;
    public GameObject PanelN;
    public GameObject PanelR;
    public GameObject controller;
	// Use this for initialization
	void Start () {
        controller = GameObject.Find("MatchController");
	}
	
	// Update is called once per frame
	void Update () {
        if (controller.GetComponent<Match>().round == 5) 
        {
            PanelN.SetActive(false);
            //PanelQ.transform.position = new Vector3(0,7f,1);
        }
	}

    public void Restart()
    {
     DontDestroyOnLoad(controller);
     Application.LoadLevel(1);

     if (controller.GetComponent<Match>().round == 5)
     {
         controller.GetComponent<Match>().round = 1;
         controller.GetComponent<Match>().score = 0;
         Application.LoadLevel(0);
     
     }
     
    }

    public void nextjump()
    {
        if (controller.GetComponent<Match>().round<=4)
        {
        controller.GetComponent<Match>().round++;
        DontDestroyOnLoad(controller);
        Application.LoadLevel(1);
        }
    }

   public void Quit()
    {
    Application.Quit();
    }
}
