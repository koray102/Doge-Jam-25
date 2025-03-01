using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class NPC1Controller : NPCBase
{
    [Header("Fake Attack Settings")]
    public float fakeAttackIntelligence = 1f;
    public float fakeAttackMultiplier = 0.5f;  // Örneğin, fakeAttackIntelligence 1 ise maksimum %50
    public float maxFakeAttackChance = 0.5f;     // Maksimum fake saldırı olasılığı
    private bool forceRealAttackNext = false;

    [Header("Hologram Detection Settings")]
    public float hologramRealizationIntelligence = 0.7f;
    public float hologramAttackRetryDelay = 0.3f;

    [Header("Attack Flags")]
    public bool isAttacking = false;
    public bool isFakeAttack = false;
    private bool canAttack = true;

    // Base sınıftaki attackDuration, attackCooldown ve attackHitDelay kullanılmaktadır.

    protected override void Patrol()
    {
        if (!isFakeAttack)
            spriteRenderer.color = Color.white;

        if (patrolPoints.Length == 0)
            return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 targetPosition = new Vector2(targetPoint.position.x, transform.position.y);
        if (Vector2.Distance(transform.position, targetPoint.position) > 1f)
        {
            facingDirection = (targetPoint.position.x - transform.position.x) >= 0 ? Vector2.right : Vector2.left;
            lastFacingDirection = facingDirection;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);
        }
        else
        {
            facingDirection = lastFacingDirection;
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        if (DetectTarget() != null)
        {
            state = NPCState.Chase;
            chaseTimer = chaseMemoryTime;
        }
    }

    protected override void CustomStart()
    {
        // NPC1 için özel başlangıç işlemleri (varsa)
    }

    protected override void ChaseAndAttack()
    {
        if (!isFakeAttack)
            spriteRenderer.color = Color.white;

        if(isAttacking || isFakeAttack)
            return;

        Transform detectedTarget = DetectTarget();
        if (detectedTarget == null || !detectedTarget.gameObject.activeInHierarchy)
        {
            state = NPCState.Patrol;
            return;
        }
        // Eğer hedef hologram ise fakat oyuncu görünüyorsa, hedef oyuncu olur.
        if (detectedTarget.CompareTag("Hologram") && DetectTarget() != null)
            detectedTarget = target;  // Base'teki target oyuncu nesnesidir.

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
            if (canAttack)
            {
                canAttack = false;
                target = detectedTarget;
                Attack();
            }
        }
    }

    protected override void Attack()
    {
        StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        isAttacking = true;
        // Eğer forceRealAttackNext aktifse, zorla gerçek saldırı gerçekleştir.
        if (forceRealAttackNext)
        {
            ExecuteRealAttack();
            forceRealAttackNext = false;
        }
        else
        {
            float calculatedFakeAttackChance = Mathf.Clamp(fakeAttackIntelligence * fakeAttackMultiplier, 0f, maxFakeAttackChance);
            if (Random.value < calculatedFakeAttackChance)
                ExecuteFakeAttack();
            else
                ExecuteRealAttack();
        }
        // Saldırının süresi kadar bekle (attackDuration örn. 0.5 sn)
        yield return new WaitForSeconds(attackDuration);
        animator.SetTrigger("EndAttack");
        isAttacking = false;
        isFakeAttack = false;

        // Ardından iki saldırı arasındaki bekleme süresi kadar (attackCooldown örn. 1 sn) bekle
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void ExecuteRealAttack()
    {
        TriggerAttackAnimation();
        if (target.CompareTag("Player"))
        {
            // Belirlenen gecikme sonrası oyuncuya saldırı uygula
            Invoke("ApplyDamage", attackHitDelay);
        }
        else if (target.CompareTag("Hologram"))
        {
            int hologramLayerMask = 1 << LayerMask.NameToLayer("Flower");
            if (Physics2D.OverlapCircle(attackPoint.position, attackRange, hologramLayerMask))
            {
                Debug.Log("NPC: Holograma saldırı gerçekleştirildi!");
                HologramScript hs = target.GetComponent<HologramScript>();
                if (hs != null && !hs.isDetected)
                {
                    if (Random.value < hologramRealizationIntelligence)
                    {
                        Debug.Log("NPC: Hologramın sahte olduğu fark edildi. Bundan sonra saldırı yapılmayacak.");
                        hs.isDetected = true;
                    }
                }
            }
        }
    }

    private void ExecuteFakeAttack()
    {
        spriteRenderer.color = Color.magenta;
        isFakeAttack = true;
        TriggerFakeAttackAnimation();
        Debug.Log("NPC: Sahte saldırı gerçekleştiriliyor...");
        // Fake saldırıda da attackDuration ve attackCooldown süreleri uygulanacağından,
        // sonraki saldırı kesinlikle gerçek olsun.
        forceRealAttackNext = true;
    }

    public void ApplyDamage()
    {
        int playerLayerMask = 1 << LayerMask.NameToLayer("PLAYER");
        if (Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayerMask))
        {
            Debug.Log("NPC: Oyuncuya saldırı gerçekleştirildi!");
            if (target != null)
                target.GetComponent<PlayerController2D>().Die();
        }
        else
        {
            Debug.Log("NPC: Oyuncuya saldırı denemesi başarısız oldu.");
        }
    }

    public void ParryReceived()
    {
        animator.SetTrigger("EndAttack");
    }

    public override void GetDamage(float damage)
    {
        TakeDamage(damage);
    }
}
