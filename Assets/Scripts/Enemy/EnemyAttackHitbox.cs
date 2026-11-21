using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    private Enemy enemy;
    private Collider2D myCollider;
    private ContactFilter2D filter;
    private readonly List<Collider2D> alreadyHitObjects = new List<Collider2D>();
    private readonly List<Collider2D> hitResults = new List<Collider2D>();

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        filter = ContactFilter2D.noFilter;
        enemy = GetComponentInParent<Enemy>();
    }
    private void OnEnable() => alreadyHitObjects.Clear();

    private void Update()
    {
        if (myCollider == null || enemy == null || enemy.enemyData == null) return;

        myCollider.Overlap(filter, hitResults);

        foreach (Collider2D col in hitResults)
        {
            if (alreadyHitObjects.Contains(col)) continue;

            if (col.CompareTag("Player") && col.TryGetComponent(out IDamageable damageable))
            {
                if (enemy.enemyData is MeleeEnemyData meleeData)
                {
                    damageable.TakeDamage(meleeData.damage);
                    alreadyHitObjects.Add(col);
                }
            }
        }
    }
}