using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SoundAndSceneLoader : MonoBehaviour
{
    public AudioSource audioSource;   // Ses oynatacak AudioSource bileşeni
    public AudioSource audioSourceLoop;   // Ses oynatacak AudioSource bileşeni
    public AudioClip soundToPlay;       // Çalınacak ses klibi
    public string sceneToLoad;          // Yüklenecek sahne adı

    public void StartButton()
    {
        StartCoroutine(PlaySoundAndLoadScene());
    }

    private IEnumerator PlaySoundAndLoadScene()
    {
        if(audioSource != null && soundToPlay != null)
        {
            audioSourceLoop.mute = true;
            // Ses klibini oynatıyoruz.
            audioSource.PlayOneShot(soundToPlay);
            
            // Sesin süresi kadar bekliyoruz.
            yield return new WaitForSeconds(soundToPlay.length);
            
            // Ses tamamlandıktan sonra sahneyi yüklüyoruz.
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("AudioSource veya soundToPlay atanmadı!");
        }
    }
}
