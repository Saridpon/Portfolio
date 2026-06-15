using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float startPosx,startPosy, length;
    public GameObject cam;
    [SerializeField] public float parallaxEffectx;
    [SerializeField] public float parallaxEffecty;
    // Start is called before the first frame update
    void Start()
    {
        startPosx = transform.position.x;
        startPosy = transform.position.y;
        //length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float distancex = cam.transform.position.x * parallaxEffectx;
        float movementx = cam.transform.position.x * (1 - parallaxEffectx);
        float distancey = cam.transform.position.y * parallaxEffecty;
        float movementy = cam.transform.position.y * (1 - parallaxEffecty);

        transform.position = new Vector3(startPosx + distancex, startPosy + distancey, transform.position.z);
        /*if (movementx > startPosx + length)
        {
            startPosx += length;
        }
        else if (movementx < startPosx - length)
        {
            startPosx -= length;
         }*/
    }
}
