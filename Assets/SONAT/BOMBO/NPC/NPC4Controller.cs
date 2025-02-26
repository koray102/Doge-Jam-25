using Unity.VisualScripting;
using UnityEngine;

public class NPC4Controller : NPCBase
{
    public ParticleSystem MetalImpact;
    public AudioSource MetalImpactSource;
    public AudioClip MetalImpactClip;
    public Transform CarpismaTransformu;
    public bool KalkanSolEldeMi = false;

    // Animator referans� ekleniyor.
   

    protected override void Patrol()
    {
        // Sprite flip durumuna g�re kalkan�n sol elde olup olmad��� ayarlan�yor.
        if (gameObject.transform.localScale.x > 0)
            KalkanSolEldeMi = false;
        else
            KalkanSolEldeMi = true;

        // Patrol animasyonu tetikleniyor.
        

        if (patrolPoints.Length == 0)
            return;

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
    }

    protected override void ozelBaslangic()
    {

    }
    protected override void ChaseAndAttack()
    {

            if (gameObject.transform.localScale.x > 0)
                KalkanSolEldeMi = false;
            else
                KalkanSolEldeMi = true;

            float deltaX = player.position.x - transform.position.x;
            bool detected = IsPlayerDetected();

            if (detected && visibleKontrol.isVisible)
            {
                // Chase animasyonu tetikleniyor.
         

                chaseTimer = chaseMemoryTime;
                facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
                lastFacingDirection = facingDirection;

                if (Mathf.Abs(deltaX) > attackRange)
                {
                    Vector2 newPos = transform.position;
                    newPos.x = Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime);
                    transform.position = newPos;
                }
                else
                {
                    if (attackTimer <= 0f)
                    {
                        AttackPlayer();
                        attackTimer = attackCooldown;
                    }
                    else
                    {
                        attackTimer -= Time.deltaTime;
                    }
                }
            }
            else
            {
                chaseTimer -= Time.deltaTime;
                facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
                lastFacingDirection = facingDirection;

                if (chaseTimer > 0f && Mathf.Abs(deltaX) > attackRange && visibleKontrol.isVisible)
                {
                    Vector2 newPos = transform.position;
                    newPos.x = Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime);
                    transform.position = newPos;
                }else if(chaseTimer > 0 && visibleKontrol.isVisible)
                {

                }
                else
                {
                    state = NPCState.Patrol;
                }
            }

        


    }

    protected override void AttackPlayer()
    {
        // Attack animasyonu tetikleniyor.
        TriggerAttackAnimation();
      
        Debug.Log("NPC1: Player'a sald�r�ld�!");


        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        foreach (Collider2D obj in hitObjects)
        {

            if (obj.gameObject.layer == LayerMask.NameToLayer("Character"))
            {
                // Diğer objeler için, örneğin enemy varsa hasar verelim:
                PlayerController2D playerScript = obj.GetComponent<PlayerController2D>();
                if (playerScript != null)
                {
                    playerScript.Die();
                }
            }
        }
    }

    public override void GetDamage()
    {
        // E�er oyuncu, NPC'nin bakt��� y�nde de�ilse (shield aktif de�ilse) �lecek.
        if (!((!KalkanSolEldeMi && player.position.x > transform.position.x) ||
              (KalkanSolEldeMi && player.position.x < transform.position.x)))
        {
            gameManagerScript.OlumOldu();
            Destroy(gameObject);
        }
        else
        {
            if(MetalImpactSource != null && MetalImpactClip != null)
                MetalImpactSource.PlayOneShot(MetalImpactClip);

            Instantiate(MetalImpact, CarpismaTransformu.position, Quaternion.identity);
        }
    }


    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
