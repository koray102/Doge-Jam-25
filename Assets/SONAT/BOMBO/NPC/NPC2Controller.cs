using UnityEngine;

public class NPC2Controller : NPCBase
{
    [Header("NPC-2 Özel Ayarlar")]
    public bool allowIdleTurning = true;
    public float idleTurnInterval = 2f;
    private float idleTurnTimer = 0f;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 5f;
    public float meleeAttackRange = 0.5f;



    protected override void ozelBaslangic()
    {
        TriggerAttackAnimation();
        if (projectilePrefab != null && projectileSpawnPoint != null)
        {

            GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                // İniş hızı veriliyor (velocity kullanıyoruz)
                projRb.linearVelocity = facingDirection * projectileSpeed;
            }
            Debug.Log("NPC2: Projectile fırlatıldı.");
        }
    }
    protected override void Patrol()
    {
        // NPC2 devriye modunda hareket etmez, sabit durur.
        // Idle turning açıksa, belirli aralıklarla yönünü tersine çevirir.
        if (allowIdleTurning)
        {
   
            idleTurnTimer -= Time.deltaTime;
            if (idleTurnTimer <= 0f)
            {
                facingDirection = (facingDirection == Vector2.right) ? Vector2.left : Vector2.right;
                lastFacingDirection = facingDirection;
                idleTurnTimer = idleTurnInterval;
            }
        }
    }

    protected override void ChaseAndAttack()
    {

            float deltaX = player.position.x - transform.position.x;
            float absDeltaX = Mathf.Abs(deltaX);

            // Her zaman oyuncuya bak, sadece x ekseninde hizalan.
            facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
            lastFacingDirection = facingDirection;

            // Eğer oyuncu melee menzili içindeyse melee saldırısı yap.
            if (absDeltaX <= meleeAttackRange)
            {
                if (attackTimer <= 0f)
                {
                    MeleeAttack();
                    attackTimer = attackCooldown;
                }
                else
                {
                    attackTimer -= Time.deltaTime;
                }
            }
            // Uzaktan saldırı: oyuncu melee mesafesinin dışında fakat attackRange içindeyse projectile fırlat.
            else if (absDeltaX <= attackRange)
            {
                if (attackTimer <= 0f)
                {
                    ShootProjectile();
                    attackTimer = attackCooldown;
                }
                else
                {
                    attackTimer -= Time.deltaTime;
                }
            }
            else
            {
                // Eğer oyuncu attackRange dışında ise sadece oyuncuya bakmaya devam et.
                chaseTimer -= Time.deltaTime;
                if (chaseTimer <= 0f)
                {
                    state = NPCState.Patrol;
                }
            

            }

    }

    protected override void AttackPlayer()
    {
        // Bu metot varsayılan saldırı için kullanılabilir; NPC2 için ayrı melee veya ranged metotlar kullanıyoruz.
        TriggerAttackAnimation();
    }

    void ShootProjectile()
    {
        TriggerAttackAnimation();
        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
  
            GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.Euler(0,0,Random.Range(0, 360)));
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                // İniş hızı veriliyor (velocity kullanıyoruz)
                projRb.linearVelocity = facingDirection * projectileSpeed;
            }
            Debug.Log("NPC2: Projectile fırlatıldı.");
        }
    }


    void MeleeAttack()
    {
        Debug.Log("NPC2: Melee saldırı yapıldı!");
    }

    public override void GetDamage()
    {
        gameManagerScript.OlumOldu();
        Destroy(gameObject);
    }
}
