using UnityEngine;
using System.Collections;

public class NPC1Controller : NPCBase
{
    [Header("NPC-1 Özel Ayarlar")]
    // Devriye noktaları (sadece NPC1 kullanıyor)
    public Transform[] patrolPoints;
    protected int currentPatrolIndex = 0;
    public float patrolSpeed = 2f;

    // Fake attack ve zeka ile ilgili ayarlar
    public float fakeAttackDelay = 1f; // Fake saldırı sırasında bekleme süresi
    public float intelligenceThresholdForFakeAttack = 50f; // Fake saldırının devreye gireceği zeka eşiği
    public float fakeAttackProbability = 0.5f; // Fake saldırı olasılığı

    [Header("Hologram Ayarları")]
    public float hologramDetectionRange = 5f; // Hologramı algılama mesafesi
    public float hologramIntelligenceThreshold = 50f; // Zekası yüksekse; holograma saldırdıktan sonra tekrar saldırmama

    private bool justPerformedFakeAttack = false;
    private bool hologramAttacked = false;

    protected override void OzelBaslangic()
    {
        // İsteğe bağlı ek başlangıç işlemleri
    }

    protected override void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        if (Vector2.Distance(transform.position, targetPoint.position) > 2f)
        {
            // Hedefe doğru yön belirle (orijinal algoritma korunuyor)
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

    protected override void ChaseAndAttack()
    {
        float deltaX = player.position.x - transform.position.x;
        bool detected = IsPlayerDetected();

        if (detected)
        {
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
                // Eğer daha önce fake attack yaptıysak, sonraki saldırı normal olmalı.
                if (justPerformedFakeAttack)
                {
                    AttackPlayer();
                    attackTimer = attackCooldown;
                    justPerformedFakeAttack = false;
                }
                else if (attackTimer <= 0f)
                {
                    // Eğer zeka eşiğinin üzerindeyse ve olasılık uymuşsa fake attack yap.
                    if (intelligence >= intelligenceThresholdForFakeAttack && Random.value < fakeAttackProbability)
                    {
                        StartCoroutine(FakeAttackRoutine());
                        justPerformedFakeAttack = true;
                    }
                    else
                    {
                        AttackPlayer();
                        attackTimer = attackCooldown;
                    }
                }
                else
                {
                    attackTimer -= Time.deltaTime;
                }
            }
        }
        else
        {
            // Oyuncu algılanmıyorsa hologramı kontrol et.
            GameObject hologram = FindHologram();
            if (hologram != null)
            {
                if (intelligence >= hologramIntelligenceThreshold)
                {
                    if (!hologramAttacked)
                    {
                        AttackHologram(hologram);
                        hologramAttacked = true;
                    }
                }
                else
                {
                    AttackHologram(hologram);
                }
            }
            else
            {
                chaseTimer -= Time.deltaTime;
                if (chaseTimer <= 0f)
                    state = NPCState.Patrol;
            }
        }
    }

    IEnumerator FakeAttackRoutine()
    {
        TriggerAttackAnimation();
        Debug.Log("NPC1: Fake Attack başladı.");
        yield return new WaitForSeconds(fakeAttackDelay);
        Debug.Log("NPC1: Fake Attack tamamlandı.");
    }

    void AttackHologram(GameObject hologram)
    {
        TriggerAttackAnimation();
        Debug.Log("NPC1: Hologram (" + hologram.name + ") saldırısı gerçekleştirildi.");
        // İsteğe bağlı: holograma hasar verme kodu eklenebilir.
    }

    protected override void AttackPlayer()
    {
        TriggerAttackAnimation();
        Debug.Log("NPC1: Player'a saldırı gerçekleştirildi!");

        // Melee saldırı: attackPoint çevresindeki objelere (örneğin, Character layer) hasar verme
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        foreach (Collider2D obj in hitObjects)
        {
            if (obj.gameObject.layer == LayerMask.NameToLayer("Character"))
            {
                CharacterController playerScript = obj.GetComponent<CharacterController>();
                if (playerScript != null)
                {
                    // Örneğin: playerScript.TakeDamage(attackDamage);
                }
            }
        }
    }

    public override void GetDamage()
    {
        // Örnek olarak 10 hasar veriliyor; bu değeri ihtiyaca göre ayarlayın.
        TakeDamage(10f);
    }

    // Hologramı bulan yardımcı metot.
    GameObject FindHologram()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, hologramDetectionRange);
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Hologram"))
                return col.gameObject;
        }
        return null;
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
