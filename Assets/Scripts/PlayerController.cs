using System.Collections.Generic;
using UnityEngine;

/// <summary>Which ability is currently selected in the cycle.</summary>
public enum SpecialType { ArcaneBlast, Fireball, IceBolt }

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Coyote Time & Jump Buffer")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Teleport")]
    [SerializeField] private float teleportMaxRange = 6f;
    [SerializeField] private float teleportCooldown = 1.5f;
    [SerializeField] private GameObject teleportReticlePrefab;
    [SerializeField] private LayerMask solidLayer;
    [SerializeField] private GameObject teleportGhostPrefab;

    [Header("Teleport VFX")]
    [Tooltip("Purple smoke puff — spawned at BOTH origin and destination when teleporting")]
    [SerializeField] private GameObject teleportSmokePrefab;

    [Header("Combat")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject arcaneBlastPrefab;
    [SerializeField] private float attackCooldown = 0.4f;

    [Header("Fireball  (Special Ability)")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float fireballCooldown = 0.5f;
    [SerializeField] private float fireballRecoilForce = 16f;
    [Tooltip("Vertical recoil is a one-time impulse (no lock), so it needs its own scale. Tune separately from horizontal.")]
    [SerializeField] private float fireballRecoilForceY = 8f;
    [SerializeField] private float recoilLockDuration = 0.45f;

    [Header("Ice Bolt  (Special Ability)")]
    [SerializeField] private GameObject iceBoltPrefab;
    [SerializeField] private float iceBoltCooldown = 1f;
    [SerializeField] private float iceFreezeMaxTime = 0.65f;

    [Header("Abilities")]
    [SerializeField] private AbilityData abilities;

    // ── Private state ──────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Animator animator;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpCooldown;
    private float attackCooldownCounter;
    private float fireballCooldownCounter;
    private float iceBoltCooldownCounter;
    private bool isGrounded;
    private bool facingRight = false;

    private float teleportCooldownCounter;
    private float recoilTimer;
    private float recoilVelocityX;   // stored so we can hold it during the lock window
    private bool scrollConsumed;
    private float iceFreezeTimer;
    private bool isFrozen;
    private int selectedSpecialIndex = 0;
    private readonly List<SpecialType> availableSpecials = new List<SpecialType>();
    [SerializeField] private bool isJumping;
    private float groundedTimer;
    private float defaultGravityScale;

    private GameObject reticleInstance;
    private GameObject activeGhost;

    // ── Lifecycle ──────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        defaultGravityScale = rb.gravityScale;
    }

    private void Start()
    {
        if (teleportReticlePrefab != null)
            reticleInstance = Instantiate(teleportReticlePrefab);

        ValidateReferences();
    }

    /// <summary>
    /// Runs once at startup and logs a warning for every unassigned
    /// Inspector field that will cause a silent failure at runtime.
    /// </summary>
    private void ValidateReferences()
    {
        if (firePoint == null)
            Debug.LogWarning("[PlayerController] 'Fire Point' is not assigned — projectiles will not spawn!", this);

        if (arcaneBlastPrefab == null)
            Debug.LogWarning("[PlayerController] 'Arcane Blast Prefab' is not assigned.", this);

        if (groundCheck == null)
            Debug.LogWarning("[PlayerController] 'Ground Check' transform is not assigned — grounding will not work.", this);

        if (abilities == null)
            Debug.LogWarning("[PlayerController] 'Abilities' ScriptableObject is not assigned — all ability checks will throw.", this);

        if (solidLayer == 0)
            Debug.LogWarning("[PlayerController] 'Solid Layer' mask is not set — teleport will never be blocked by walls or ground!", this);
    }

    private void Update()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
        HandleSpecialAbility(); // Left Click to fire · Scroll to cycle
        HandleTeleport();       // Right Click
        FaceMouse();
        UpdateAnimator();
    }

    // ── Ground & Jump ──────────────────────────────────────────────────
    private void CheckGround()
    {
        jumpCooldown -= Time.deltaTime;
        isGrounded = jumpCooldown <= 0f && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            groundedTimer += Time.deltaTime;
            if (rb.linearVelocity.y <= 0f && groundedTimer >= 0.08f)
                isJumping = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            groundedTimer = 0f;
            isJumping = true;
        }

        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleMovement()
    {
        recoilTimer -= Time.deltaTime;

        if (recoilTimer > 0f)
        {
            // Actively hold the recoil X velocity so player input can't fight it mid-launch
            rb.linearVelocity = new Vector2(recoilVelocityX, rb.linearVelocity.y);
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isJumping && rb.linearVelocity.y <= 0.1f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            jumpCooldown = (2f * jumpForce) / (rb.gravityScale * Mathf.Abs(Physics2D.gravity.y));
            isJumping = true;
            animator?.SetTrigger("6_Other");
        }
    }

    // ── Special Abilities — Left Click to fire · Scroll to cycle ─────────
    private void HandleSpecialAbility()
    {
        // All cooldowns tick every frame regardless of which spell is selected,
        // so switching away doesn't pause a spell's recharge.
        attackCooldownCounter   -= Time.deltaTime;
        fireballCooldownCounter -= Time.deltaTime;
        iceBoltCooldownCounter  -= Time.deltaTime;

        // Ice freeze state must be resolved every frame — process it before any input.
        if (isFrozen)
        {
            iceFreezeTimer -= Time.deltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            if (iceFreezeTimer <= 0f || isGrounded)
            {
                isFrozen = false;
                rb.gravityScale = defaultGravityScale;
            }
            return;  // No new input while frozen
        }

        // Build the ability list. ArcaneBlast is always slot 0 — the reliable
        // fallback the player can always scroll back to. Unlocked abilities follow.
        availableSpecials.Clear();
        availableSpecials.Add(SpecialType.ArcaneBlast);
        if (abilities.hasFireball) availableSpecials.Add(SpecialType.Fireball);
        if (abilities.hasIceBolt)  availableSpecials.Add(SpecialType.IceBolt);

        // Number keys select spell directly (silently ignored if not yet unlocked).
        if (Input.GetKeyDown(KeyCode.Alpha1))      SelectSpell(SpecialType.ArcaneBlast);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSpell(SpecialType.Fireball);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSpell(SpecialType.IceBolt);

        // Scroll wheel cycles — one step per physical scroll gesture.
        // scrollConsumed prevents a single trackpad swipe from skipping multiple slots.
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.01f)
            scrollConsumed = false;
        else if (!scrollConsumed && availableSpecials.Count > 1)
        {
            scrollConsumed = true;
            selectedSpecialIndex += scroll > 0f ? 1 : -1;
            selectedSpecialIndex = ((selectedSpecialIndex % availableSpecials.Count)
                                    + availableSpecials.Count) % availableSpecials.Count;
        }
        // Clamp so a future ability-loss edge case can't leave index out of range.
        selectedSpecialIndex = Mathf.Clamp(selectedSpecialIndex, 0, availableSpecials.Count - 1);

        // Hold left mouse button to fire the selected ability (cooldowns rate-limit it).
        if (!Input.GetMouseButton(0)) return;

        switch (availableSpecials[selectedSpecialIndex])
        {
            case SpecialType.ArcaneBlast:
                if (arcaneBlastPrefab == null || attackCooldownCounter > 0f) break;
                FireProjectile(arcaneBlastPrefab, AimDirection());
                attackCooldownCounter = attackCooldown;
                animator?.SetTrigger("2_Attack");
                AudioManager.Instance?.PlayArcaneBlast();
                break;

            case SpecialType.Fireball:
                if (fireballPrefab == null || fireballCooldownCounter > 0f) break;
                Vector2 dir = AimDirection();
                FireProjectile(fireballPrefab, dir);
                recoilVelocityX = -dir.x * fireballRecoilForce;
                rb.linearVelocity = new Vector2(recoilVelocityX, -dir.y * fireballRecoilForceY);
                recoilTimer = recoilLockDuration;
                fireballCooldownCounter = fireballCooldown;
                animator?.SetTrigger("2_Attack");
                AudioManager.Instance?.PlayFireball();
                break;

            case SpecialType.IceBolt:
                if (iceBoltPrefab == null || iceBoltCooldownCounter > 0f) break;
                FireProjectile(iceBoltPrefab, AimDirection());
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.gravityScale = 0f;
                iceFreezeTimer = iceFreezeMaxTime;
                isFrozen = true;
                iceBoltCooldownCounter = iceBoltCooldown;
                animator?.SetTrigger("2_Attack");
                AudioManager.Instance?.PlayIceBolt();
                break;
        }
    }

    /// <summary>
    /// The currently selected special, or null if no specials are unlocked yet.
    /// Use this from a HUD script to display the active spell icon.
    /// </summary>
    public SpecialType? CurrentSpecial =>
        availableSpecials.Count > 0 ? availableSpecials[selectedSpecialIndex] : (SpecialType?)null;

    // 0 = ready, 1 = just fired (fully cooling). Used by SpellHUD cooldown overlays.
    public float ArcaneBlastCooldown01 => attackCooldown   > 0f ? Mathf.Clamp01(attackCooldownCounter   / attackCooldown)   : 0f;
    public float FireballCooldown01    => fireballCooldown  > 0f ? Mathf.Clamp01(fireballCooldownCounter  / fireballCooldown)  : 0f;
    public float IceBoltCooldown01     => iceBoltCooldown   > 0f ? Mathf.Clamp01(iceBoltCooldownCounter   / iceBoltCooldown)   : 0f;

    // ── Teleport — Right Click (requires hasTeleport) ──────────────────
    private void HandleTeleport()
    {
        teleportCooldownCounter -= Time.deltaTime;

        if (!abilities.hasTeleport)
        {
            if (reticleInstance != null) reticleInstance.SetActive(false);
            return;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 toMouse = mouseWorld - transform.position;
        float distance = toMouse.magnitude;
        Vector3 targetPosition = distance <= teleportMaxRange
            ? mouseWorld
            : transform.position + (Vector3)(toMouse.normalized * teleportMaxRange);

        // useTriggers = false so Room bounds (trigger) don't block every position in the room
        var filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.SetLayerMask(solidLayer);
        filter.useTriggers = false;

        // 1. Destination must not be inside a wall
        var overlapResults = new Collider2D[1];
        bool destinationClear = Physics2D.OverlapCircle(targetPosition, 0.25f, filter, overlapResults) == 0;

        // 2. Path from player to destination must not pass through a wall.
        var lineResults = new RaycastHit2D[1];
        Vector2 lineStart = (Vector2)transform.position + Vector2.up * 0.4f;
        bool pathClear = Physics2D.Linecast(lineStart, targetPosition, filter, lineResults) == 0;

        bool onCooldown = teleportCooldownCounter > 0f;
        bool valid = destinationClear && pathClear && !onCooldown;

        if (reticleInstance != null)
        {
            reticleInstance.SetActive(true);
            reticleInstance.transform.position = targetPosition;
            var sr = reticleInstance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (onCooldown)
                    sr.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);  // dimmed grey while recharging
                else
                    sr.color = valid ? Color.green : Color.red;
            }
        }

        if (Input.GetMouseButtonDown(1) && valid)
        {
            teleportCooldownCounter = teleportCooldown;
            Vector3 originPosition = transform.position;

            // Spawn ghost copy at origin (triggers remote switches).
            // Raycast downward so the ghost lands on the nearest ground surface —
            // without this it floats mid-air when the player jumped before teleporting.
            if (teleportGhostPrefab != null)
            {
                if (activeGhost != null) Destroy(activeGhost);

                Vector3 ghostSpawnPos = originPosition;
                RaycastHit2D groundHit = Physics2D.Raycast(originPosition, Vector2.down, 12f, groundLayer);
                if (groundHit.collider != null)
                    ghostSpawnPos = groundHit.point;

                activeGhost = Instantiate(teleportGhostPrefab, ghostSpawnPos, Quaternion.identity);
            }

            // Departure smoke at origin
            if (teleportSmokePrefab != null)
                Instantiate(teleportSmokePrefab, originPosition, Quaternion.identity);

            // Move the wizard
            transform.position = targetPosition;
            AudioManager.Instance?.PlayTeleport();

            // Arrival smoke at destination
            if (teleportSmokePrefab != null)
                Instantiate(teleportSmokePrefab, targetPosition, Quaternion.identity);
        }
    }

    private void SelectSpell(SpecialType type)
    {
        int idx = availableSpecials.IndexOf(type);
        if (idx >= 0) selectedSpecialIndex = idx;
    }

    // ── Utilities ──────────────────────────────────────────────────────

    /// <summary>
    /// Instantiates a projectile prefab and calls Initialize on it.
    /// Searches root first, then children — handles any prefab hierarchy.
    /// Logs a warning if the Projectile component can't be found.
    /// </summary>
    private Projectile FireProjectile(GameObject prefab, Vector2 direction)
    {
        var go = Instantiate(prefab, firePoint.position, Quaternion.identity);
        var proj = go.GetComponent<Projectile>()
                ?? go.GetComponentInChildren<Projectile>();

        if (proj == null)
        {
            Debug.LogWarning($"[PlayerController] '{prefab.name}' has no Projectile component on root or any child. Did you forget to add it?", go);
            return null;
        }

        // Prevent the projectile from triggering against the player who fired it.
        // We do this at the physics level so the collision event never fires at all,
        // rather than filtering it out after the fact in OnTriggerEnter2D.
        var projCollider = go.GetComponent<Collider2D>()
                        ?? go.GetComponentInChildren<Collider2D>();
        if (projCollider != null)
        {
            foreach (var col in GetComponentsInChildren<Collider2D>())
                Physics2D.IgnoreCollision(projCollider, col);
        }

        proj.Initialize(direction);
        return proj;
    }

    /// <summary>Returns a normalised direction from the fire point toward the mouse cursor.</summary>
    private Vector2 AimDirection()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        return ((Vector2)mouseWorld - origin).normalized;
    }

    private void FaceMouse()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool mouseIsRight = mouseWorld.x > transform.position.x;

        if (mouseIsRight && !facingRight) Flip();
        else if (!mouseIsRight && facingRight) Flip();
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetBool("1_Move", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
