using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LightFlicker : MonoBehaviour
{
    [Header("Flicker Ayarları")]
    public float minIntensity = 0.5f;   // Işığın alabileceği en düşük parlaklık
    public float maxIntensity = 1f;     // Işığın alabileceği en yüksek parlaklık
    public float flickerInterval = 0.1f; // Her titreme arasında bekleme süresi

    private Light2D light2D;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
    }

    private void OnEnable()
    {
        StartCoroutine(Flicker());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            // Işığın intensitesini min ve max arasında rastgele ayarla
            light2D.intensity = Random.Range(minIntensity, maxIntensity);
            yield return new WaitForSeconds(flickerInterval);
        }
    }
}
