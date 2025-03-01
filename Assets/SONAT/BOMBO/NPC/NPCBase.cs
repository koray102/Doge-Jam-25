using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public abstract class NPCBase : MonoBehaviour
{   
    [Header("NPC Stats")]
    public float health = 100f;
    public float intelligence = 0f; // Inspector üzerinden ayarlanabilir
    private float xThickness;
    public bool startDirectionIsRight = true;

    [Header("Hareket Ayarları")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3f;

    [Header("Algılama ve Saldırı")]
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 1f; // İki saldırı arasındaki bekleme süresi

    [Header("Attack Timing")]
    public float attackDuration = 0.5f; // Saldırının süresi
    public float attackHitDelay = 0.2f; // Vuruşun gerçekleşme gecikmesi

    public float detectionRayOffset = 0.5f;
    public LayerMask detectionLayerMask;

    [Header("Chase Bellek Süresi")]
    public float chaseMemoryTime = 2f;
    protected float chaseTimer = 0f;
    protected int chaseDirectionSign = 0;

    [Header("Devriye Noktaları")]
    public Transform[] patrolPoints;
    protected int currentPatrolIndex = 0;

    [Header("Yer Kontrolü")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    protected bool isGrounded;

    public Transform rayPoint;

    protected Transform target;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;

    protected enum NPCState { Patrol, Chase }
    protected NPCState state = NPCState.Patrol;

    protected Vector2 facingDirection = Vector2.right;
    protected Vector2 lastFacingDirection = Vector2.right;
    
    protected Animator animator;

    public Transform attackPoint;

    public ParticleSystem deathExplosion;

    protected GameManagerScript gameManagerScript;
    protected Transform player;

    protected virtual void Start()
    {
        xThickness = transform.localScale.x;
        // Başlangıç yönü ayarı
        facingDirection = startDirectionIsRight ? Vector2.right : Vector2.left;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (gameManagerScript == null)
        {
            GameObject gmObj = GameObject.FindGameObjectWithTag("GameManager");
            if (gmObj != null)
                gameManagerScript = gmObj.GetComponent<GameManagerScript>();
        }

        CustomStart();
    }

    protected virtual void Update()
    {
        if (player == null)
            return;

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
                target = DetectTarget();
                if (target != null)
                {
                    state = NPCState.Chase;
                    chaseTimer = chaseMemoryTime;
                    chaseDirectionSign = (target.position.x - transform.position.x) >= 0 ? 1 : -1;
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

    protected Transform DetectTarget()
    {
        Vector2 originUpper = (Vector2)rayPoint.position + Vector2.up * detectionRayOffset;
        Vector2 originLower = (Vector2)rayPoint.position - Vector2.up * detectionRayOffset;
        RaycastHit2D hitUpper = Physics2D.Raycast(originUpper, facingDirection, detectionRange, detectionLayerMask);
        RaycastHit2D hitLower = Physics2D.Raycast(originLower, facingDirection, detectionRange, detectionLayerMask);

        if ((hitUpper.collider != null && hitUpper.collider.CompareTag("Player")) ||
            (hitLower.collider != null && hitLower.collider.CompareTag("Player")))
        {
            //Debug.Log("Player detected");
            return player;
        }
        else if (hitUpper.collider != null && hitUpper.collider.CompareTag("Hologram"))
        {
            HologramScript hs = hitUpper.transform.GetComponent<HologramScript>();
            if (!hs.isDetected)
                return hitUpper.transform;
        }
        else if(hitLower.collider != null && hitLower.collider.CompareTag("Hologram"))
        {
            HologramScript hs = hitLower.transform.GetComponent<HologramScript>();
            if (!hs.isDetected)
                return hitLower.transform;
        }
        return null;
    }

    protected void UpdateSpriteFlip()
    {
        if (spriteRenderer != null)
        {
            if (facingDirection.x < 0)
                transform.localScale = new Vector3(-Mathf.Abs(xThickness), transform.localScale.y, transform.localScale.z);
            else if (facingDirection.x > 0)
                transform.localScale = new Vector3(Mathf.Abs(xThickness), transform.localScale.y, transform.localScale.z);
        }
    }

    protected void TriggerAttackAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    protected void TriggerFakeAttackAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Fake Attack");
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if(deathExplosion) Instantiate(deathExplosion, transform.position, quaternion.identity);

        Destroy(gameObject);
    }

    // Türetilen sınıfların uygulaması gereken metotlar:
    protected abstract void Patrol();
    protected abstract void CustomStart();
    protected abstract void ChaseAndAttack();
    protected abstract void Attack();
    public abstract void GetDamage(float damage);

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 originUpper = (Vector2)rayPoint.position + Vector2.up * detectionRayOffset;
        Vector2 originLower = (Vector2)rayPoint.position - Vector2.up * detectionRayOffset;
        Gizmos.DrawLine(originUpper, originUpper + facingDirection * detectionRange);
        Gizmos.DrawLine(originLower, originLower + facingDirection * detectionRange);
    }
}
