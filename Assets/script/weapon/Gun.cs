using System.Collections;
using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    [Header("Hit Effects")]
    [SerializeField] protected GameObject bulletHolePrefab;
    [SerializeField] protected GameObject bulletHitParticlePrefab;

    [Header("References")]
    [SerializeField] protected GunData gunData;

    [SerializeField] protected Transform gunMuzzle;

    protected PlayerController playerController;
    protected Transform cameraTransform;

    protected int currentAmmo;
    protected float nextTimeToFire;
    protected bool isReloading;

    protected virtual void Start()
    {
        currentAmmo = gunData.magazineSize;

        playerController = GetComponentInParent<PlayerController>();
        cameraTransform = playerController.virtualCamera.transform;
    }

    protected virtual void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            TryShoot();

        if (Input.GetKeyDown(KeyCode.R))
            TryReload();
    }

    protected Vector3 GetSpreadDirection()
    {
        float spreadX = Random.Range(-gunData.spreadAngle, gunData.spreadAngle);
        float spreadY = Random.Range(-gunData.spreadAngle, gunData.spreadAngle);

        Quaternion spreadRotation = Quaternion.Euler(spreadX, spreadY, 0f);

        return spreadRotation * cameraTransform.forward;
    }

    protected void TryShoot()
    {
        if (isReloading || Time.time < nextTimeToFire) return;

        if (currentAmmo <= 0)
        {
            TryReload();
            return;
        }

        nextTimeToFire = Time.time + (1f / gunData.fireRate);
        currentAmmo--;

        playerController.ApplyRecoil(gunData);
        Shoot();

        Debug.Log($"{gunData.gunName} fired | Ammo: {currentAmmo}");
    }

    protected void TryReload()
    {
        if (isReloading || currentAmmo == gunData.magazineSize) return;
        StartCoroutine(ReloadRoutine());
    }

    protected IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log($"{gunData.gunName} reloading...");

        yield return new WaitForSeconds(gunData.reloadTime);

        currentAmmo = gunData.magazineSize;
        isReloading = false;

        Debug.Log($"{gunData.gunName} reloaded");
    }

    protected void SpawnHitFX(RaycastHit hit)
    {
        Vector3 hitPosition = hit.point + hit.normal * 0.01f;
        Quaternion hitRotation = Quaternion.LookRotation(hit.normal);

        GameObject hole = Instantiate(bulletHolePrefab, hitPosition, hitRotation);
        GameObject particle = Instantiate(bulletHitParticlePrefab, hitPosition, hitRotation);

        hole.transform.SetParent(hit.collider.transform);
        particle.transform.SetParent(hit.collider.transform);

        Destroy(hole, 5f);
        Destroy(particle, 2f);
    }

    protected abstract void Shoot();
}
