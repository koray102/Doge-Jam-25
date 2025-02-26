using UnityEngine;

public class DestroyGameObject : MonoBehaviour
{
    [SerializeField] private float destroyTime = 10f;


    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
