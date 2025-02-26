using Unity.VisualScripting;
using UnityEngine;

public abstract class NPCBase : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float chaseSpeed = 3f;

    [Header("Algılama ve Saldırı")]
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    protected float attackTimer = 0f;
    public float detectionRayOffset = 0.5f;
    public LayerMask detectionLayerMask;

    [Header("Chase Bellek Süresi")]
    public float chaseMemoryTime = 2f;
    protected float chaseTimer = 0f;
    protected int chaseDirectionSign = 0;


    [Header("Yer Kontrolü")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    protected bool isGrounded;

    [Header("NPC Statları")]
    public float health = 100f;
    public float intelligence = 0f;

    protected Transform player;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;

    protected enum NPCState { Patrol, Chase }
    protected NPCState state = NPCState.Patrol;

    protected Vector2 facingDirection = Vector2.right;
    protected Vector2 lastFacingDirection = Vector2.right;
    public bool startDirectionIsRight = true;

    protected Animator animator;

    public Transform attackPoint;

    public GameManagerScript gameManagerScript;

    protected virtual void Start()
    {
        // Başlangıç yönü ayarı
        facingDirection = startDirectionIsRight ? Vector2.right : Vector2.left;
        lastFacingDirection = facingDirection;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Animator bileşeni alınıyor.

        // Player objesini "Player" tag'iyle ara.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // GameManagerScript'i "GameManager" tag'iyle ara.
        GameObject gmObj = GameObject.FindGameObjectWithTag("GameManager");
        if (gmObj != null)
            gameManagerScript = gmObj.GetComponent<GameManagerScript>();

        OzelBaslangic();
    }

    protected virtual void Update()
    {
        if (player == null)
            return;

        // Debug: Mevcut state bildirimi
        Debug.Log($"{gameObject.name} state: {state}");

        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        switch (state)
        {
            case NPCState.Patrol:
                if (animator != null)
                {
                    animator.SetBool("IsPatrolling", true);
                    animator.SetBool("IsChasing", false);
                }
                Patrol();
                if (IsPlayerDetected())
                {
                    state = NPCState.Chase;
                    chaseTimer = chaseMemoryTime;
                    chaseDirectionSign = (player.position.x - transform.position.x) >= 0 ? 1 : -1;
                }
                break;
            case NPCState.Chase:
                if (animator != null)
                {
                    animator.SetBool("IsPatrolling", false);
                    animator.SetBool("IsChasing", true);
                }
                ChaseAndAttack();
                break;
        }

        UpdateSpriteFlip();
    }

    // Oyuncu algılama: Sadece NPC'nin önüne (facingDirection) doğru, yukarı ve aşağı offsetli raycast kullanılır.
    protected bool IsPlayerDetected()
    {
        Vector2 originUpper = (Vector2)transform.position + Vector2.up * detectionRayOffset;
        Vector2 originLower = (Vector2)transform.position - Vector2.up * detectionRayOffset;
        RaycastHit2D hitUpper = Physics2D.Raycast(originUpper, facingDirection, detectionRange, detectionLayerMask);
        RaycastHit2D hitLower = Physics2D.Raycast(originLower, facingDirection, detectionRange, detectionLayerMask);
        return ((hitUpper.collider != null && hitUpper.collider.CompareTag("Player")) ||
                (hitLower.collider != null && hitLower.collider.CompareTag("Player")));
    }

    protected void UpdateSpriteFlip()
    {
        float kalinlik = 1f;
        if (spriteRenderer != null)
        {
            if (facingDirection.x < 0)
                transform.localScale = new Vector3(-1 * kalinlik, transform.localScale.y, transform.localScale.z);
            else if (facingDirection.x > 0)
                transform.localScale = new Vector3(kalinlik, transform.localScale.y, transform.localScale.z);
        }
    }

    protected void TriggerAttackAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    // Hasar alma işlemi
    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        if (gameManagerScript != null)
            gameManagerScript.OlumOldu();
        Destroy(gameObject);
    }

    // Türeyen sınıflarda (NPC1, NPC2) uygulanması gereken metotlar:
    protected abstract void Patrol();
    protected abstract void OzelBaslangic();
    protected abstract void ChaseAndAttack();
    protected abstract void AttackPlayer();
    public abstract void GetDamage();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 originUpper = (Vector2)transform.position + Vector2.up * detectionRayOffset;
        Vector2 originLower = (Vector2)transform.position - Vector2.up * detectionRayOffset;
        Gizmos.DrawLine(originUpper, originUpper + facingDirection * detectionRange);
        Gizmos.DrawLine(originLower, originLower + facingDirection * detectionRange);
    }
}
