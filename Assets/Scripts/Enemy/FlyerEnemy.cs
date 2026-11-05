using UnityEngine;

public class FlyerEnemy : Enemy
{
    public FlyPatrolState FlyPatrol { get; private set; }
    public FlyFollowState FlyFollow { get; private set; }
    public FlyChargeState FlyCharge { get; private set; }

    [HideInInspector] public float nextChargeTime;

    private bool burstSpawned;

    protected override void Awake()
    {
        base.Awake();
        rb.gravityScale = 0f;
        FlyPatrol = new FlyPatrolState(this, stateMachine, enemyData, "idle");
        FlyFollow = new FlyFollowState(this, stateMachine, enemyData, "idle");
        FlyCharge = new FlyChargeState(this, stateMachine, enemyData, "charge");

        CoreHealth health = GetComponent<CoreHealth>();
        if (health != null) health.OnDeath.AddListener(SpawnBurstHead);
    }

    protected override void Start()
    {
        stateMachine.Initialize(FlyPatrol);
    }

    private void SpawnBurstHead()
    {
        if (burstSpawned) return;
        burstSpawned = true;

        FlyerData data = (FlyerData)enemyData;
        if (data.headPrefab == null) return;

        GameObject head = Instantiate(data.headPrefab, transform.position, Quaternion.identity);
        FlyerHead sporeHead = head.GetComponent<FlyerHead>();
        if (sporeHead != null) sporeHead.Init(data.burstDamage, data.burstRadius, data.burstFuse);
    }
}