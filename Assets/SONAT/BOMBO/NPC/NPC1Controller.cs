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
    private Coroutine attackCoroutine;

    [Header("Parry Settings")]
    [SerializeField] private float stunAfterParryDuration;
    [SerializeField] private float stunPlayerDuration;
    private bool didParried;

    // Base sınıftaki attackDuration, attackCooldown ve attackHitDelay kullanılmaktadır.


    protected override void Update()
    {
        // NPCBase'deki Update metodu çalışsın
        base.Update();

        if(isAttacking && !isFakeAttack) // Normal saldiri yapiyorsa
        {
            Debug.Log("Attacking");

            if(player.gameObject.GetComponent<PlayerController2D>()._isAttacking)
            {
                StartCoroutine(ParryReceived());
            }
        }else if (isAttacking && isFakeAttack)
        {
            Debug.Log("Faking");

            if(player.gameObject.GetComponent<PlayerController2D>()._isAttacking)
            {
                StartCoroutine(StunPlayer());
            }
        }
    }


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

        if(isAttacking || isFakeAttack || didParried)
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
        attackCoroutine = StartCoroutine(AttackSequence());
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

    public IEnumerator ParryReceived()
    {
        Debug.Log("Attack Parried");
        didParried = true;
        if(attackCoroutine != null) StopCoroutine(attackCoroutine);

        CancelInvoke("ApplyDamage");
        isAttacking = false;
        isFakeAttack = false;

        animator.SetTrigger("EndAttack");

        yield return new WaitForSeconds(stunAfterParryDuration);
        canAttack = true;
        didParried = false;
    }

    public IEnumerator StunPlayer()
    {
        Debug.Log("Player Faked");
        player.gameObject.GetComponent<PlayerController2D>().isStunned = true;

        if(attackCoroutine != null) StopCoroutine(attackCoroutine);
        
        isAttacking = false;
        isFakeAttack = false;
        animator.SetTrigger("EndAttack");

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        
        yield return new WaitForSeconds(stunPlayerDuration - attackCooldown);
        player.gameObject.GetComponent<PlayerController2D>().isStunned = false;
    }

    public override void GetDamage(float damage)
    {
        TakeDamage(damage);
    }
}
