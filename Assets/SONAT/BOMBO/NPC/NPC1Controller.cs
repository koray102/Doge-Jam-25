using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

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
        Debug.Log("Saldırı başlatıldı");
        isAttacking = true;

        // 1. Zorla gerçek saldırı durumu
        if (forceRealAttackNext)
        {
            ExecuteRealAttack();
            forceRealAttackNext = false;
            return;
        }

        // 2. Sahte saldırı olasılığını hesapla
        float fakeAttackChance = Mathf.Clamp(fakeAttackIntelligence * fakeAttackMultiplier, 0f, maxFakeAttackChance);
        if (Random.value < fakeAttackChance)
        {
            ExecuteFakeAttack();
            return;
        }

        // 3. Normal gerçek saldırı
        ExecuteRealAttack();
    }

    private void ExecuteRealAttack()
    {
        TriggerAttackAnimation();

        // Oyuncuya saldırı
        if (target.CompareTag("Player"))
        {
            // LayerMask için bit kayması kullanarak doğru maskeyi oluşturuyoruz
            int playerLayerMask = 1 << LayerMask.NameToLayer("PLAYER");
            if (Physics2D.OverlapCircle(new Vector2(attackPoint.position.x, attackPoint.position.y), attackRange, playerLayerMask))
            {
                Debug.Log("NPC: Oyuncuya saldırı gerçekleştirildi!");
                target.GetComponent<PlayerController2D>().Die();
            }
            else
            {
                Debug.Log("NPC: Oyuncuya saldırı denemesi başarısız oldu.");
            }
        }
        // Holograma saldırı
        else if (target.CompareTag("Hologram"))
        {
            int hologramLayerMask = 1 << LayerMask.NameToLayer("Flower");
            if (Physics2D.OverlapCircle(new Vector2(attackPoint.position.x, attackPoint.position.y), attackRange, hologramLayerMask))
            {
                Debug.Log("NPC: Holograma saldırı gerçekleştirildi!");
                HologramScript hs = target.GetComponent<HologramScript>();
                if (hs != null && !hs.isDetected)
                {
                    // Zeka oranına bağlı olarak hologramın sahte olduğu fark edilebilir.
                    if (Random.value < hologramRealizationIntelligent)
                    {
                        Debug.Log("NPC: Hologramın sahte olduğu fark edildi. Bundan sonra saldırı yapılmayacak.");
                        hs.isDetected = true;
                    }
                }
            }
        }

        StartCoroutine(ResetAttackFlags());
    }

    private void ExecuteFakeAttack()
    {
        isFakeAttack = true;
        TriggerFakeAttackAnimation();
        Debug.Log("NPC: Sahte saldırı gerçekleştiriliyor, bekleme süresi uygulanıyor...");
        StartCoroutine(FakeAttackCoroutine());
        StartCoroutine(ResetAttackFlags());
    }

    private IEnumerator FakeAttackCoroutine()
    {
        yield return new WaitForSeconds(fakeAttackDelay);
        // Fake saldırı sonrası sonraki saldırı kesinlikle gerçek olacak.
        forceRealAttackNext = true;
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
