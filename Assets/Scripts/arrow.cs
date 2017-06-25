using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrow : MonoBehaviour
{
    public player player;
    double pos;
    float position;
    // Use this for initialization
    void Start()
    {
        pos = 0;
    }

    // Update is called once per frame
    void Update()
    {

        
        if (player.state != 0 && player.state != 3)
        {
            if (player.state == 1)
            {
                pos = player.transform.position.y * (.0157 / 3002);
                position = (float)pos;

                gameObject.transform.Translate(new Vector3(0, -1 * position, 0));
            }

            else if (player.state == 2)
            {
                pos = player.transform.position.y * (.003 / 3002);
                position = (float)pos;
                gameObject.transform.Translate(new Vector3(0, -1 * position, 0));
            }
        }
    }

}
