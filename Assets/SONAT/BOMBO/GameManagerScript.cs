using System.Threading;
using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class GameManagerScript : MonoBehaviour
{

    private Scene currentScene;
    public String afterSceneName;

    public Material TransitionMat;

    private float maskAmount = 1f;
    private float minTransition = -1f;
    private float maxTransition = 1f;

    public SeviyeTamamlanmaControll SeviyeTamamlanmaControll;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        if (SeviyeTamamlanmaControll.SeviyeBittiMi())
        {
            SeviyeTamamlanmaControll.SeviyeBittiEffectleriniAc();
        }
    }

    
}
