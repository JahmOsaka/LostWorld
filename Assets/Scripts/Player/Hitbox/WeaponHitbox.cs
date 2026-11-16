using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    private PlayerData playerData;
    private Collider2D myCollider;
    private ContactFilter2D filter;
    private readonly List<Collider2D> alreadyHitObjects = new List<Collider2D>();
    private readonly List<Collider2D> hitResults = new List<Collider2D>();

    private Player player;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        filter = ContactFilter2D.noFilter;

        player = GetComponentInParent<Player>();
        if (player != null)
        {
            playerData = player.playerData;
        }

        if (myCollider != null) myCollider.enabled = false;
    }

    private void OnEnable()
    {
        alreadyHitObjects.Clear();
        if (myCollider != null) myCollider.enabled = true;
    }

    private void OnDisable()
    {
        if (myCollider != null) myCollider.enabled = false;
    }

    private void FixedUpdate()
    {
        if (myCollider == null || !myCollider.enabled || playerData == null) return;

        myCollider.Overlap(filter, hitResults);

        foreach (Collider2D col in hitResults)
        {
            if (alreadyHitObjects.Contains(col)) continue;

            if (col.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(playerData.attackDamage);
                alreadyHitObjects.Add(col);

                TriggerHitstopEffect();
            }
        }
    }

    private void TriggerHitstopEffect()
    {
        if (GameFeelManager.Instance == null || playerData == null) return;

        int currentCombo = (player != null) ? player.CombatData.ComboCounter : 1;
        float hitstopTime = 0f;

        if (currentCombo == 1)
        {
            hitstopTime = playerData.hitstopHit1;
        }
        else if (currentCombo == 2)
        {
            hitstopTime = playerData.hitstopHit2;
        }
        else if (currentCombo >= 3)
        {
            hitstopTime = playerData.hitstopHit3;

            if (Camera.main != null)
            {
                GameFeelManager.Instance.TriggerShake(playerData.screenShakeDuration, playerData.screenShakeMagnitude, Camera.main.transform);
            }
        }

        if (hitstopTime > 0f)
        {
            GameFeelManager.Instance.TriggerHitstop(hitstopTime);
        }
    }
}