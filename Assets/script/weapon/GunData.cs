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
}
