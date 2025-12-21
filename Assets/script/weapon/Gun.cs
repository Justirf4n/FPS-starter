using System.Collections;
using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GunData gunData;
    [SerializeField] private Transform gunMuzzle;

    [Header("Hit Effects")]
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private GameObject bulletHitParticlePrefab;

    [Header("Muzzle Flash")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private float muzzleFlashDuration = 0.05f;

    private PlayerController playerController;
    private Transform cameraTransform;

    private int currentAmmo;
    private float nextTimeToFire;
    private bool isReloading;

    #region Unity Lifecycle
    private void Start()
    {
        currentAmmo = gunData.magazineSize;

        playerController = GetComponentInParent<PlayerController>();
        cameraTransform = playerController.virtualCamera.transform;

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void Update()
    {
        HandleInput();
    }
    #endregion

    #region Input & Flow Control
    private void HandleInput()
    {
        if (Input.GetButtonDown("Fire1"))
            TryShoot();

        if (Input.GetKeyDown(KeyCode.R))
            TryReload();
    }

    private void TryShoot()
    {
        if (isReloading || Time.time < nextTimeToFire)
            return;

        if (currentAmmo <= 0)
        {
            TryReload();
            return;
        }

        nextTimeToFire = Time.time + (1f / gunData.fireRate);
        currentAmmo--;

        playerController.ApplyRecoil(gunData);
        PlayMuzzleFlash();

        OnShoot();

        Debug.Log($"{gunData.gunName} fired | Ammo: {currentAmmo}");
    }

    private void TryReload()
    {
        if (isReloading || currentAmmo == gunData.magazineSize)
            return;

        StartCoroutine(ReloadRoutine());
    }
    #endregion

    #region Reload
    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(gunData.reloadTime);

        currentAmmo = gunData.magazineSize;
        isReloading = false;
    }
    #endregion

    #region FX
    private void PlayMuzzleFlash()
    {
        if (muzzleFlash == null) return;
        if (!muzzleFlash.gameObject) return;

        muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        muzzleFlash.Play();
    }

    private IEnumerator MuzzleFlashRoutine()
    {
        muzzleFlash.Play();
        yield return new WaitForSeconds(muzzleFlashDuration);
        muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    protected void SpawnHitFX(RaycastHit hit)
    {
        Vector3 pos = hit.point + hit.normal * 0.05f;
        Quaternion rot = Quaternion.LookRotation(-hit.normal);

        GameObject hole = Instantiate(bulletHolePrefab, pos, rot, hit.collider.transform);
        GameObject particle = Instantiate(bulletHitParticlePrefab, pos, rot, hit.collider.transform);

        Destroy(hole, 5f);
        Destroy(particle, 2f);
    }
    #endregion

    #region Utilities
    protected Vector3 GetShootDirection()
    {
        float spreadX = Random.Range(-gunData.spreadAngle, gunData.spreadAngle);
        float spreadY = Random.Range(-gunData.spreadAngle, gunData.spreadAngle);

        return Quaternion.Euler(spreadX, spreadY, 0f) * cameraTransform.forward;
    }

    protected Transform Muzzle => gunMuzzle;
    protected GunData Data => gunData;
    #endregion

    #region Extension Point
    protected abstract void OnShoot();
    #endregion
}
