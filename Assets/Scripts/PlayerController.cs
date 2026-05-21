using UnityEngine;

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
    [SerializeField] private GameObject teleportReticlePrefab;
    [SerializeField] private LayerMask solidLayer;
    [SerializeField] private GameObject teleportGhostPrefab;

    [Header("Combat")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject arcaneBlastPrefab;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float attackCooldown = 0.4f;

    [Header("Fireball Side Effect")]
    [SerializeField] private float fireballRecoilForce = 10f;
    [SerializeField] private float recoilLockDuration = 0.18f;

    [Header("Ice Bolt")]
    [SerializeField] private GameObject iceBoltPrefab;
    [SerializeField] private float iceBoltCooldown = 1f;
    [SerializeField] private float iceFreezeMaxTime = 0.65f;

    [Header("Abilities")]
    [SerializeField] private AbilityData abilities;

    private Rigidbody2D rb;
    private Animator animator;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpCooldown;
    private float attackCooldownCounter;
    private float iceBoltCooldownCounter;
    private bool isGrounded;
    private bool facingRight = false;

    private float recoilTimer;
    private float iceFreezeTimer;
    private bool isFrozen;
    [SerializeField] private bool isJumping;
    private float groundedTimer;
    private float defaultGravityScale;

    private GameObject reticleInstance;
    private GameObject activeGhost;

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
    }

    private void Update()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
        HandleIceBolt();
        HandleTeleport();
        HandleAttack();
        FaceMouse();
        UpdateAnimator();
    }

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
        if (recoilTimer > 0f) return;

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

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
    }

    private void HandleTeleport()
    {
        if (!abilities.hasTeleport)
        {
            if (reticleInstance != null)
                reticleInstance.SetActive(false);
            return;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 toMouse = mouseWorld - transform.position;
        float distance = toMouse.magnitude;
        Vector3 targetPosition = distance <= teleportMaxRange
            ? mouseWorld
            : transform.position + (Vector3)(toMouse.normalized * teleportMaxRange);

        bool valid = !Physics2D.OverlapCircle(targetPosition, 0.3f, solidLayer);

        if (reticleInstance != null)
        {
            reticleInstance.SetActive(true);
            reticleInstance.transform.position = targetPosition;

            var sr = reticleInstance.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = valid ? Color.green : Color.red;
        }

        if (Input.GetMouseButtonDown(1) && valid)
        {
            if (teleportGhostPrefab != null)
            {
                if (activeGhost != null) Destroy(activeGhost);
                activeGhost = Instantiate(teleportGhostPrefab, transform.position, Quaternion.identity);
            }
            transform.position = targetPosition;
        }
    }

    private void HandleAttack()
    {
        attackCooldownCounter -= Time.deltaTime;

        if (!Input.GetMouseButtonDown(0) || attackCooldownCounter > 0f)
            return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 direction = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        if (abilities.hasFireball)
        {
            Instantiate(fireballPrefab, firePoint.position, Quaternion.identity)
                .GetComponent<Projectile>()?.Initialize(direction);

            rb.linearVelocity = new Vector2(-direction.x * fireballRecoilForce, rb.linearVelocity.y);
            recoilTimer = recoilLockDuration;
        }
        else
        {
            Instantiate(arcaneBlastPrefab, firePoint.position, Quaternion.identity)
                .GetComponent<Projectile>()?.Initialize(direction);
        }

        attackCooldownCounter = attackCooldown;
        animator?.SetTrigger("2_Attack");
    }

    private void HandleIceBolt()
    {
        iceBoltCooldownCounter -= Time.deltaTime;

        if (isFrozen)
        {
            iceFreezeTimer -= Time.deltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

            if (iceFreezeTimer <= 0f || isGrounded)
            {
                isFrozen = false;
                rb.gravityScale = defaultGravityScale;
            }
            return;
        }

        if (!abilities.hasIceBolt) return;
        if (!Input.GetKeyDown(KeyCode.E) || iceBoltCooldownCounter > 0f) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 direction = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        if (iceBoltPrefab != null)
            Instantiate(iceBoltPrefab, firePoint.position, Quaternion.identity)
                .GetComponent<Projectile>()?.Initialize(direction);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.gravityScale = 0f;
        iceFreezeTimer = iceFreezeMaxTime;
        isFrozen = true;
        iceBoltCooldownCounter = iceBoltCooldown;
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
