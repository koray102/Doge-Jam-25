using Unity.VisualScripting;
using UnityEngine;

public class OlumBolgesi : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    PlayerController2D player;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController2D>();
    }

    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player.Die();
        }
    }
}
