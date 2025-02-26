using UnityEngine;

public class KalkanScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Brake()
    {
        Destroy(gameObject, 0.1f);
    }
}
