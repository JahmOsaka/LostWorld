using UnityEngine;

public class EliteTeleportState : EnemyState
{
    private enum Phase { Vanish, TravelAsOrb, Reappear }

    private EliteEnemy elite;
    private EliteEnemyData eliteData;
    private Phase phase;
    private float timer;

    private GameObject activeTravelOrb;
    private Vector3 targetRootPos;
    private Vector3 targetOrbPos;

    public EliteTeleportState(EliteEnemy enemy, EnemyStateMachine stateMachine, EnemyData enemyData, string animBoolName)
        : base(enemy, stateMachine, enemyData, animBoolName)
    {
        elite = enemy;
        eliteData = enemyData as EliteEnemyData;
    }

    public override void Enter()
    {
        base.Enter();
        phase = Phase.Vanish;
        timer = 0f;

        elite.rb.linearVelocity = Vector2.zero;
        elite.FacePlayer();
        SpawnTeleportVFX(eliteData.teleportVanishVfxPrefab, elite.BodyCenter);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        timer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Vanish:
                float fadeOut = 1f - Mathf.Clamp01(timer / eliteData.vanishDuration);
                SetAlpha(fadeOut);

                if (timer >= eliteData.vanishDuration)
                {
                    StartOrbTravel();
                    phase = Phase.TravelAsOrb;
                    timer = 0f;
                }
                break;

            case Phase.TravelAsOrb:
                if (activeTravelOrb != null)
                {
                    activeTravelOrb.transform.position = Vector3.MoveTowards(
                        activeTravelOrb.transform.position,
                        targetOrbPos,
                        eliteData.orbTravelSpeed * Time.deltaTime
                    );

                    if (Vector3.Distance(activeTravelOrb.transform.position, targetOrbPos) <= 0.05f)
                    {
                        FinishOrbTravel();
                    }
                }
                else
                {
                    FinishOrbTravel();
                }
                break;

            case Phase.Reappear:
                float fadeIn = Mathf.Clamp01(timer / eliteData.reappearDelay);
                SetAlpha(fadeIn);

                if (timer >= eliteData.reappearDelay)
                {
                    elite.nextTeleportTime = Time.time + eliteData.teleportCooldown;
                    stateMachine.ChangeState(elite.AttackState);
                }
                break;
        }
    }

    public override void Exit()
    {
        base.Exit();
        elite.SetActiveOrbsVisible(true);
        if (activeTravelOrb != null)
        {
            Object.Destroy(activeTravelOrb);
            activeTravelOrb = null;
        }
        SetVisibleAndHittable(true);
        SetAlpha(1f);
    }

    private void StartOrbTravel()
    {
        SetVisibleAndHittable(false);
        elite.SetActiveOrbsVisible(false);

        Vector3 bodyOffset = (Vector3)elite.BodyCenter - elite.transform.position;
        targetRootPos = CalculateTargetRootPosition();
        targetOrbPos = targetRootPos + bodyOffset;
        if (eliteData.teleportOrbPrefab != null)
        {
            activeTravelOrb = Object.Instantiate(eliteData.teleportOrbPrefab, elite.BodyCenter, Quaternion.identity);

            var orbDmg = activeTravelOrb.GetComponent<TeleportOrbDamage>();
            if (orbDmg != null) orbDmg.Init(eliteData.teleportOrbDamage);
        }
    }

    private void FinishOrbTravel()
    {
        if (activeTravelOrb != null)
        {
            Object.Destroy(activeTravelOrb);
            activeTravelOrb = null;
        }

        elite.transform.position = targetRootPos;
        elite.SetActiveOrbsVisible(true); 
        elite.FacePlayer();
        SetVisibleAndHittable(true);
        SpawnTeleportVFX(eliteData.teleportReappearVfxPrefab, elite.BodyCenter);

        phase = Phase.Reappear;
        timer = 0f;
    }

    private Vector3 CalculateTargetRootPosition()
    {
        if (elite.playerTransform == null) return elite.transform.position;

        Player player = elite.playerTransform.GetComponent<Player>();
        int playerFacing = (player != null)
            ? player.FacingDirection
            : (elite.playerTransform.localScale.x < 0f ? -1 : 1);

        Vector2 playerPos = elite.playerTransform.position;
        Vector2 safePos = FindSafeTeleportPosition(playerPos, playerFacing);
        return new Vector3(safePos.x, safePos.y, elite.transform.position.z);
    }

    private Vector2 FindSafeTeleportPosition(Vector2 playerPos, int dir)
    {
        if (TryGetValidSpot(playerPos, dir, out Vector2 frontSpot)) return frontSpot;
        if (TryGetValidSpot(playerPos, -dir, out Vector2 backSpot)) return backSpot;
        return new Vector2(playerPos.x, elite.transform.position.y);
    }

    private bool TryGetValidSpot(Vector2 playerPos, int dir, out Vector2 result)
    {
        Vector2 desired = playerPos + new Vector2(dir * eliteData.frontOffset, 0f);

        RaycastHit2D wallHit = Physics2D.Linecast(
            playerPos + Vector2.up * 0.5f,
            desired + Vector2.up * 0.5f,
            elite.groundLayer
        );

        float targetX = wallHit.collider != null
            ? wallHit.point.x - (dir * 0.6f)
            : desired.x;

        LayerMask walkable = elite.groundLayer | elite.oneWayPlatformLayer;
        RaycastHit2D groundHit = Physics2D.Raycast(
            new Vector2(targetX, playerPos.y + 0.5f),
            Vector2.down,
            3f,
            walkable
        );

        if (groundHit.collider != null)
        {
            result = new Vector2(targetX, elite.transform.position.y);
            return true;
        }

        result = Vector2.zero;
        return false;
    }

    private void SetVisibleAndHittable(bool active)
    {
        if (elite.spriteRenderer != null) elite.spriteRenderer.enabled = active;
        if (elite.hitboxCollider != null) elite.hitboxCollider.enabled = active;
    }

    private void SetAlpha(float a)
    {
        if (elite.spriteRenderer == null) return;
        Color c = elite.spriteRenderer.color;
        c.a = a;
        elite.spriteRenderer.color = c;
    }

    private void SpawnTeleportVFX(GameObject prefab, Vector3 pos)
    {
        if (prefab != null)
        {
            GameObject vfx = Object.Instantiate(prefab, pos, Quaternion.identity);
            Object.Destroy(vfx, 1f);
        }
    }
}