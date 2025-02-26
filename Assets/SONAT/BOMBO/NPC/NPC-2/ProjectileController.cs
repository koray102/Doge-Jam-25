using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
    public float rotationSpeed = 360f;        // Havada dönüş hızı (derece/saniye)
    public float lifetimeAfterImpact = 60f;     // Çarpışma sonrasında yok olma süresi (saniye)

    // Yeni prefab için eklenecek özellikler:
    public bool shouldPauseOnProximity = false; // Eğer true ise, player yaklaştığında bekleme özelliği aktif olur.
    public float pauseDistance = 5f;            // Player ile olan mesafe eşiği (birim)
    public float pauseDuration = 2f;            // Bekleme süresi (saniye)

    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasImpacted = false;
    private bool isPaused = false;
    private bool hasPaused = false;             // Daha önce bekleme yapıldı mı kontrolü
    private Vector2 storedVelocity;
    private Transform playerTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        // Çarpışma gerçekleşmediyse dönüş devam eder.
        if (!hasImpacted)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            // Eğer pause özelliği açıksa, daha önce beklenmediyse ve playerTransform mevcutsa
            if (shouldPauseOnProximity && !hasPaused && playerTransform != null)
            {
                float distance = Vector2.Distance(transform.position, playerTransform.position);
                if (distance <= pauseDistance && !isPaused)
                {
                    // Mevcut hızı sakla ve hareketi durdur
                    storedVelocity = rb.linearVelocity;
                    rb.linearVelocity = Vector2.zero;
                    isPaused = true;
                    StartCoroutine(PauseAndResume());
                }
            }
        }
    }

    IEnumerator PauseAndResume()
    {
        // Belirtilen süre bekle
        yield return new WaitForSeconds(pauseDuration);
        // Eski hızı geri yükle
        rb.linearVelocity = storedVelocity;
        isPaused = false;
        hasPaused = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            //player.Die();
        }
        if (!hasImpacted)
        {
            hasImpacted = true;
            // Hareketi durdur
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Collider'ı kapatarak diğer objelere çarpmasını engelle
            if (col != null)
                col.enabled = false;
            // Belirtilen süre sonra yok ol
            Destroy(gameObject, lifetimeAfterImpact);
        }
    }
}
