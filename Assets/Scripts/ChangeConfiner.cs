using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeConfiner : MonoBehaviour
{
    private Cinemachine.CinemachineConfiner confiner;

    void Start()
    {
        confiner = FindObjectOfType<Cinemachine.CinemachineConfiner>();
    }

    public void ChangeConfiner2DTo(Collider2D newConfiner)
    {
        Debug.Log("Changing confiner to " + newConfiner.name);
        confiner.m_BoundingShape2D = newConfiner;
    }
}
