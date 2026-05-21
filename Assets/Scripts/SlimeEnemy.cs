using UnityEngine;

public class SlimeEnemy : EnemyBase
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] private float edgeCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    private SpriteRenderer spriteRenderer;
    private bool movingRight = true;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Move();
        CheckTurnConditions();
    }

    private void Move()
    {
        float dir = movingRight ? 1f : -1f;
        transform.Translate(Vector2.right * dir * moveSpeed * Time.deltaTime);
    }

    private void CheckTurnConditions()
    {
        float dir = movingRight ? 1f : -1f;

        Vector2 wallOrigin = (Vector2)transform.position + Vector2.right * dir * wallCheckDistance;
        bool wallAhead = Physics2D.Raycast(wallOrigin, Vector2.right * dir, wallCheckDistance, groundLayer);

        Vector2 edgeOrigin = (Vector2)transform.position + Vector2.right * dir * 0.4f;
        bool groundAhead = Physics2D.Raycast(edgeOrigin, Vector2.down, edgeCheckDistance, groundLayer);

        if (wallAhead || !groundAhead)
            Flip();
    }

    private void Flip()
    {
        movingRight = !movingRight;
        spriteRenderer.flipX = !movingRight;
    }
}