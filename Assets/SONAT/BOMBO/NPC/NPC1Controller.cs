using UnityEngine;

public class NPC1Controller : NPCBase
{
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

    protected override void ozelBaslangic()
    {
    }
    protected override void ChaseAndAttack()
    {

                // NPC ile oyuncu aras�ndaki yatay mesafeyi hesapla
            float deltaX = player.position.x - transform.position.x;

            // Oyuncunun tespit edilip edilmedi�ini kontrol et
            bool detected = IsPlayerDetected();

            if (detected)
            {
            
                // Chase zamanlay�c�s�n� s�f�rla
                chaseTimer = chaseMemoryTime;

            // NPC'nin bak�� y�n�n� belirle

                facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
                lastFacingDirection = facingDirection;

                // E�er oyuncu sald�r� menzilinin d���ndaysa, ancak menzile yak�nsa, NPC'yi oyuncunun x pozisyonuna do�ru hareket ettir
                if (Mathf.Abs(deltaX) > attackRange)
                {
                    Vector2 newPos = transform.position;
                    newPos.x = Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime);
                    transform.position = newPos;
                }
                else
                {
                    // Sald�r� menzilindeyse, sald�r� zamanlay�c�s�n� kontrol et
                    if (attackTimer <= 0f)
                    {
                        // Sald�r�y� ger�ekle�tir
                        AttackPlayer();
                        // Sald�r� sonras� bekleme s�resini ayarla
                        attackTimer = attackCooldown;
                    }
                    else
                    {
                        // Sald�r� zamanlay�c�s�n� azalt
                        attackTimer -= Time.deltaTime;
                    }
                }
            }
            else
            {
                // Chase zamanlay�c�s�n� azalt
                chaseTimer -= Time.deltaTime;

                // NPC'nin bak�� y�n�n� belirle
                facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
                lastFacingDirection = facingDirection;

                // Chase zamanlay�c�s� s�f�rdan b�y�kse, NPC'yi oyuncunun son bilinen x pozisyonuna do�ru hareket ettir
                if (chaseTimer > 0f && Mathf.Abs(deltaX) > attackRange)
                {
                    Vector2 newPos = transform.position;
                    newPos.x = Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime);
                    transform.position = newPos;
                }
                else if (chaseTimer > 0f)
                {

                }
                else
                {
                    // Chase zamanlay�c�s� s�f�ra ula�t�ysa, devriye moduna geri d�n
                    state = NPCState.Patrol;
                }
            }
        

    }

    protected override void AttackPlayer()
    {
        TriggerAttackAnimation();
        Debug.Log("NPC1: Player'a sald�r�ld�!");

        // Attack alanındaki tüm objeleri alıyoruz (layer filtresi uygulamıyoruz ki hem enemy hem de bullet kontrol edilebilsin)
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        foreach (Collider2D obj in hitObjects)
        {

            if (obj.gameObject.layer == LayerMask.NameToLayer("Character"))
            {
                // Diğer objeler için, örneğin enemy varsa hasar verelim:
                CharacterController playerScript = obj.GetComponent<CharacterController>();
                if (playerScript != null)
                {
                    //playerScript.Die();
                }
            }
        }

    }



    public override void GetDamage()
    {
        gameManagerScript.OlumOldu();
        Destroy(gameObject);
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
