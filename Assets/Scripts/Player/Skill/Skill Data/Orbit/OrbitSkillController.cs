using System.Collections.Generic;
using UnityEngine;

public class OrbitSkillController : MonoBehaviour
{
    public Transform Owner { get; private set; }

    private OrbitSkillData data;
    private readonly List<Transform> orbs = new List<Transform>();
    private float currentAngle;
    private float remainingTime;

    public void Initialize(Transform owner, OrbitSkillData skillData)
    {
        Owner = owner;
        data = skillData;
        remainingTime = data.duration;

        transform.position = Owner.position;

        int count = Mathf.Max(1, data.orbCount);
        for (int i = 0; i < count; i++)
        {
            GameObject orbObj = Instantiate(data.orbPrefab, transform.position, Quaternion.identity, transform);
            OrbitOrb orb = orbObj.GetComponent<OrbitOrb>();
            if (orb != null)
            {
                orb.Initialize(Owner, data);
            }
            orbs.Add(orbObj.transform);
        }

        UpdateOrbPositions();
    }

    private void LateUpdate()
    {
        if (Owner == null || data == null)
        {
            Destroy(gameObject);
            return;
        }

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Owner.position;
        currentAngle += data.orbitSpeed * Time.deltaTime;
        UpdateOrbPositions();
    }

    private void UpdateOrbPositions()
    {
        int count = orbs.Count;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            if (orbs[i] == null) continue;

            float rad = (currentAngle + angleStep * i) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * data.orbitRadius;
            orbs[i].position = transform.position + offset;
        }
    }
}