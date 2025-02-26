using System.Collections.Generic;
using UnityEngine;

public class SeviyeTamamlanmaControll : MonoBehaviour
{   
    private bool SeviyeGoreviBitti= false;
    public GameManagerScript gameManagerScript;
    public ParticleSystem duman;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        duman.Stop();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool SeviyeBittiMi()
    {
        string npcLayerName = "NPC";
        // NPC layer'ýnýn indeksini alýn
        int npcLayer = LayerMask.NameToLayer(npcLayerName);
        // Hierarchy'deki tüm GameObject'leri alýn
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Parent objeleri tutmak için bir liste oluþturun
        List<GameObject> parentObjects = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Objeyi ve layer'ýný kontrol edin
            if (obj.layer == npcLayer && obj.transform.parent == null)
            {
                // Eðer obje NPC layer'ýnda ve parent'ý yoksa, listeye ekleyin
                parentObjects.Add(obj);
            }
        }
        Debug.Log(parentObjects.Count);
        
        if (parentObjects.Count == 1 || parentObjects.Count < 1)
        {

            SeviyeBittiEffectleriniAc();
            return true;
        }
        else
        {
            return false;
        }
        

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(SeviyeGoreviBitti && collision.CompareTag("Player"))
        {
            gameManagerScript.SonrakiSeviye();
        }
    }

    public void SeviyeBittiEffectleriniAc()
    {
        SeviyeGoreviBitti = true;
        duman.Play();
    }


}
