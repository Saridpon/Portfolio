using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fruitcollect : MonoBehaviour
{
    int totalFruit = 0;
    public fruitcounter fruitCounter;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D other){
    if (other.gameObject.tag == "Fruit" || other.gameObject.tag == "FruitMagic" )
        {
            totalFruit += 1;
            fruitCounter.showFruit(totalFruit);
            Destroy(other.gameObject);
        }
    }
}
