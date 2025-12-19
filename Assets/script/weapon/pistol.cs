using System.Collections;
using UnityEngine;

public class Pistol : Gun
{   
    private Vector3 targetPoint;

    protected override void Shoot()
    {
        Vector3 shootDirection = GetSpreadDirection();
        Ray ray = new Ray(cameraTransform.position, shootDirection);

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, gunData.shootingRange, gunData.targetLayerMask))
        {
            targetPoint = hit.point;
            SpawnHitFX(hit);
            Debug.Log($"{gunData.gunName} hit {targetPoint}");
        }
        else
        {
            targetPoint = ray.origin + ray.direction * gunData.shootingRange;
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
