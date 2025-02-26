using UnityEngine;

public class ThrowFlower : MonoBehaviour
{
    public GameObject projectilePrefab;   // Fırlatılacak prefab
    public Transform firePoint;             // Fırlatma noktası (örneğin karakterin elinin bulunduğu nokta)
    public float projectileSpeed = 10f;     // Fırlatma hızı


    void Start()
    {
        
    }


    void Update()
    {
        
    }


    public void FireProjectile()
    {
        // Prefab'i firePoint konumunda instantiate ediyoruz.
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // Karakterin baktığı yönü belirleyelim (örneğin, scale.x pozitifse sağ, negatifse sol)
        float direction = Mathf.Sign(transform.localScale.x);
        
        // Eğer projectile'da Rigidbody2D varsa, ona hız atayarak fırlatalım:
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if(rb != null)
        {
            rb.linearVelocity = new Vector2(direction * projectileSpeed, 0f);
        }
    }
}
