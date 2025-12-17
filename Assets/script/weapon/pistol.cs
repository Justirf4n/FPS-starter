using UnityEngine;

public class Pistol : Gun
{
    protected override void Update()
    {
        base.Update();

        if (Input.GetButtonDown("Fire1"))
            TryShoot();

        if (Input.GetKeyDown(KeyCode.R))
            TryReload();
    }

    protected override void Shoot()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, gunData.shootingRange, gunData.targetLayerMask))
        {
            Debug.Log($"{gunData.gunName} hit {hit.collider.name}");
        }
    }
}
