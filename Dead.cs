using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dead : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject respawnPoint;
    public float respawnTime = 1f;
    private SpriteRenderer m_spriteRenderer;
    public Rigidbody2D rb;
    Vector2 checkPoint;
    
    public screenFlash flashingScreen;
    // Start is called before the first frame update
    void Start()
    {
        checkPoint = respawnPoint.transform.position;
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collistion){
        if (collistion.gameObject.tag == "Spike")
        {
            Debug.Log("is dead");
            rb.simulated = false;
            m_spriteRenderer.enabled = false;
            StartCoroutine(Waitepsawntime());
            flashingScreen.Flash();
            
        }
    }


    public void UpdateCheckPoint(Vector2 pos)
    {
        checkPoint = pos;
        }
    private IEnumerator Waitepsawntime()
    {
        yield return new WaitForSeconds(respawnTime);
        transform.position = checkPoint;
        rb.velocity = new Vector2(0, 0);
        m_spriteRenderer.enabled = true;
        rb.simulated = true;
        Debug.Log("respawn");
        //Instantiate(player, respawnPoint.transform.position, Quaternion.identity);
    }
}
