using UnityEngine;
using System.Collections;

public class NPC1Controller : NPCBase
{
    // Fake attack ayarları: zeka arttıkça fake saldırı olasılığı lineer artıyor (maksimum belirli bir değere kadar)
    public float fakeAttackDelay = 1.0f;
    public float fakeAttackMultiplier = 0.5f;  // Örneğin, intelligence 1 ise maksimum %50
    public float maxFakeAttackChance = 0.5f;     // Maksimum fake saldırı olasılığı
    private bool forceRealAttackNext = false;    // Fake saldırı gerçekleşirse, sonraki saldırı gerçek olmak zorunda

    // Saldırı anı algılanması için bayraklar:
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public bool isFakeAttack = false;

    protected override void Patrol()
    {
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

    protected override void OzelBaslangic()
    {
        // NPC1 için ek başlangıç işlemleri (varsa)
    }

    protected override void ChaseAndAttack()
    {
        float deltaX = player.position.x - transform.position.x;
        bool playerDetected = IsPlayerDetected();

        // Eğer oyuncu algılanmıyorsa, hologram araması yapılacak:
        if (!playerDetected)
        {
            GameObject hologram = GameObject.FindGameObjectWithTag("Hologram");
            if (hologram != null)
            {
                // Hologramın bağlı olduğu script’in isDetected değeri kontrol ediliyor.
                HologramScript holoScript = hologram.GetComponent<HologramScript>();
                if (holoScript != null && !holoScript.isDetected)
                {
                    // Zeka arttıkça hologramı fark etme olasılığı artar.
                    float detectionProbability = Mathf.Clamp01(intelligence);
                    if (Random.value < detectionProbability)
                    {
                        // Hologram algılandı: NPC sanki oyuncuyu görmüş gibi chase moduna girip holograma saldırır.
                        chaseTimer = chaseMemoryTime;
                        facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
                        lastFacingDirection = facingDirection;
                        AttackHologram(hologram.transform);
                        holoScript.isDetected = true;
                        return;
                    }
                }
            }
            // Oyuncu algılanmıyorsa, chase süresi bitene kadar oyuncunun x koordinatına doğru ilerlemeye devam et.
            if (chaseTimer > 0f)
            {
                facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
                lastFacingDirection = facingDirection;
                Vector2 newPos = transform.position;
                newPos.x = Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime);
                transform.position = newPos;
                chaseTimer -= Time.deltaTime;
                return;
            }
            else
            {
                state = NPCState.Patrol;
                return;
            }
        }
        else
        {
            // Oyuncu normal algılandığında chase süresi resetlenir.
            chaseTimer = chaseMemoryTime;
            facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
            lastFacingDirection = facingDirection;
        }

        // Eğer oyuncu attackRange'e yakınsa, daha fazla yaklaşmaya gerek yok; sadece saldır.
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

    protected override void AttackPlayer()
    {
        // Saldırı anını algılamak için bayrak set ediliyor:
        isAttacking = true;

        if (forceRealAttackNext)
        {
            TriggerAttackAnimation();
            Debug.Log("NPC1: Gerçek saldırı (Player) gerçekleştirildi!");
            forceRealAttackNext = false;
            isFakeAttack = false;
            StartCoroutine(ResetAttackFlags());
        }
        else
        {
            // Zekaya bağlı fake saldırı olasılığı (maksimum %50)
            float computedFakeChance = Mathf.Clamp(intelligence * fakeAttackMultiplier, 0f, maxFakeAttackChance);
            if (Random.value < computedFakeChance)
            {
                isFakeAttack = true;
                Debug.Log("NPC1: Fake saldırı gerçekleştiriliyor, bekleme süresi uygulanıyor...");
                StartCoroutine(FakeAttackCoroutine());
                StartCoroutine(ResetAttackFlags());
                return;
            }
            else
            {
                TriggerAttackAnimation();
                Debug.Log("NPC1: Gerçek saldırı (Player) gerçekleştirildi!");
                isFakeAttack = false;
                StartCoroutine(ResetAttackFlags());
            }
        }
        // Gerçek saldırıda oyuncuya hasar verme kodlarını buraya ekleyebilirsin.
    }

    private IEnumerator FakeAttackCoroutine()
    {
        yield return new WaitForSeconds(fakeAttackDelay);
        // Fake saldırı sonrası sonraki saldırı kesinlikle gerçek olacak.
        forceRealAttackNext = true;
    }

    // Saldırı bayraklarını kısa bir süre sonra resetler (örneğin, animasyon süresine göre ayarlanabilir)
    private IEnumerator ResetAttackFlags()
    {
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
        isFakeAttack = false;
    }

    // Holograma saldırı metodu: hedef olarak verilen hologram üzerine saldırı yapar.
    private void AttackHologram(Transform hologram)
    {
        TriggerAttackAnimation();
        Debug.Log("NPC1: Holograma saldırı gerçekleştirildi!");
        // Holograma hasar verme veya imha etme işlemleri eklenebilir.
    }

    public override void GetDamage(float damage)
    {
        TakeDamage(damage);
    }
}
