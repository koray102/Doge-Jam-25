using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    internal enum soundType {
        Attack1,
        Attack2,
        Attack3,
        Dash,
        HitEnemy,
        Death,
        Perry
    }

    [SerializeField] private List<AudioClip> soundList;
    private static SoundManager soundManagerInstance;
    private AudioSource audioSource;
    

    private void Awake()
    {
        soundManagerInstance = this;
    }


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    internal static void PlaySound(soundType sound, float volume = 1)
    {
        int index = (int)sound;
        if (soundManagerInstance != null && soundManagerInstance.soundList != null && 
            index >= 0 && index < soundManagerInstance.soundList.Count)
        {
            soundManagerInstance.audioSource.PlayOneShot(soundManagerInstance.soundList[index], volume);
        }
    }
}
