using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetLogic : MonoBehaviour
{
    public Transform resetPosition;
    public PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetPlayer()
    {
        playerController.transform.position = resetPosition.position;
        playerController.transform.rotation = resetPosition.rotation;
        playerController.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        playerController.GetComponent<Rigidbody2D>().angularVelocity = 0;
    }
}