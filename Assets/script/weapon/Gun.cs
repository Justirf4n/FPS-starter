using System.Collections;
using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected GunData gunData;

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

    protected virtual void Update() { }

    protected void TryShoot()
    {
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            TryReload();
            return;
        }

        if (Time.time < nextTimeToFire) return;

        nextTimeToFire = Time.time + (1f / gunData.fireRate);
        currentAmmo--;

        Shoot();
        Debug.Log($"{gunData.gunName} shot. Ammo left: {currentAmmo}");
    }

    protected void TryReload()
    {
        if (isReloading) return;
        if (currentAmmo == gunData.magazineSize) return;

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

    protected abstract void Shoot();
}
