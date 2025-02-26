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


}
