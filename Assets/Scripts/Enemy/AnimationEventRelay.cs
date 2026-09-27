using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }
    public void NotifySpawnOrbs() => enemy.NotifySpawnOrbs();

}