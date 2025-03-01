using UnityEngine;
using System.Collections;

public class NPC2Controller : NPCBase
{
    [Header("NPC-2 Özel Ayarlar")]
    public bool allowIdleTurning = true;
    public float idleTurnInterval = 2f;
    private float idleTurnTimer = 0f;

    public GameObject projectilePrefab;
    public GameObject fakeProjectilePrefab; // Fake projectile için ek prefab
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 5f; // Proje hızı

    // Eskiden rastgele atış aralığı için kullanılan değerler kaldırıldı.
    
    // Yerel saldırı bayrağı (attack sequence devam ederken true)
    private bool isAttacking = false;

    protected override void CustomStart()
    {
        // NPC2 için özel başlangıç işlemleri (varsa)
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
        target = DetectTarget();
        if (target == null)
        {
            state = NPCState.Patrol;
            return;
        }

        float deltaX = target.position.x - transform.position.x;
        float absDeltaX = Mathf.Abs(deltaX);

        facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
        lastFacingDirection = facingDirection;

        // Eğer hedef attack range içerisindeyse ve saldırı aktif değilse saldırı başlatılır.
        if (absDeltaX <= attackRange)
        {
            if (!isAttacking)
            {
                Attack();
            }
        }
        else
        {
            // Hedef menzil dışındaysa, NPC2 oyuncuya yaklaşmaya çalışır.
            Vector2 newPos = transform.position;
            newPos.x = Mathf.MoveTowards(transform.position.x, target.position.x, chaseSpeed * Time.deltaTime);
            transform.position = newPos;
        }
    }

    protected override void Attack()
    {
        StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        isAttacking = true;
        TriggerAttackAnimation();
        ShootProjectile();
        // Saldırı süresi kadar bekle (attackDuration örn. 0.5 sn)
        yield return new WaitForSeconds(attackDuration);
        // Saldırı tamamlandıktan sonra, iki saldırı arasındaki bekleme süresi kadar (attackCooldown örn. 1 sn) bekle
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    void ShootProjectile()
    {
        if (projectileSpawnPoint != null)
        {
            // intelligence değerine bağlı olarak fake projectile şansı hesaplanır.
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
            GameObject proj = Instantiate(projToShoot, projectileSpawnPoint.position, Quaternion.identity);
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
