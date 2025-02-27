using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZekaManager : MonoBehaviour
{
    // Public prefab referanslarý; Unity Editor üzerinden atanabilir
    public GameObject prefab1;
    public GameObject prefab2;

    private NPC1Controller npc1;
    private NPC2Controller npc2;

    private List<GameObject> npcObjects = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        npc1 = prefab1.GetComponent<NPC1Controller>();
        npc2 = prefab2.GetComponent<NPC2Controller>();

        int npcLayer = LayerMask.NameToLayer("NPC");
        if (npcLayer == -1)
        {
            Debug.LogError("NPC layer bulunamadý. Lütfen 'NPC' adýnda bir layer oluþturun.");
            return;
        }

        // Sahnedeki root objeleri alýp, recursive olarak NPC layer'ýndaki objeleri topluyoruz
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
            CollectNPCObjects(root, npcLayer);
        }

    }
    void CollectNPCObjects(GameObject obj, int npcLayer)
    {
        if (obj.layer == npcLayer)
        {
            npcObjects.Add(obj);
        }

        foreach (Transform child in obj.transform)
        {
            CollectNPCObjects(child.gameObject, npcLayer);
        }
    }

    void Update()
    {
        // Eðer oyun esnasýnda bir NPC yok olduysa (destroy edildiyse), listenin referansýný temizle
        npcObjects.RemoveAll(npc => npc == null);
    }

    // Hologram zekasýný arttýran metot
    public void HologramZekaArttir()
    {
        // Hologram zekasýnýn arttýrýlmasýna yönelik kod buraya gelecek
        npc1.hologramRealizationIntelligent += 0.04f;
        Guncelle();
    }

    // Fake zekasýný arttýran metot
    public void FakeZekaArttir()
    {
        // Fake zekasýnýn arttýrýlmasýna yönelik kod buraya gelecek
        npc1.fakeAttackIntelligence += 0.04f;
        Guncelle();
    }

    // Projectile zekasýný arttýran metot
    public void Projectilezekaarttir()
    {
        npc2.zeka += 0.03f;
        Guncelle();
    }

    public void Guncelle()
    {
        foreach (GameObject npc in npcObjects)
        {
            if (npc != null)
            {
                if (npc.CompareTag("NPC-1"))
                {
                    NPC1Controller tempNPC1 = npc.GetComponent<NPC1Controller>();

                    tempNPC1.hologramRealizationIntelligent = npc1.hologramRealizationIntelligent;
                    tempNPC1.fakeAttackIntelligence = npc1.fakeAttackIntelligence;
                }
                else if (npc.CompareTag("NPC-2"))
                {
                    NPC2Controller tempNPC2 = npc.GetComponent<NPC2Controller>();
                    tempNPC2.zeka = npc2.zeka;
                }
                
            }
        }


    }
}
