using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class player : MonoBehaviour
{
    bool sound_played;
    public int state;
    public float velocity;
    public float x_velocity;
    public float z_velocity;
    public float damping;
    public Camera mainCam;
    public Text altitude;
    public float rotation;
    public bool hasJumped;
    public float rotspd;
    public Quaternion startRot;
    public Quaternion fallingRot;
    public Vector3 oldRot;
    public  float timer = 3.0f;
    public  GameObject parachute;
    public GameObject menu;
    public GameObject match;
    public AudioClip wind1;
    public AudioClip wind2;
	public AudioClip voice2;
	public AudioClip voice3;
	public AudioClip voice4;
	public AudioClip voice5;
	public AudioSource captain;
    public AudioSource SFX;
    // Use this for initialization
    void Start()
    {
        sound_played = false;
        match = GameObject.Find("MatchController");
        menu.SetActive(false);
        parachute.SetActive(false);
        startRot = new Quaternion(-30, 0, 0,0);
        fallingRot = new Quaternion(30,0,0,0);
        hasJumped = false;
        mainCam.enabled = true;
        state = 0;
        gameObject.GetComponent<Rigidbody>().useGravity = false;
        damping = .01f;

       match.GetComponent<Match>().t  = GameObject.Find("target");
       match.GetComponent<Match>().t2 = GameObject.Find("target2");
       match.GetComponent<Match>().t3 = GameObject.Find("target3");
       match.GetComponent<Match>().t4 = GameObject.Find("target4");
       match.GetComponent<Match>().t5 = GameObject.Find("target5");

    }

    void Jump()
    {
       
      

        if (!hasJumped)
        {
            hasJumped = true;
        }
        
             
            return;
    
    }

    public void UpPlayerState()
    {
        if(state < 2)
        state += 1;
    
    }
    public void TurnOff()
    {

        x_velocity = 0;
        z_velocity = 0;
     
        
    }
    public void TurnLeft()
    {
     //   Debug.Log("Left arrow");
        if (state == 1 || state == 2)
        {
        //    transform.Rotate(new Vector3(0, -20, 0));

            if (x_velocity >= 30)
            {
                x_velocity = 30;
            }
            else if (x_velocity < 30)
            {
                x_velocity += 60;
            }
       
        }
        
    }
    public void TurnRight()
    {
     //   Debug.Log("Right arrow");

        if (state == 1 || state == 2)
            {

         //       transform.Rotate(0, 20, 0);
                if (x_velocity <= -30)
                {
                    x_velocity = -30;
                }
                else if (x_velocity > -30)
                {
                    x_velocity -= 60;
                }
              
            }
   
    }

    public void TurnUp()
    {
        if (state == 1 || state == 2)
        { 
    
        //    Debug.Log("Up arrow");

            if (z_velocity < 30)
            {
                z_velocity += 60;
            }
            else if (z_velocity >= 30)
            {
                z_velocity = 30;
            }

            gameObject.GetComponent<Rigidbody>().velocity = new Vector3(x_velocity, velocity, z_velocity);

      
        }
    }

    public void TurnDown()
    {
        if (state == 1 || state == 2)
        {
           


           // Debug.Log("Down arrow");
            if (z_velocity > -30)
            {
                z_velocity -= 60;
            }

            if (z_velocity <= -30)
            {
                z_velocity = -30;
            }
            gameObject.GetComponent<Rigidbody>().velocity = new Vector3(x_velocity, velocity, z_velocity);
         
        }
    }


    void OnCollisionEnter(Collision col)
    {
        state = 3;
        timer = .9f;
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll ;
       // gameObject.transform.eulerAngles= new Vector3 (0,0,0);
        gameObject.GetComponent<AudioSource>().volume = .50f;
        
        if(col.gameObject.name=="target"){
           
			captain.clip = voice3;
			captain.Play();
            match.GetComponent<Match>().target = true;
            float dist = Vector3.Distance(col.transform.position, transform.position);
            Debug.Log("Distance to other: " + dist);

            if (dist <= 48)
            {
                match.GetComponent<Match>().score += 15;

            }

            if (dist <= 55)
            {
                match.GetComponent<Match>().score += 10;

            }

            if (dist >= 55.000001f)
            {
                match.GetComponent<Match>().score += 5;

            }

        }

        if (col.gameObject.name == "target2")
        {
			captain.clip = voice3;
			captain.Play();
            match.GetComponent<Match>().target2 = true;
            float dist = Vector3.Distance(col.transform.position, transform.position);
            Debug.Log("Distance to other: " + dist);

            if (dist <= 48)
            {
                match.GetComponent<Match>().score += 15;

            }

            if (dist <= 55)
            {
                match.GetComponent<Match>().score += 10;

            }

            if (dist >= 46)
            {
                match.GetComponent<Match>().score += 5;

            }

        }

        if (col.gameObject.name == "target3")
        {
			captain.clip = voice3;
			captain.Play();
            match.GetComponent<Match>().target3 = true;
            float dist = Vector3.Distance(col.transform.position, transform.position);
            Debug.Log("Distance to other: " + dist);

            if (dist <= 48)
            {
                match.GetComponent<Match>().score += 15;

            }

            if (dist <= 55)
            {
                match.GetComponent<Match>().score += 10;

            }

            if (dist >= 46)
            {
                match.GetComponent<Match>().score += 5;

            }

        }

        if (col.gameObject.name == "target4")
        {
			captain.clip = voice3;
			captain.Play();
            match.GetComponent<Match>().target4 = true;
            float dist = Vector3.Distance(col.transform.position, transform.position);
            Debug.Log("Distance to other: " + dist);

            if (dist <= 48)
            {
                match.GetComponent<Match>().score += 15;

            }

            if (dist <= 55)
            {
                match.GetComponent<Match>().score += 10;

            }

            if (dist >= 46)
            {
                match.GetComponent<Match>().score += 5;

            }

        }

        if (col.gameObject.name == "target5")
        {
			captain.clip = voice3;
			captain.Play();
            match.GetComponent<Match>().target5 = true;
            float dist = Vector3.Distance(col.transform.position, transform.position);
            Debug.Log("Distance to other: " + dist);

            if (dist <= 48)
            {
                match.GetComponent<Match>().score += 15;
            
            }

            if (dist <= 55)
            {
                match.GetComponent<Match>().score += 10;

            }

            if (dist >= 46)
            {
                match.GetComponent<Match>().score += 5;

            }
        }

		if (col.gameObject.name == "Terrain") {
			captain.clip = voice4;
			captain.Play();

		}
        
    }
    // Update is called once per frame
    void Update()
    {

 
                if (state == 2)
        { 
        
        SFX.clip = wind2;
        }
               
        if (state == 3)
           {

          SFX.volume =0;
          }
     
      
      //  Debug.Log(gameObject.GetComponent<Rigidbody>().velocity);
        if (gameObject.GetComponent<Rigidbody>().velocity.y <= -54)
        {
            gameObject.GetComponent<Rigidbody>().velocity = new Vector3(x_velocity, -54, 0); //terminal velocity
            velocity = gameObject.GetComponent<Rigidbody>().velocity.y;
            x_velocity = gameObject.GetComponent<Rigidbody>().velocity.x;
            //  Debug.Log("reached terminal velocity");
        }

        if ((Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonDown(0)) && state <2)
        {
          state += 1;
			if (state == 1) {
				captain.clip = voice2;
				captain.Play();
			}
          if (state == 2)
          {
              parachute.SetActive(true);
				captain.clip = voice5;
				captain.Play();
              timer = 1.4f;
              velocity += 70;
              if (velocity > -16)
              {
                  velocity = -16;
              }
          }
        }
        switch (state)
        {
            case (0):
                gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                gameObject.GetComponent<Rigidbody>().useGravity = false;
            //    Debug.Log("press Space to initiate jump");
                break;

            case (1):
            //gameObject.GetComponent<Rigidbody>().useGravity  = true;
                if (velocity > -54)
                {
                    velocity -= 13f;
                }
                if (velocity <= -54)
                {
                    velocity = -54;
                }

                Jump();
                gameObject.GetComponent<Rigidbody>().velocity = new Vector3(x_velocity, velocity, z_velocity);
                if (timer > 0)
                {
                    timer -= 1 * Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(transform.rotation, fallingRot, damping * Time.deltaTime);
                }
                    // Debug.Log("Press Space to initiate the parachute");
                break;

            case (2):
               // Debug.Log("set the parachute!");
              //  velocity += 1f;
              
                
                if (velocity > -20)
                {
                    velocity -= .0010f;
                }
                if (velocity <= -20)
                {
                    velocity = -20;
                }
                gameObject.GetComponent<Rigidbody>().velocity = new Vector3(x_velocity, velocity, z_velocity);
                if (timer > 0)
                {
                    timer -= 1 * Time.deltaTime;
                    if(transform.rotation.x >0)
                    gameObject.transform.Rotate(-0.4f, 0, 0, 0);
                }
                break;

            case (3):
                
                if (timer > 0)
                {
                    timer -= 1 * Time.deltaTime;
                    gameObject.transform.Rotate(-2.1f, 0, 0, 0);
                    //transform.rotation = Quaternion.Slerp(fallingRot,startRot, damping * Time.deltaTime);            
                }
                if (timer <= 0)
                {
                    Debug.Log("Show the menu");
                    menu.SetActive(true);
                }
             
             break;
        }

    }
}
