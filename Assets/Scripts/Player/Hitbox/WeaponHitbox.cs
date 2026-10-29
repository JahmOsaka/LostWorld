using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    private PlayerData playerData;
    private Collider2D myCollider;
    private ContactFilter2D filter;
    private readonly List<Collider2D> alreadyHitObjects = new List<Collider2D>();
    private readonly List<Collider2D> hitResults = new List<Collider2D>();

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        filter = ContactFilter2D.noFilter;

        Player player = GetComponentInParent<Player>();
        if (player != null)
        {
            playerData = player.playerData;
        }
    }

    private void OnEnable() => alreadyHitObjects.Clear();

    private void Update()
    {
        if (myCollider == null || playerData == null) return;

        myCollider.Overlap(filter, hitResults);

        foreach (Collider2D col in hitResults)
        {
            if (alreadyHitObjects.Contains(col)) continue;

            if (col.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(playerData.attackDamage);
                alreadyHitObjects.Add(col);
            }
        }
    }
}