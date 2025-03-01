using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZekaManager : MonoBehaviour
{
    // Public prefab referansları; Unity Editor üzerinden atanabilir
    public GameObject prefab1;
    public GameObject prefab2;

    private NPC1Controller npc1;
    private NPC2Controller npc2;

    private List<GameObject> npcObjects = new List<GameObject>();

    void Start()
    {
        npc1 = prefab1.GetComponent<NPC1Controller>();
        npc2 = prefab2.GetComponent<NPC2Controller>();

        int npcLayer = LayerMask.NameToLayer("NPC");
        if (npcLayer == -1)
        {
            Debug.LogError("NPC layer bulunamadı. Lütfen 'NPC' adında bir layer oluşturun.");
            return;
        }

        // Sahnedeki root objeleri alıp, recursive olarak NPC layer'indeki objeleri topluyoruz
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
        // Eğer oyun esnasında bir NPC yok olduysa (destroy edildiyse), listenin referansını temizle
        npcObjects.RemoveAll(npc => npc == null);
    }

    // Hologram intelligence'ını arttıran metot
    public void IncreaseHologramIntelligence()
    {
        // Hologram intelligence'ının arttırılmasına yönelik kod buraya gelecek
        npc1.hologramRealizationIntelligence += 0.04f;
        UpdateNPCStats();
    }

    // Fake attack intelligence'ını arttıran metot
    public void IncreaseFakeAttackIntelligence()
    {
        npc1.fakeAttackIntelligence += 0.04f;
        UpdateNPCStats();
    }

    // Projectile intelligence'ını arttıran metot
    public void IncreaseProjectileIntelligence()
    {
        npc2.intelligence += 0.03f;
        UpdateNPCStats();
    }

    public void UpdateNPCStats()
    {
        foreach (GameObject npc in npcObjects)
        {
            if (npc != null)
            {
                if (npc.CompareTag("NPC-1"))
                {
                    NPC1Controller tempNPC1 = npc.GetComponent<NPC1Controller>();
                    tempNPC1.hologramRealizationIntelligence = npc1.hologramRealizationIntelligence;
                    tempNPC1.fakeAttackIntelligence = npc1.fakeAttackIntelligence;
                }
                else if (npc.CompareTag("NPC-2"))
                {
                    NPC2Controller tempNPC2 = npc.GetComponent<NPC2Controller>();
                    tempNPC2.intelligence = npc2.intelligence;
                }
            }
        }
    }
}
