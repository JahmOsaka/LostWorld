using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class GameFeelManager : MonoBehaviour
{
    public static GameFeelManager Instance;

    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }
    public void TriggerHitstop(float duration)
    {
        StartCoroutine(HitstopRoutine(duration));
    }

    private IEnumerator HitstopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    public void TriggerShake(float duration, float magnitude, Transform cameraTransform = null)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulseWithForce(magnitude);
        }
        else if (cameraTransform != null)
        {
            StartCoroutine(ShakeRoutine(duration, magnitude, cameraTransform));
        }
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude, Transform camTransform)
    {
        Vector3 originalPos = camTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            camTransform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        camTransform.localPosition = originalPos;
    }
}