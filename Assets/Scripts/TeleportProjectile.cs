using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TeleportProjectile : MonoBehaviour
{
    private Transform playerTransform;
    public AudioClip tpSound;

    private AudioSource playerAudioSource;
    private CinemachineVirtualCamera vCam;
    public float projectileLifeTime = 2.0f;

    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void Start()
    {
        playerAudioSource = FindObjectOfType<PlayerController>().GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (spriteRenderer != null && trailRenderer != null)
        {
            Color trailColor = trailRenderer.startColor;
            trailColor.a = spriteRenderer.color.a;
            trailRenderer.startColor = trailColor;
            trailRenderer.endColor = trailColor;
        }
    }

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
        for (float t = 0; t < projectileLifeTime; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(1.0f, 0.0f, t / projectileLifeTime);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);

            yield return null;
        }

        DeactivateProjectile();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player") return; // Ignore collision with player

        if (other.gameObject.tag == "TP_Wall" || other.gameObject.tag == "Repawn")
        {
            playerTransform.position = transform.position; // Teleport player

            // Reset player's velocity
            Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.angularVelocity = 0;
            }

            // Play teleport sound
            if (playerAudioSource != null && tpSound != null)
            {
                playerAudioSource.PlayOneShot(tpSound);
            }
        }

        if (other.gameObject.tag == "Repawn")
        {
            playerTransform.GetComponent<PlayerController>().ammo++;
            other.gameObject.GetComponent<ResetLogic>().ResetPlayer();
        }

        DeactivateProjectile();
    }

    private void DeactivateProjectile()
    {
        ResetCameraFollow();
        Destroy(gameObject);
    }

    private void ResetCameraFollow()
    {
        if (vCam != null && vCam.Follow == transform)
        {
            vCam.Follow = playerTransform; // Reset camera to follow the player
        }
    }
}