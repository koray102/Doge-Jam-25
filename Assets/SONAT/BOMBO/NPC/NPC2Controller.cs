using UnityEngine;

public class NPC2Controller : NPCBase
{
    [Header("NPC-2 Özel Ayarlar")]
    public bool allowIdleTurning = true;
    public float idleTurnInterval = 2f;
    private float idleTurnTimer = 0f;

    [Header("Projectile Ayarları")]
    public GameObject projectilePrefab;
    public GameObject fakeProjectilePrefab; // Artık iki tür projectile var.
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 5f;
    public float minProjectileSpeed = 3f;
    public float maxProjectileSpeed = 10f;

    protected override void OzelBaslangic()
    {
        TriggerAttackAnimation();
        // Başlangıçta isteğe bağlı bir projectile fırlatılır.
        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = facingDirection * projectileSpeed;
            }
            Debug.Log("NPC2: Projectile fırlatıldı.");
        }
    }

    protected override void Patrol()
    {
        // NPC2 devriye modunda hareket etmez, sabit kalır; yalnızca idle turning uygulanır.
        if (allowIdleTurning)
        {
            idleTurnTimer -= Time.deltaTime;
            if (idleTurnTimer <= 0f)
            {
                // İlk koddaki flip direction algoritması korunuyor.
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

        // Her zaman oyuncuya bak, sadece x ekseninde hizalanır.
        facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
        lastFacingDirection = facingDirection;

        // Eğer oyuncu attackRange (melee menzili gibi) içinde ise, projectile fırlatma saldırısı yapılır.
        if (absDeltaX <= attackRange)
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
            chaseTimer -= Time.deltaTime;
            if (chaseTimer <= 0f)
                state = NPCState.Patrol;
        }
    }

    protected override void AttackPlayer()
    {
        // NPC2 için melee saldırı yerine projectile saldırısı kullanılacaktır.
        TriggerAttackAnimation();
    }

    void ShootProjectile()
    {
        TriggerAttackAnimation();
        if (projectileSpawnPoint != null)
        {
            // Zeka yüksekse, projectileSpeed ani değişim gösterir.
            if (intelligence > 50f)
            {
                projectileSpeed = Random.Range(minProjectileSpeed, maxProjectileSpeed);
            }

            // Fake projectile olasılığı; zeka ile artar.
            float fakeChance = Mathf.Clamp(intelligence / 100f, 0f, 1f);
            bool useFake = Random.value < fakeChance;
            GameObject chosenPrefab = (useFake && fakeProjectilePrefab != null) ? fakeProjectilePrefab : projectilePrefab;

            GameObject proj = Instantiate(chosenPrefab, projectileSpawnPoint.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = facingDirection * projectileSpeed;
            }
            Debug.Log("NPC2: " + (useFake ? "Fake" : "Normal") + " projectile fırlatıldı. Hız: " + projectileSpeed);
        }
    }

    public override void GetDamage()
    {
        // Örneğin 10 hasar; değeri ihtiyaca göre ayarlayın.
        TakeDamage(10f);
    }
}
