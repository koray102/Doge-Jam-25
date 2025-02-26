using System.Threading;
using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class GameManagerScript : MonoBehaviour
{

    private Scene currentScene;
    public String afterSceneName;

    public Material TransitionMat;

    private float maskAmount = 1f;
    private float minTransition = -1f;
    private float maxTransition = 1f;


    private bool SeviyeGoreviBitti = false;
    public ParticleSystem duman;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        duman.Stop();

        TransitionMat.SetFloat("_MaskAmount", maskAmount);
        currentScene = SceneManager.GetActiveScene();

        StartCoroutine(SmoothLerp(minTransition, 1f));
    
        
    }

    // Update is called once per frame
    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.M)) 
        {
            SonrakiSeviye();
        }

    }

    public void SonrakiSeviye()
    {
        StartCoroutine(SmoothLerp(maxTransition, 1f));
        
        Invoke("SonrakiSeviyeGecis", 2f);
    }
    private void SonrakiSeviyeGecis()
    {
        SceneManager.LoadScene(afterSceneName);
    }

    public void SeviyeTekrari()
    {
        StartCoroutine(SmoothLerp(maxTransition, 0.2f));

        Invoke("SeviyeTekrariGecis", 3f);
    }
    private void SeviyeTekrariGecis()
    {
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    IEnumerator SmoothLerp(float targetValue, float speed)
    {
        float startValue = maskAmount;
        // Toplam ge�i� s�resi = |target - start| / speed
        float duration = Mathf.Abs(targetValue - startValue) / speed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // elapsed/duration, 0 ile 1 aras�nda gidip ge�i� oran�n� belirler.
            maskAmount = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            
            elapsed += Time.deltaTime;

            TransitionMat.SetFloat("_MaskAmount", maskAmount);

            yield return null; // Bir sonraki frame'e ge�
        }

        // Son ad�mda de�eri tam hedefe e�itliyoruz.
        maskAmount = targetValue;
        Debug.Log("Hedefe ula��ld�!");
    }

    public void OlumOldu()
    {
        if (SeviyeBittiMi())
        {
            SeviyeBittiEffectleriniAc();
        }
    }


    public bool SeviyeBittiMi()
    {
        string npcLayerName = "NPC";
        // NPC layer'ının indeksini alın
        int npcLayer = LayerMask.NameToLayer(npcLayerName);
        // Hierarchy'deki tüm GameObject'leri alın
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Parent objeleri tutmak için bir liste oluşturun
        List<GameObject> parentObjects = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Objeyi ve layer'ını kontrol edin
            if (obj.layer == npcLayer && obj.transform.parent == null)
            {
                // Eğer obje NPC layer'ında ve parent'ı yoksa, listeye ekleyin
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
    public void SeviyeBittiEffectleriniAc()
    {
        SeviyeGoreviBitti = true;
        duman.Play();
    }

}
