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

    protected virtual void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            TryShoot();

        if (Input.GetKeyDown(KeyCode.R))
            TryReload();
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

    protected abstract void Shoot();
}
