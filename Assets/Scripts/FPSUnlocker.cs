using UnityEngine;

public class FPSUnlocker : MonoBehaviour
{
    void Awake()
    {
        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = 360;
    }
}