using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportProjectile : MonoBehaviour
{
    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "TP_Wall")
        {
            player.transform.position = transform.position;
        }
        Destroy(gameObject);
    }
}
