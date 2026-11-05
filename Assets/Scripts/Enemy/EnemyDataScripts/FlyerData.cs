using UnityEngine;

[CreateAssetMenu(fileName = "FlyerData", menuName = "Enemy/Flyer")]
public class FlyerData : EnemyData
{
    [Header("Area")]
    public float hoverRadiusX = 4f;      
    public float hoverRadiusY = 1.5f;
    public float leashRadius = 8f;      

    [Header("Follow")]
    public float followSpeed = 3f;
    public float keepDistance = 3f;      
    public float followHeight = 1.5f;    

    [Header("Charge Attack")]
    public float attackRange = 4f;       
    public float chargeWindup = 0.6f;
    public float chargeSpeed = 12f;
    public float chargeOvershoot = 1f;   
    public float chargeRecover = 0.5f;
    public float chargeCooldown = 2f;

    [Header("Death Burst")]
    public GameObject headPrefab;    
    public int burstDamage = 30;
    public float burstRadius = 2f;
    public float burstFuse = 1f;
}
