using UnityEngine;

public class NPC3Controller : NPCBase
{
    // NPC-3: Bu NPC sabit durur, sadece oyuncu sald�r� menziline geldi�inde sald�r�r.

    protected override void Patrol()
    {
        // NPC-3, devriye (patrol) s�ras�nda hareket etmez.
        // Sadece oyuncunun x konumuna g�re y�z�n� ayarlar.
        float deltaX = player.position.x - transform.position.x;
        facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
        lastFacingDirection = facingDirection;
        // Hareket etmiyor; pozisyon sabit kal�yor.
    }
    protected override void ozelBaslangic()
    {

    }
    protected override void ChaseAndAttack()
    {   

        // NPC-3, chase moduna ge�ti�inde da ayn� yerde kal�r.
        // Yaln�zca oyuncunun x pozisyonuna g�re y�z�n� ayarlar ve
        // e�er oyuncu attackRange i�inde ise sald�r�r.
        float deltaX = player.position.x - transform.position.x;
        float absDeltaX = Mathf.Abs(deltaX);

        facingDirection = (deltaX >= 0) ? Vector2.right : Vector2.left;
        lastFacingDirection = facingDirection;

        if (absDeltaX <= attackRange)
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
        // E�er oyuncu menzile girmemi�se, NPC sadece y�z�n� oyuncuya d�nd�r�r.
    }

    protected override void AttackPlayer()
    {
        TriggerAttackAnimation();
        Debug.Log("NPC-3: Player'a sald�r�ld�!");
        // Buraya oyuncuya hasar verme veya sald�r� animasyonu eklenebilir.

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
