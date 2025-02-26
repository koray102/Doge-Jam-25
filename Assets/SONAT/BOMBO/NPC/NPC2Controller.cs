using UnityEngine;

public class NPC2Controller : NPCBase
{
    [Header("NPC-2 Özel Ayarlar")]
    public bool allowIdleTurning = true;
    public float idleTurnInterval = 2f;
    private float idleTurnTimer = 0f;

    public GameObject projectilePrefab;
    public GameObject fakeProjectilePrefab; // Fake projectile için ek prefab
    public Transform projectileSpawnPoint;

    public float projectileSpeed = 5f; // Proje hızı sabit

    // Firing rate ayarları: temel atış aralığı ve zeka etkisiyle rastgele değişen sapma
    public float baseFiringInterval = 1f;
    public float firingIntervalVariation = 0.5f;
    public float minFiringInterval = 0.3f;

    protected override void OzelBaslangic()
    {
        TriggerAttackAnimation();
        if (projectilePrefab != null && projectileSpawnPoint != null)
            ShootProjectile();
    }

    protected override void Patrol()
    {
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

        facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
        lastFacingDirection = facingDirection;

        // Rastgele, düzensiz atış aralığı hesaplaması: her atış sonrası yeni rastgele değer oluşturulur.
        float randomOffset = Random.Range(-firingIntervalVariation, firingIntervalVariation) * intelligence;
        float randomFiringInterval = Mathf.Clamp(baseFiringInterval + randomOffset, minFiringInterval, baseFiringInterval + firingIntervalVariation);

        if (absDeltaX <= attackRange)
        {
            if (attackTimer <= 0f)
            {
                ShootProjectile();
                attackTimer = randomFiringInterval;
            }
            else
            {
                attackTimer -= Time.deltaTime;
            }
        }
        else
        {
            // Chase zamanı bitse dahi, NPC2 her zaman oyuncunun x konumuna yaklaşmaya çalışır.
            Vector2 newPos = transform.position;
            newPos.x = Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime);
            transform.position = newPos;
            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;
        }
    }

    protected override void AttackPlayer()
    {
        TriggerAttackAnimation();
    }

    void ShootProjectile()
    {
        TriggerAttackAnimation();
        if (projectileSpawnPoint != null)
        {
            // Zeka arttıkça fake projectile şansı lineer olarak %50'ye kadar artar.
            float fakeChance = Mathf.Clamp(intelligence * 0.5f, 0f, 0.5f);
            GameObject projToShoot = projectilePrefab;
            if (Random.value < fakeChance && fakeProjectilePrefab != null)
            {
                projToShoot = fakeProjectilePrefab;
                Debug.Log("NPC2: Fake projectile fırlatılıyor.");
            }
            else
            {
                Debug.Log("NPC2: Gerçek projectile fırlatılıyor.");
            }

            GameObject proj = Instantiate(projToShoot, projectileSpawnPoint.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            if (projRb != null)
                projRb.linearVelocity = facingDirection * projectileSpeed;
        }
    }

    public override void GetDamage(float damage)
    {
        TakeDamage(damage);
    }
}
