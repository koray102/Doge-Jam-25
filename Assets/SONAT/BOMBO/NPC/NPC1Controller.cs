using UnityEngine;
using System.Collections;

public class NPC1Controller : NPCBase
{
    // Fake attack ayarları: zeka arttıkça fake saldırı olasılığı lineer artıyor (maksimum belirli bir değere kadar)
    public float fakeAttackDelay = 1.0f;
    public float fakeAttackMultiplier = 0.5f;  // Zekanın fake saldırı olasılığına etkisi (örneğin, intelligence 1 ise maksimum %50)
    public float maxFakeAttackChance = 0.5f;     // Maksimum fake saldırı olasılığı
    private bool forceRealAttackNext = false;    // Fake saldırı gerçekleşirse, sonraki saldırı gerçek olmak zorunda

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
                    // Zeka arttıkça hologramı fark etme olasılığı artıyor.
                    float detectionProbability = Mathf.Clamp01(intelligence);
                    if (Random.value < detectionProbability)
                    {
                        // Hologram algılandı: chase modu, holograma saldırı
                        chaseTimer = chaseMemoryTime;
                        facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
                        lastFacingDirection = facingDirection;
                        AttackHologram(hologram.transform);
                        holoScript.isDetected = true;
                        return;
                    }
                }
            }
            // Oyuncu algılanmıyorsa bile, chase süresi dolana kadar oyuncunun x koordinatına yönel.
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
            // Oyuncu normal olarak algılandıysa chase süresi sıfırlanır.
            chaseTimer = chaseMemoryTime;
            facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
            lastFacingDirection = facingDirection;
        }

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
        if (forceRealAttackNext)
        {
            TriggerAttackAnimation();
            Debug.Log("NPC1: Gerçek saldırı (Player) gerçekleştirildi!");
            forceRealAttackNext = false;
        }
        else
        {
            // Zekaya bağlı fake saldırı olasılığını hesapla (maksimum %50)
            float computedFakeChance = Mathf.Clamp(intelligence * fakeAttackMultiplier, 0f, maxFakeAttackChance);
            if (Random.value < computedFakeChance)
            {
                Debug.Log("NPC1: Fake saldırı gerçekleştiriliyor, bekleme süresi uygulanıyor...");
                StartCoroutine(FakeAttackCoroutine());
                return;
            }
            else
            {
                TriggerAttackAnimation();
                Debug.Log("NPC1: Gerçek saldırı (Player) gerçekleştirildi!");
            }
        }
        // Gerçek saldırıda oyuncuya hasar verme kodunu ekleyebilirsin.
    }

    private IEnumerator FakeAttackCoroutine()
    {
        yield return new WaitForSeconds(fakeAttackDelay);
        // Fake saldırı sonrası bir sonraki saldırı kesinlikle gerçek olacak.
        forceRealAttackNext = true;
    }

    // Holograma saldırı metodu: hedef olarak verilen hologram üzerine saldırı yapar.
    private void AttackHologram(Transform hologram)
    {
        TriggerAttackAnimation();
        Debug.Log("NPC1: Holograma saldırı gerçekleştirildi!");
        // Burada holograma hasar verme veya imha etme işlemleri eklenebilir.
    }

    public override void GetDamage(float damage)
    {
        TakeDamage(damage);
    }
}
