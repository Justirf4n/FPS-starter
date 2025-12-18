using System.Runtime;
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
        else
        {
            targetPoint = cameraTransform.position + cameraTransform.forward * gunData.shootingRange;
        }

        StartCoroutine(BulletTrailRoutine(targetPoint));
    }

    protected IEnumerator BulletTrailRoutine(Vector3 target)
    {
        GameObject bulletTrail = Instantiate(
            gunData.bulletTrailPrefab,
            gunMuzzle.position,
            Quaternion.identity
        );
    
        while (Vector3.Distance(bulletTrail.transform.position, target) > 0.05f)
        {
            bulletTrail.transform.position =
                Vector3.MoveTowards(
                    bulletTrail.transform.position,
                    target,
                    gunData.bulletSpeed * Time.deltaTime
                );
    
            yield return null;
        }
    
        Destroy(bulletTrail);
    }
}
