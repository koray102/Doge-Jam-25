using UnityEngine;

public class HologramScript : MonoBehaviour
{
    public bool isDetected = false;


    private ZekaManager zekaManager;
    private void Start()
    {
        zekaManager = FindFirstObjectByType<ZekaManager>();
    }

    public void Detect()
    {
        isDetected = true;
        zekaManager.IncreaseHologramIntelligence();
    }
}
