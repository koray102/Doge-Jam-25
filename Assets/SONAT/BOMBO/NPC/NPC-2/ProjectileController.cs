using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float rotationSpeed = 360f;        // Havada d�n�� h�z� (derece/saniye)
    public float lifetimeAfterImpact = 60f;     // �arp��ma sonras� yok olma s�resi (saniye)

    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasImpacted = false;

    public LayerMask OyuncuLayer;

    private PlayerController2D player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj.GetComponent<PlayerController2D>();
    }

    void Update()
    {
        // �arp��ma ger�ekle�memi�se havada s�rekli d�ns�n
        if (!hasImpacted)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            player.Die();
        }
        if (!hasImpacted)
        {
            hasImpacted = true;
            // Hareketi durdur
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            
            // Collider'� kapatarak di�er objelere �arpmamas�n� sa�la
            if (col != null)
                col.enabled = false;
            // 1 dakika sonra yok olsun
            Destroy(gameObject, lifetimeAfterImpact);
        }
    }
}
