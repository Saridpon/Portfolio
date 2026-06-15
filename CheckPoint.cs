using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    Dead dead;
    public Transform respawnPoint;
    // Start is called before the first frame update
    private void Awake()
    {
        dead = GameObject.FindGameObjectWithTag("Player").GetComponent<Dead>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            dead.UpdateCheckPoint(respawnPoint.position);
        }
    }
}
