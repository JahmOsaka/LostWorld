using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private PlayerData playerData;

    private List<Collider2D> alreadyHitObjects = new List<Collider2D>();

    private Collider2D myCollider;
    private ContactFilter2D filter;
    private List<Collider2D> hitResults = new List<Collider2D>();

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();

        filter = ContactFilter2D.noFilter;
    }

    private void OnEnable()
    {
        alreadyHitObjects.Clear();
    }

    private void Update()
    {
        myCollider.Overlap(filter, hitResults);

        foreach (Collider2D col in hitResults)
        {
            if (alreadyHitObjects.Contains(col)) continue;

            if (col.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(playerData.attackDamage);
                alreadyHitObjects.Add(col);

                Debug.Log($"Whip hit {col.name} for {playerData.attackDamage} damage!");
            }
        }
    }
}