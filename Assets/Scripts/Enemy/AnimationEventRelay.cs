using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }
    public void NotifySpawnOrbs()
    {
        if (enemy is EliteEnemy elite)
        {
            elite.NotifySpawnOrbs();
        }
    }

}