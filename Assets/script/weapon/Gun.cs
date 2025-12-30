using System.Collections;
using UnityEngine;
using Cinemachine;

public abstract class Gun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GunData gunData;
    [SerializeField] private Transform gunMuzzle;

    [Header("Muzzle Flash")]
    [SerializeField] private ParticleSystem muzzleFlash;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Recoil")]
    [SerializeField] private WeaponAnimation weaponAnimation;

    protected Transform CameraTransform => cameraTransform;

    private WaitForSeconds reloadWait;
    private CinemachineImpulseSource impulse;
    private PlayerController playerController;
    private Transform cameraTransform;

    private int currentAmmo;
    private float nextTimeToFire;
    private bool isReloading;

    #region Unity Lifecycle
    private void Start()
    {
        currentAmmo = gunData.magazineSize;

        reloadWait = new WaitForSeconds(gunData.reloadTime);
        impulse = GetComponent<CinemachineImpulseSource>();
        playerController = GetComponentInParent<PlayerController>();
        cameraTransform = playerController.vCam.transform;
        weaponAnimation = GetComponentInChildren<WeaponAnimation>();
        Debug.Assert(playerController != null, "playerController not found!");
        Debug.Assert(cameraTransform != null, "Camera Transform missing!");

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (isReloading) return;
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
        weaponAnimation.Fire();
        impulse.GenerateImpulse();
        PlayMuzzleFlash();
        PlayFireSound();
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
        yield return reloadWait;

        currentAmmo = gunData.magazineSize;
        isReloading = false;
    }
    #endregion

    #region FX
    private void PlayMuzzleFlash()
    {
        if (!muzzleFlash) return;
    
        muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        muzzleFlash.Play();
    }

    protected void SpawnHitFX(RaycastHit hit)
    {
        if (!hit.collider) return;

        // ===== BULLET HOLE =====
        GameObject hole = BulletImpactPool.Instance.GetHole();

        hole.SetActive(true);

        Debug.Log("Hole active: " + hole.activeInHierarchy);

        hole.transform.position = hit.point + hit.normal * 0.001f;
        hole.transform.rotation = Quaternion.LookRotation(-hit.normal);
        hole.transform.localScale = Vector3.one * 0.02f;
        hole.transform.SetParent(hit.collider.transform, true);
    
        // ===== HIT PARTICLE =====
        GameObject particle = BulletImpactPool.Instance.GetParticle();
        particle.SetActive(true);
        particle.transform.SetParent(null);
        particle.transform.position = hit.point + hit.normal * 0.03f;
        particle.transform.rotation = Quaternion.LookRotation(hit.normal);
    
        // ===== RESET & PLAY =====
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }
    }

    private void PlayFireSound()
    {
        if (audioSource == null) return;
        if (gunData.fireSound == null) return;

        audioSource.PlayOneShot(gunData.fireSound);
    }
    #endregion

    #region Utilities
    protected Vector3 GetShootDirection()
    {
        float spreadX = Random.Range(-gunData.spreadAngle, gunData.spreadAngle);
        float spreadY = Random.Range(-gunData.spreadAngle, gunData.spreadAngle);
        spreadX = 0;
        spreadY = 0;

        return Quaternion.Euler(spreadX, spreadY, 0f) * cameraTransform.forward;
    }

    protected Transform Muzzle => gunMuzzle;
    protected GunData Data => gunData;
    #endregion

    #region Extension Point
    protected abstract void OnShoot();
    #endregion
}
