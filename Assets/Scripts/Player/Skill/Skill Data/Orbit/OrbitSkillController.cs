using System.Collections.Generic;
using UnityEngine;

public class OrbitSkillController : MonoBehaviour
{
    public Transform Owner { get; private set; }

    private OrbitSkillData data;
    private readonly List<OrbitOrb> orbs = new List<OrbitOrb>();
    private float currentAngle;
    private float elapsed;

    public void Initialize(Transform owner, OrbitSkillData skillData)
    {
        Owner = owner;
        data = skillData;
        elapsed = 0f;

        transform.position = GetOrbitCenter();

        int count = Mathf.Max(1, data.orbCount);
        for (int i = 0; i < count; i++)
        {
            GameObject orbObj = Instantiate(data.orbPrefab, transform.position, Quaternion.identity, transform);
            OrbitOrb orb = orbObj.GetComponent<OrbitOrb>();
            if (orb != null)
            {
                orb.Initialize(Owner, data);
                orb.SetLifecycleScale(0f);
                orbs.Add(orb);
            }
        }

        UpdateOrbs(0f, 0f);
    }

    private void LateUpdate()
    {
        if (Owner == null || data == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.deltaTime;
        if (elapsed >= data.duration)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = GetOrbitCenter();

        float radiusFactor = 1f;
        float scaleFactor = 1f;
        float spinMultiplier = 1f;

        if (data.spawnDuration > 0f && elapsed < data.spawnDuration)
        {
            float t = Mathf.Clamp01(elapsed / data.spawnDuration);
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);
            radiusFactor = easeOut;
            scaleFactor = easeOut;
            spinMultiplier = Mathf.Lerp(data.spawnSpinBoost, 1f, easeOut);
        }
        else if (data.despawnDuration > 0f && elapsed > data.duration - data.despawnDuration)
        {
            float remaining = Mathf.Max(0f, data.duration - elapsed);
            float t = Mathf.Clamp01(remaining / data.despawnDuration);
            float easeIn = t * t;
            radiusFactor = Mathf.Lerp(0.25f, 1f, easeIn);
            scaleFactor = easeIn;
        }

        currentAngle += data.orbitSpeed * spinMultiplier * Time.deltaTime;
        UpdateOrbs(radiusFactor, scaleFactor);
    }

    private Vector3 GetOrbitCenter()
    {
        return Owner.position + new Vector3(data.centerOffset.x, data.centerOffset.y, 0f);
    }

    private void UpdateOrbs(float radiusFactor, float scaleFactor)
    {
        int count = orbs.Count;
        if (count == 0) return;

        float angleStep = 360f / count;
        Vector3 center = transform.position;

        for (int i = 0; i < count; i++)
        {
            OrbitOrb orb = orbs[i];
            if (orb == null) continue;

            float rad = (currentAngle + angleStep * i) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(rad) * data.orbitRadius.x * radiusFactor,
                Mathf.Sin(rad) * data.orbitRadius.y * radiusFactor,
                0f
            );

            orb.transform.position = center + offset;
            orb.SetLifecycleScale(scaleFactor);
        }
    }
}