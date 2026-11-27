using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HazardPitZone : MonoBehaviour
{
    [Header("Damage & I-Frame")]
    [Tooltip("Damage dealt to the player upon falling into this hazard (bypasses normal i-frames).")]
    [SerializeField] private int hazardDamage = 15;
    [Tooltip("Invincibility duration granted after respawning on safe ground.")]
    [SerializeField] private float iFrameDurationAfterRespawn = 1.2f;

    [Header("Spirit Wisp Return Feel")]
    [Tooltip("Prefab of the smoke/spirit wisp (e.g., ParticleSystem + TrailRenderer) that flies back to safe ground.")]
    [SerializeField] private GameObject spiritWispPrefab;
    [Tooltip("Optional VFX spawned when the player dissolves into spirit form.")]
    [SerializeField] private GameObject dissolveBurstVFX;
    [Tooltip("Optional VFX spawned when the player materializes back on safe ground (uses dissolveBurstVFX if empty).")]
    [SerializeField] private GameObject reappearBurstVFX;
    [Tooltip("Extra time to let the wisp's trail/particles fade out naturally after arriving.")]
    [SerializeField] private float wispFadeOutDelay = 0.6f;
    [Tooltip("Duration in seconds for the spirit wisp to fly back to safe ground.")]
    [SerializeField] private float returnDuration = 0.4f;
    [Tooltip("Extra upward arc height while returning to clear cliff edges.")]
    [SerializeField] private float returnArcHeight = 1.4f;
    [Tooltip("Impact pause in seconds before dissolving into the spirit wisp.")]
    [SerializeField] private float warpDelay = 0.12f;

    [Header("Impact Juice")]
    [Tooltip("Hitstop duration triggered on hazard impact.")]
    [SerializeField] private float hitstopDuration = 0.06f;
    [Tooltip("Camera shake duration on hazard impact.")]
    [SerializeField] private float shakeDuration = 0.15f;
    [Tooltip("Camera shake intensity on hazard impact.")]
    [SerializeField] private float shakeMagnitude = 0.12f;

    [Header("Optional Override")]
    [Tooltip("Optional. If left empty, automatically returns the player to their last safe ground position.")]
    [SerializeField] private Transform overrideRespawnPoint;

    private static bool isRespawningPlayer;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnDisable()
    {
        isRespawningPlayer = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTriggerHazard(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryTriggerHazard(other);
    }

    private void TryTriggerHazard(Collider2D other)
    {
        if (isRespawningPlayer || !other.CompareTag("Player")) return;

        Player player = other.GetComponentInParent<Player>() ?? other.GetComponent<Player>();
        CoreHealth health = other.GetComponentInParent<CoreHealth>() ?? other.GetComponent<CoreHealth>();
        if (player == null || health == null || health.currentHealth <= 0) return;

        StartCoroutine(HazardRespawnRoutine(player, health));
    }

    private IEnumerator HazardRespawnRoutine(Player player, CoreHealth health)
    {
        isRespawningPlayer = true;

        bool died = health.TakeHazardDamage(hazardDamage, iFrameDurationAfterRespawn);

        if (GameFeelManager.Instance != null)
        {
            if (hitstopDuration > 0f) GameFeelManager.Instance.TriggerHitstop(hitstopDuration);
            if (Camera.main != null && shakeDuration > 0f)
            {
                GameFeelManager.Instance.TriggerShake(shakeDuration, shakeMagnitude, Camera.main.transform);
            }
        }

        if (died)
        {
            isRespawningPlayer = false;
            yield break;
        }

        Rigidbody2D rb = player.RB != null ? player.RB : player.GetComponent<Rigidbody2D>();
        RigidbodyType2D originalBodyType = rb != null ? rb.bodyType : RigidbodyType2D.Dynamic;
        RigidbodyInterpolation2D originalInterpolation = rb != null ? rb.interpolation : RigidbodyInterpolation2D.None;
        Transform moveTarget = rb != null ? rb.transform : player.transform;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.interpolation = RigidbodyInterpolation2D.None;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        player.enabled = false;

        if (warpDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(warpDelay);
        }

        PlayerSensor sensor = player.GetComponent<PlayerSensor>() ?? player.GetComponentInChildren<PlayerSensor>();
        Vector3 targetPos = overrideRespawnPoint != null
            ? overrideRespawnPoint.position
            : (sensor != null && sensor.HasSafePosition ? sensor.LastSafePosition : moveTarget.position);

        Transform visualRoot = player.transform.Find("PlayerVisual") ?? player.transform.Find("Visual") ?? player.transform;
        SpriteRenderer[] renderers = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        bool[] originalRendererStates = new bool[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalRendererStates[i] = renderers[i].enabled;
            renderers[i].enabled = false;
        }

        Vector3 startPos = moveTarget.position;

        if (dissolveBurstVFX != null)
        {
            Instantiate(dissolveBurstVFX, startPos, Quaternion.identity);
        }

        GameObject wispInstance = spiritWispPrefab != null
            ? Instantiate(spiritWispPrefab, startPos, Quaternion.identity)
            : null;

        float duration = Mathf.Max(0.05f, returnDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = t * t * (3f - 2f * t);

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, easedT);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * returnArcHeight;

            moveTarget.position = currentPos;
            if (rb != null) rb.position = currentPos;
            if (wispInstance != null) wispInstance.transform.position = currentPos;

            yield return null;
        }

        moveTarget.position = targetPos;
        if (rb != null) rb.position = targetPos;
        if (wispInstance != null) wispInstance.transform.position = targetPos;
        Physics2D.SyncTransforms();

        if (wispInstance != null)
        {
            foreach (ParticleSystem ps in wispInstance.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            foreach (SpriteRenderer wispSr in wispInstance.GetComponentsInChildren<SpriteRenderer>())
            {
                wispSr.enabled = false;
            }
            Destroy(wispInstance, Mathf.Max(0.05f, wispFadeOutDelay));
        }

        GameObject arrivalVfx = reappearBurstVFX != null ? reappearBurstVFX : dissolveBurstVFX;
        if (arrivalVfx != null)
        {
            Instantiate(arrivalVfx, targetPos, Quaternion.identity);
        }

        player.StateMachine?.ChangeState(player.IdleState);
        Animator anim = player.GetComponentInChildren<Animator>();
        if (anim != null) anim.Update(0f);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].enabled = originalRendererStates[i];
        }

        if (rb != null)
        {
            rb.bodyType = originalBodyType;
            rb.interpolation = originalInterpolation;
            rb.linearVelocity = Vector2.zero;
        }

        player.enabled = true;

        yield return new WaitForFixedUpdate();
        isRespawningPlayer = false;
    }
}