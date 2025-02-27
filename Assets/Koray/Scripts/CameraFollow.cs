using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Oyuncunun transform'u
    public float minX;        // Haritanın sol sınırı
    public float maxX;        // Haritanın sağ sınırı

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPos = transform.position;
            // Sadece x ekseninde oyuncuyu takip ediyoruz
            newPos.x = Mathf.Clamp(player.position.x, minX, maxX);
            transform.position = newPos;
        }
    }
}
