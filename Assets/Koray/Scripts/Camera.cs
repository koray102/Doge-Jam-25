using UnityEngine;
using System.Collections;

public class Camera : MonoBehaviour
{
    // Bu metodu istediğiniz an çağırarak kamerayı sarsabilirsiniz.
    // duration: sarsıntı süresi, magnitude: sarsıntı yoğunluğu
    public static IEnumerator Shake(float duration, float magnitude)
    {
        GameObject mainCam = GameObject.Find("Main Camera");

        Vector3 originalPos = mainCam.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            mainCam.transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.localPosition = originalPos;
    }
}
