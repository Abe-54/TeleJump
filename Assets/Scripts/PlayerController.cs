using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Abilities")]
    public bool hasWallSlide = false;
    public bool hasAirBurst = false;

    [Header("Movement Attributes")]
    public GameObject indicatorAnchor;
    public GameObject indicator;

    [Header("Shooting Attributes")]
    public GameObject projectilePrefab;
    public float projectileLifeTime = 2.0f;
    public float projectileSpeed = 10.0f;

    [Header("Wall Sliding Attributes")]
    public float wallHoldTime = 3.0f; // Time in seconds to hold onto the wall
    public LayerMask wallLayerMask; // Layer mask to identify what is considered a wall
    public LayerMask groundLayerMask; // Layer mask to identify what is considered the ground
    private float wallHoldTimer;

    [Header("Air Burst Attributes")]
    public ParticleSystem forceParticles;
    public LayerMask forceLayerMask;
    public float forceRadius = 1.0f;
    public float forceStrength = 10.0f;
    private float initialGravityScale;

    [Header("Debug")]
    public Vector3 mouseWorldPos;
    public bool isFacingRight = true;
    public bool isHoldingWall = false;
    public bool isGrounded = false;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialGravityScale = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        RotateIndicator();
        CheckFlipPlayer();
        CheckIfGrounded();

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        if (Input.GetMouseButtonDown(1) && hasAirBurst)
        {
            AddForceAtMousePosition();
        }

        if (hasWallSlide)
        {
            CheckForWall();
            if (isHoldingWall)
            {
                wallHoldTimer -= Time.deltaTime;
                if (wallHoldTimer <= 0)
                {
                    // Gradually restore gravity
                    rb.gravityScale = Mathf.MoveTowards(rb.gravityScale, initialGravityScale, Time.deltaTime);
                    if (rb.gravityScale == initialGravityScale)
                    {
                        isHoldingWall = false; // Player falls off the wall
                    }
                }
            }
        }
    }

    private void AddForceAtMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(mouseWorldPos, forceRadius, forceLayerMask);

        // Play particle effect
        forceParticles.transform.position = mouseWorldPos;
        forceParticles.Play();


        foreach (Collider2D collider in colliders)
        {
            Debug.Log("Adding force to " + collider.name);
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForceAtPosition(Vector2.up * forceStrength, mouseWorldPos, ForceMode2D.Impulse);
            }
        }
    }

    void OnDrawGizmos()
    {
        //only draw this gizmo when the game is running and the player pressed the mouse button\
        if (!Application.isPlaying || !Input.GetMouseButton(1))
        {
            return;
        }

        //draw a circle in the scene view to show the radius ussing gizmos
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mouseWorldPos, forceRadius);
    }

    void RotateIndicator()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
        mousePos -= objectPos;

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg - 90;

        //Limit angle to 180 degrees
        // angle = angle > 180 ? angle - 360 : angle;
        // angle = angle < -180 ? angle + 360 : angle;
        // angle = Mathf.Clamp(angle, -90, 90);

        indicatorAnchor.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    void CheckFlipPlayer()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
        mousePos -= objectPos;

        if (mousePos.x < 0 && isFacingRight)
        {
            FlipPlayer();
        }
        else if (mousePos.x > 0 && !isFacingRight)
        {
            FlipPlayer();
        }
    }

    void FlipPlayer()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        Debug.Log("Flipped player");
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, indicator.transform.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody2D>().velocity = indicator.transform.up * projectileSpeed;
        Destroy(projectile, projectileLifeTime);

    }

    void CheckForWall()
    {
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.0f, wallLayerMask);

        // Draw the ray in the Scene view
        if (hit.collider != null)
        {
            // If the ray hits a wall, draw it in red
            Debug.DrawRay(transform.position, direction * hit.distance, Color.red);
        }
        else
        {
            // If the ray does not hit a wall, draw it in green
            Debug.DrawRay(transform.position, direction * 1.0f, Color.green);
        }

        if (hit.collider != null)
        {
            // Wall detected
            if (!isHoldingWall)
            {
                isHoldingWall = true;
                wallHoldTimer = wallHoldTime;
                rb.gravityScale = 0; // Stop gravity
            }
        }
        else
        {
            // No wall detected
            if (isHoldingWall)
            {
                // Start falling off the wall
                isHoldingWall = false;
                rb.gravityScale = initialGravityScale;
            }
        }
    }

    void CheckIfGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.0f, groundLayerMask);

        // Draw the ray in the Scene view
        if (hit.collider != null)
        {
            // If the ray hits a wall, draw it in red
            Debug.DrawRay(transform.position, Vector2.down * hit.distance, Color.red);
        }
        else
        {
            // If the ray does not hit a wall, draw it in green
            Debug.DrawRay(transform.position, Vector2.down * 1.0f, Color.green);
        }

        if (hit.collider != null)
        {
            if (!isGrounded)
            {
                isGrounded = true;
            }
        }
        else
        {
            if (isGrounded)
            {
                isGrounded = false;
            }
        }
    }
}
