using UnityEngine;
using System.Collections;

public class NPC1Controller : NPCBase
{
    [Header("Fake Attack Ayarları")]
    public float fakeAttackIntelligence = 1f;
    public float fakeAttackDelay = 1.0f;
    public float fakeAttackMultiplier = 0.5f;  // Örneğin, fakeAttackIntelligence 1 ise maksimum %50
    public float maxFakeAttackChance = 0.5f;     // Maksimum fake saldırı olasılığı
    private bool forceRealAttackNext = false;    // Fake saldırı gerçekleşirse, sonraki saldırı gerçek olmak zorunda

    [Header("Hologram Tespit Ayarları")]
    // NPC'nin hologramı fark etme (sahte olduğunu anlama) şansı (0-1 arası)
    public float hologramRealizationIntelligent = 0.7f;
    // Eğer fark edilemezse saldırıyı tekrarlama gecikmesi
    public float hologramAttackRetryDelay = 0.3f;

    [Header("Saldırı Bayrakları")]
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public bool isFakeAttack = false;
    // Saldırı bayraklarının aktif kalma süresi
    public float attackFlagDuration = 0.3f;

    protected override void Patrol()
    {
        if (patrolPoints.Length == 0)
            return;

        // Standart patrol hareketi:
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        if (Vector2.Distance(transform.position, targetPoint.position) > 2f)
        {
            facingDirection = (targetPoint.position.x - transform.position.x) >= 0 ? Vector2.right : Vector2.left;
            lastFacingDirection = facingDirection;
            transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, patrolSpeed * Time.deltaTime);
        }
        else
        {
            facingDirection = lastFacingDirection;
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        // Patrol modunda, eğer oyuncu veya (raycast ile gerçek şekilde tespit edilmiş) hologram varsa chase moduna geç.
        if (IsSomethingDetected() != null)
        {
            state = NPCState.Chase;
            chaseTimer = chaseMemoryTime;
        }
    }

    protected override void OzelBaslangic()
    {
        // NPC1 için özel başlangıç işlemleri (varsa)
    }

    protected override void ChaseAndAttack()
    {
        // Hedef seçimi: Base kodunuzdaki IsSomethingDetected() metodu hem Player hem de Hologram'ı yakalıyor.
        Transform detectedTarget = IsSomethingDetected();
        if (detectedTarget == null || !detectedTarget.gameObject.activeInHierarchy)
        {
            state = NPCState.Patrol;
            return;
        }
        // Eğer hedef hologram ise fakat saldırı sırasında oyuncu görünürse, hedef oyuncuya çevrilir.
        
        if (detectedTarget.CompareTag("Hologram") && IsSomethingDetected())
        {
            detectedTarget = target;  // base.target Player objesidir.
        }

        // Hareket yalnızca x ekseninde uygulanır:
        float deltaX = detectedTarget.position.x - transform.position.x;
        chaseTimer = chaseMemoryTime;
        facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
        lastFacingDirection = facingDirection;

        if (Mathf.Abs(deltaX) > attackRange)
        {
            Vector2 newPos = transform.position;
            newPos.x = Mathf.MoveTowards(transform.position.x, detectedTarget.position.x, chaseSpeed * Time.deltaTime);
            transform.position = newPos;
        }
        else
        {
            if (attackTimer <= 0f)
            {
                // Tek birleşik Attack fonksiyonu çağrılır:
                target = detectedTarget;
                Attack();
                attackTimer = attackCooldown;
            }
            else
            {
                attackTimer -= Time.deltaTime;
            }
        }
    }

    // Tek birleşik Attack fonksiyonu: Hedef Player veya Hologram olabilir.
    protected override void Attack()
    {
        Debug.Log("saldırı");


        isAttacking = true;
        TriggerAttackAnimation();

        if (target.CompareTag("Player"))
        {
            Debug.Log("NPC1: Player'a saldırı gerçekleştirildi!");
            target.GetComponent<PlayerController2D>().Die();
            StartCoroutine(ResetAttackFlags());
        }
        else if (target.CompareTag("Hologram"))
        {
            Debug.Log("NPC1: Holograma saldırı gerçekleştirildi!");
            HologramScript hs = target.GetComponent<HologramScript>();
            if (hs != null && !hs.isDetected)
            {
                // Zeka oranına bağlı olarak hologramın sahte olduğunu fark etme şansı:
                if (Random.value < hologramRealizationIntelligent )
                {
                    Debug.Log("NPC1: Hologramın sahte olduğu fark edildi. Bundan sonra saldırılmayacak.");
                    hs.isDetected = true;
                    StartCoroutine(ResetAttackFlags());
                }
            }
            else
            {
                StartCoroutine(ResetAttackFlags());
            }
        }
        else
        {
            StartCoroutine(ResetAttackFlags());
        }
    }


    private IEnumerator ResetAttackFlags()
    {
        yield return new WaitForSeconds(attackFlagDuration);
        isAttacking = false;
        isFakeAttack = false;
    }

    public override void GetDamage(float damage)
    {
        TakeDamage(damage);
    }
}
