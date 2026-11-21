using System.Collections;
using UnityEngine;

public enum CastFeedbackType
{
    Throw,
    Pulse
}

public class PlayerCastFeedback : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The child transform holding the player's sprite visual.")]
    [SerializeField] private Transform visualTransform;
    [Tooltip("Optional small particle prefab spawned at the player when casting.")]
    [SerializeField] private GameObject castBurstPrefab;

    [Header("Throw Feel (Pea Shooting, Bomb Head)")]
    [Tooltip("Forward lean angle in degrees when throwing a projectile.")]
    [SerializeField] private float throwTiltAngle = 12f;
    [Tooltip("Squash and stretch multiplier applied on throw.")]
    [SerializeField] private Vector2 throwScale = new Vector2(1.18f, 0.86f);
    [Tooltip("Recovery duration in seconds for the throw pose.")]
    [SerializeField] private float throwRecoveryDuration = 0.14f;

    [Header("Pulse Feel (Orbit, Buffs, Summons)")]
    [Tooltip("Vertical stretch multiplier applied when activating a buff or summon.")]
    [SerializeField] private Vector2 pulseScale = new Vector2(0.84f, 1.22f);
    [Tooltip("Recovery duration in seconds for the pulse pose.")]
    [SerializeField] private float pulseRecoveryDuration = 0.18f;
    [Tooltip("Subtle camera shake duration on pulse cast.")]
    [SerializeField] private float pulseShakeDuration = 0.08f;
    [Tooltip("Subtle camera shake magnitude on pulse cast.")]
    [SerializeField] private float pulseShakeMagnitude = 0.06f;

    private Player player;
    private Vector3 baseAbsScale = Vector3.one;
    private Coroutine castCoroutine;

    private void Awake()
    {
        player = GetComponent<Player>();

        if (visualTransform == null)
        {
            visualTransform = transform.Find("PlayerVisual") ?? transform.Find("Visual");
        }

        if (visualTransform != null)
        {
            Vector3 s = visualTransform.localScale;
            baseAbsScale = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), s.z);
        }
    }

    public void TriggerCast(CastFeedbackType type)
    {
        if (!gameObject.activeInHierarchy || visualTransform == null) return;

        if (castBurstPrefab != null)
        {
            GameObject fx = Instantiate(castBurstPrefab, visualTransform.position, Quaternion.identity);
            Destroy(fx, 0.5f);
        }

        if (type == CastFeedbackType.Pulse && GameFeelManager.Instance != null && Camera.main != null)
        {
            GameFeelManager.Instance.TriggerShake(pulseShakeDuration, pulseShakeMagnitude, Camera.main.transform);
        }

        if (castCoroutine != null) StopCoroutine(castCoroutine);
        castCoroutine = StartCoroutine(CastPoseRoutine(type));
    }

    private IEnumerator CastPoseRoutine(CastFeedbackType type)
    {
        float facing = (player != null) ? player.FacingDirection : 1f;

        float startZ = (type == CastFeedbackType.Throw) ? -facing * throwTiltAngle : 0f;
        Vector2 startScale = (type == CastFeedbackType.Throw) ? throwScale : pulseScale;
        float duration = (type == CastFeedbackType.Throw) ? throwRecoveryDuration : pulseRecoveryDuration;

        ApplyVisualPose(startZ, startScale.x, startScale.y);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            float currentZ = Mathf.Lerp(startZ, 0f, ease);
            float multX = Mathf.Lerp(startScale.x, 1f, ease);
            float multY = Mathf.Lerp(startScale.y, 1f, ease);

            ApplyVisualPose(currentZ, multX, multY);
            yield return null;
        }

        ApplyVisualPose(0f, 1f, 1f);
        castCoroutine = null;
    }

    private void ApplyVisualPose(float zRotation, float multX, float multY)
    {
        if (visualTransform == null) return;

        float signX = visualTransform.localScale.x < 0f ? -1f : 1f;
        visualTransform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        visualTransform.localScale = new Vector3(
            baseAbsScale.x * multX * signX,
            baseAbsScale.y * multY,
            baseAbsScale.z
        );
    }

    private void OnDisable()
    {
        ApplyVisualPose(0f, 1f, 1f);
        castCoroutine = null;
    }
}