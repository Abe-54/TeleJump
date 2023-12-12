using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TeleportProjectile : MonoBehaviour
{
    private Transform playerTransform;
    public AudioClip tpSound;
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
        SpriteRenderer projectileSpriteRenderer = GetComponent<SpriteRenderer>();

        for (float t = 0; t < projectileLifeTime; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(1.0f, 0.0f, t / projectileLifeTime);
            projectileSpriteRenderer.color = new Color(projectileSpriteRenderer.color.r, projectileSpriteRenderer.color.g, projectileSpriteRenderer.color.b, alpha);
            yield return null;
        }

        // yield return new WaitForSeconds(projectileLifeTime);
        DeactivateProjectile();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {

        if (other.gameObject.tag == "Player") return; // Ignore collision with player

        if (other.gameObject.tag == "TP_Wall")
        {
            playerTransform.position = transform.position; // Teleport player

            // Play teleport sound
            AudioSource.PlayClipAtPoint(tpSound, transform.position);
        }

        if (other.gameObject.tag == "Reset")
        {
            playerTransform.position = transform.position; // Teleport player
            playerTransform.GetComponent<PlayerController>().ammo++;
            other.gameObject.GetComponent<ResetLogic>().ResetPlayer();
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