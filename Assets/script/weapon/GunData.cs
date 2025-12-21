using UnityEngine;

[CreateAssetMenu(fileName = "NewGunData", menuName = "Weapons/Gun Data")]
public class GunData : ScriptableObject
{
    [Header("Basic Info")]
    public string gunName;

    [Header("Target")]
    public LayerMask targetLayerMask;

    [Header("Fire Config")]
    public float shootingRange = 50f;
    public float fireRate = 5f;
    public int damage = 25;

    [Header("Reload Config")]
    public int magazineSize = 12;
    public float reloadTime = 1.5f;

    [Header("Recoil Config")]
    [Tooltip("X = Vertical kick, Y = Horizontal kick")]
    public Vector2 recoilKick = new Vector2(1f, 2f);

    [Tooltip("How fast recoil is applied")]
    public float recoilKickSpeed = 25f;

    [Tooltip("How fast recoil returns to center")]
    public float recoilReturnSpeed = 15f;

    [Header("VFX")]
    public GameObject bulletTrailPrefab;
    public float bulletSpeed = 120f;

    [Header("Accuracy")]
    public float spreadAngle = 1.5f;
}