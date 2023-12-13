using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Abilities")]
    public bool canShoot = true;
    public bool hasWallSlide = false;
    public bool hasAirBurst = false;
    [Space(10)]

    [Header("Movement Attributes")]
    public GameObject indicatorAnchor;
    public GameObject indicator;
    [Space(10)]

    [Header("Shooting Attributes")]
    public GameObject projectilePrefab;
    public GameObject projectileSpawnPoint;
    public int maxAmmo = 3;
    public int ammo = 3;
    public float rechargeTime = 2.0f;
    public float projectileSpeed = 10.0f;
    [Space(10)]

    [Header("Wall Sliding Attributes")]
    public float wallHoldTime = 3.0f; // Time in seconds to hold onto the wall
    public LayerMask wallLayerMask; // Layer mask to identify what is considered a wall
    public LayerMask groundLayerMask; // Layer mask to identify what is considered the ground
    private float wallHoldTimer;
    [Space(10)]

    [Header("Air Burst Attributes")]
    public AudioClip airBurstSound;
    public ParticleSystem forceParticles;
    public LayerMask forceLayerMask;
    public GameObject forceIndicator;
    public float airBurstCooldown = 0.2f;
    private float lastAirBurstTime = 0f;
    public float forceStrength = 10.0f;
    public float selfForceStrength = 20.0f;
    private float initialGravityScale;
    [Space(10)]

    [Header("Camera Attributes")]
    public CinemachineVirtualCamera playerVCam;
    public CinemachineConfiner playerVCamConfiner;
    public PolygonCollider2D currentConfinerCollider;
    [Space(10)]

    [Header("Art")]
    public List<Sprite> idleSprite;
    public List<Sprite> nSprites;
    public List<Sprite> neSprites;
    public List<Sprite> eSprites;
    public List<Sprite> seSprites;
    public List<Sprite> fallingSprites;


    [Header("Debug")]
    public Vector3 mouseWorldPos;
    public bool isFacingRight = true;
    public bool isHoldingWall = false;
    public bool isGrounded = false;
    public ForceIndicatorEditor forceIndicatorEditor;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool shouldShoot = false;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        initialGravityScale = rb.gravityScale;

        forceIndicator.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!canShoot)
        {
            return;
        }
        RotateIndicator();
        CheckFlipPlayer();
        CheckIfGrounded();

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 10; // Set to an appropriate depth
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        forceIndicator.transform.position = mouseWorldPos;

        if (Input.GetMouseButtonDown(0) && ammo > 0)
        {
            shouldShoot = true;
        }

        if (ammo > maxAmmo)
        {
            ammo = maxAmmo;
        }

        if (hasAirBurst)
        {
            // Move force indicator to follow the mouse
            if (Input.GetMouseButtonDown(1) && Time.time - lastAirBurstTime >= airBurstCooldown)
            {
                AddForceAtMousePosition();
                lastAirBurstTime = Time.time;
            }
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

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSpriteDirection();
    }

    private void FixedUpdate()
    {
        if (shouldShoot && canShoot)
        {
            StartCoroutine(Shoot());
            shouldShoot = false; // Reset the shooting flag
        }
    }

    // Returns the sprite to use based on the direction of the mouse
    Sprite GetSpriteDirection()
    {
        if (!IsMouseWithinRange())
        {
            // If the mouse is outside the range, return the current sprite
            // or a default sprite. This depends on your game logic.
            return GetComponent<SpriteRenderer>().sprite;
        }

        // convert mouse position from screen to world coordinates
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // direction vector from the player to the mouse position
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        // calculating the angle of the direction vector in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Adjust angle for flipped player
        if (transform.localScale.x < 0)
        {
            angle = 180 - angle;
        }

        // Normalize the angle to be within 0-360 range
        if (angle < 0) angle += 360;

        // Debug.Log("Angle: " + angle);

        // checking the angle to determine which sprite to use
        if (angle > 75 && angle <= 105) // shooting directly above
        {
            return nSprites[0];
        }
        else if ((angle > 45 && angle <= 75) || (angle > 105 && angle < 135)) // shooting northeast or northwest
        {
            return neSprites[0]; // or nwSprites[0] depending on your sprite setup
        }
        else if (angle > 315 && angle <= 359) // shooting southeast or southwest
        {
            return seSprites[0]; // or swSprites[0] depending on your sprite setup
        }
        else // shooting directly in front (east or west)
        {
            return eSprites[0];
        }
    }

    // Adds force to all objects within the force radius
    private void AddForceAtMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(mouseWorldPos, forceIndicatorEditor.forceRadius, forceLayerMask);

        // Play particle effect
        forceParticles.transform.position = mouseWorldPos;
        forceParticles.Play();

        //Play sound effect
        if (audioSource != null && airBurstSound != null)
        {
            audioSource.PlayOneShot(airBurstSound);
        }

        foreach (Collider2D collider in colliders)
        {
            Debug.Log("Adding force to " + collider.name);

            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = collider.transform.position - mouseWorldPos;
                direction.Normalize(); // Normalize the direction

                if (collider.gameObject.tag == "Player")
                {
                    rb.AddForce(direction * selfForceStrength, ForceMode2D.Impulse);
                }
                else
                {
                    rb.AddForce(direction * forceStrength, ForceMode2D.Impulse);
                }
            }
        }
    }


    // Draw a circle in the scene view to show the radius
    void OnDrawGizmos()
    {
        //only draw this gizmo when the game is running and the player pressed the mouse button
        if (!Application.isPlaying || !Input.GetMouseButton(1))
        {
            return;
        }

        //draw a circle in the scene view to show the radius ussing gizmos
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mouseWorldPos, forceIndicatorEditor.forceRadius);
    }

    // Rotates the indicator to point towards the mouse
    void RotateIndicator()
    {
        if (!IsMouseWithinRange()) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
        mousePos -= objectPos;

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg - 90;

        //Limit angle to 180 degrees
        angle = angle > 220 ? angle - 360 : angle;
        angle = angle < -220 ? angle + 360 : angle;
        angle = Mathf.Clamp(angle, -130, 130);

        indicatorAnchor.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    // Flips the player to face the mouse
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

    // Flips the player to face the opposite direction
    void FlipPlayer()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        Debug.Log("Flipped player");
    }

    // Shoots a projectile
    IEnumerator Shoot()
    {
        GameObject newProjectile = Instantiate(projectilePrefab, projectileSpawnPoint.transform.position, Quaternion.identity);
        newProjectile.SetActive(false);

        TeleportProjectile projectileScript = newProjectile.GetComponent<TeleportProjectile>();
        projectileScript.Initialize(playerVCam, transform);

        newProjectile.SetActive(true);

        Rigidbody2D projectileRb = newProjectile.GetComponent<Rigidbody2D>();
        projectileRb.velocity = indicator.transform.up * projectileSpeed;

        ammo--;

        yield return new WaitWhile(() => newProjectile != null);

        ammo = Mathf.Min(ammo + 1, maxAmmo);
    }


    // Checks if the player is holding onto a wall
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

    // Checks if the player is grounded
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

    // Checks if the mouse is within the range of the player
    bool IsMouseWithinRange()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
        mousePos -= objectPos;

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;

        // Normalize the angle to be within 0-360 range
        if (angle < 0) angle += 360;

        return (angle >= 0 && angle <= 225) || (angle >= 315 && angle <= 360);
    }

    public void SetCanShoot(bool canShoot)
    {
        this.canShoot = canShoot;
    }

    public void SetHasWallSlide(bool hasWallSlide)
    {
        this.hasWallSlide = hasWallSlide;
    }

    public void SetHasAirBurst(bool hasAirBurst)
    {
        this.hasAirBurst = hasAirBurst;
    }
}