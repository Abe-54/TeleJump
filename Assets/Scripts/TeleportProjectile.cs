using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TeleportProjectile : MonoBehaviour
{
    private Transform playerTransform;
    private CinemachineVirtualCamera vCam;
    public float projectileLifeTime = 2.0f;

    private void OnEnable()
    {
        Debug.Log("vCam: " + vCam);
        if (vCam != null)
        {
            Debug.Log("Projectile enabled");
            vCam.Follow = transform; // Set camera to follow this projectile
            Debug.Log("Camera follow: " + vCam.Follow);
        }
        Debug.Log("Resetting after lifetime");
        StartCoroutine(ResetAfterLifetime());
    }

    public void Initialize(CinemachineVirtualCamera camera, Transform player)
    {
        vCam = camera;
        playerTransform = player;
    }

    private IEnumerator ResetAfterLifetime()
    {
        yield return new WaitForSeconds(projectileLifeTime);
        DeactivateProjectile();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {

        if (other.gameObject.tag == "Player") return; // Ignore collision with player

        if (other.gameObject.tag == "TP_Wall")
        {
            playerTransform.position = transform.position; // Teleport player
        }
        DeactivateProjectile();
    }

    private void DeactivateProjectile()
    {
        ResetCameraFollow();
        gameObject.SetActive(false);
    }

    private void ResetCameraFollow()
    {
        if (vCam != null && vCam.Follow == transform)
        {
            vCam.Follow = playerTransform; // Reset camera to follow the player
        }
    }
}
